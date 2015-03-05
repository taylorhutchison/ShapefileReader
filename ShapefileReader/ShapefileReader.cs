using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GISData.ShapefileFormat
{
    public class ShapefileReader : IDisposable
    {
        public ShapefileMain Shpfile { get; private set; }
        public List<ShapefileIndexRecord> Index { get; private set; }
        public Dbase Table { get; private set; }
        public ShapefileProjection Projection { get; private set; } //Geographic projection information.
        public FileStream readStream { get; set; } //Only one stream per reader.
        public string Path { get; private set; } //The path to the shapefile without any extension.
        public bool HasMainFiles { get; private set; } //Are .shp, .shx, and .dbf present in directory?
        public bool isDisposed { get; private set; } //Check if reader is disposed during garabge collection.
        public int CurrentRecord { get; private set; } //State variable for iterating through main records.

        public ShapefileHeader Header
        {
            get
            {
                return this.Shpfile.Header;
            }
        }

        public ShapefileReaderManager.ShapeTypes ShapeType
        {
            get
            {
                return (ShapefileReaderManager.ShapeTypes)this.Shpfile.Header.ShapeType;
            }
        }

        public List<ShapefileRecord> Records
        {
            get
            {
                return this.Shpfile.Records;
            }
        }

        public int RecordCount
        {
            get
            {
                if (this.Shpfile != null && this.Index != null)
                {
                    return this.Index.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        private byte[] ReadBytesFromOpenStream(long startPosition, int numBytesRequested)
        {
            byte[] bufferBytes = new byte[numBytesRequested];
            int totalBytesRead = 0;
            int bytesRead = 0;
            do
            {
                bytesRead = this.readStream.Read(bufferBytes, totalBytesRead, numBytesRequested - totalBytesRead);
                if (bytesRead == 0)
                {
                    byte[] resizedBufferBytes = new byte[totalBytesRead];
                    Array.Copy(bufferBytes, resizedBufferBytes, totalBytesRead);
                    return resizedBufferBytes;
                }
                else
                {
                    totalBytesRead += bytesRead;
                }
            } while (totalBytesRead < numBytesRequested);
            return bufferBytes;
        }

        private bool IsStreamReadable()
        {
            if (this.readStream == null)
            {
                return false;
            }
            else
            {
                if (this.readStream.CanRead)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool OpenFileStream(string pathToShapefile)
        {
            try
            {
                this.readStream = new FileStream(pathToShapefile, FileMode.Open, FileAccess.Read, FileShare.Read);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private byte[] GetNextRecord(long recordOffset, int recordLength)
        {
            if (this.readStream.CanRead)
            {
                if (this.readStream.Position != recordOffset)
                {
                    this.readStream.Position = recordOffset;
                }
                byte[] record = this.ReadBytesFromOpenStream(recordOffset, recordLength);
                if (record!=null && record.Length == recordLength)
                {
                    return record;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public byte[] GetNextRecord()
        {
            if (this.Index!=null && this.Index.Count>0)
            {
                this.CurrentRecord++;
                if (CurrentRecord > Index.Count)
                {
                    return null;
                }
                return GetNextRecord(Index[CurrentRecord-1].Offset*2+8, Index[CurrentRecord-1].Length*2);
            }
            else
            {
                if (this.readStream.Position < 100)
                {
                    this.readStream.Position = 100;
                }
                byte[] recordHeader = this.ReadBytesFromOpenStream((int)readStream.Position, 8);
                if (recordHeader == null || recordHeader.Length < 8)
                {
                    return null;
                }
                Array.Reverse(recordHeader);

                int recordLength = BitConverter.ToInt32(recordHeader, 0);
                return GetNextRecord(readStream.Position, recordLength*2);
            }
        }

        public bool ReadShapefile()
        {
            if (this.HasMainFiles)
            {
                if (this.ParseMainFile() && this.ParseIndexFile() && this.ParseDbaseFile())
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        public bool ReadMainHeader(bool keepOpen)
        {
            this.OpenFileStream(this.Path + ".shp");
            if (IsStreamReadable())
            {
                this.Shpfile.SetHeader(ShapefileReaderManager.GetHeader(this.readStream));
                if (!keepOpen)
                {
                    this.CloseFileStream();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool OpenComponentFile(string fileExtension)
        {
            if (fileExtension.IndexOf(".") == -1)
            {
                fileExtension = "." + fileExtension;
            }
            return this.OpenFileStream(this.Path + fileExtension.ToLower());
        }

        public bool ReadMainHeader()
        {
            return ReadMainHeader(false);
        }

        public bool CloseFileStream()
        {
            if (this.readStream != null)
            {
                this.readStream.Close();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ParseMainFile()
        {
            this.OpenFileStream(this.Path + ".shp");
            if (IsStreamReadable())
            {
                this.Shpfile.SetHeader(ShapefileReaderManager.GetHeader(this.readStream));
                this.Shpfile.SetRecords(ShapefileReaderManager.GetAllMainRecords(this.readStream));
                this.CloseFileStream();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ParseIndexFile()
        {
            this.OpenFileStream(this.Path + ".shx");
            if (IsStreamReadable())
            {
                this.Index = (ShapefileReaderManager.GetAllIndexRecords(this.readStream));
                this.CloseFileStream();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ParseDbaseFile()
        {
            this.OpenFileStream(this.Path + ".dbf");
            if (IsStreamReadable())
            {
                this.CloseFileStream();
                return true;

            }
            else
            {
                return false;
            }
        }

        ~ShapefileReader()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                try
                {
                    if (this.IsStreamReadable())
                    {
                        this.CloseFileStream();
                    }

                }
                finally
                {
                    this.isDisposed = true;
                }
                GC.SuppressFinalize(this);
            }
        }

        public ShapefileReader(string pathToShapefile)
        {
            //Ternary operation to get just the full path to the shapefile WITHOUT an extension. 
            this.Path = System.IO.Path.HasExtension(pathToShapefile) ? pathToShapefile.Substring(0, pathToShapefile.Length - (System.IO.Path.GetExtension(pathToShapefile).Length)) : pathToShapefile;

            this.HasMainFiles = ShapefileReaderManager.ValidShapefileParts(this.Path);

            this.Shpfile = new ShapefileMain();

            this.CurrentRecord = 0;

            this.isDisposed = false;
        }

        public ShapefileReader(string pathToShapefile, bool open)
            : this(pathToShapefile)
        {
            if (open == true)
            {
                this.ReadShapefile();
            }
        }
    }
}
