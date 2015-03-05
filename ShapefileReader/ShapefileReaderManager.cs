using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//The ShapefileMain Format According to ESRI ShapefileMain Technical Description, July 1998
//Available from http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf
namespace GISData.ShapefileFormat
{
    public static class ShapefileReaderManager
    {

        private const int HeaderLengthInBytes = 100;

        public static List<ShapefileReader> Readers { get; private set; }

        private static List<Exception> ReaderExceptions;

        private static string[] GetMainShapefilesInDirectory(string directory)
        {
            return Directory.GetFiles(directory, ".shp");
        }

        private static bool isValidHeader(byte[] header)
        {
            byte[] fileCodeBytes = new byte[] { header[3], header[2], header[1], header[0] };
            return BitConverter.ToInt32(fileCodeBytes, 0) == 9994 && header.Length >= 100;
        }

        //This function is used to try to prevent multiple readers being opened up for the same shapefile.
        private static bool HasReaderForPath(string shapefilePath)
        {
            foreach (ShapefileReader reader in Readers)
            {
                if (reader!=null && reader.Path == shapefilePath)
                {
                    return true;
                }
            }
            return false;
        }

        private static byte[] ReadBytesFromStream(FileStream readStream, int numBytesRequested)
        {
            byte[] bufferBytes = new byte[numBytesRequested];
            if (readStream != null)
            {
                if (readStream.CanRead)
                {
                    int totalBytesRead = 0;
                    int bytesRead = 0;
                    do
                    {
                        bytesRead = readStream.Read(bufferBytes, totalBytesRead, numBytesRequested - totalBytesRead);
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

                }
                else
                {
                    throw new IOException("Unable to read from stream: ." + readStream.Name);
                }
            }
            else
            {
                throw new NullReferenceException("Stream is null and must be initialized before use.");
            }
            return bufferBytes;
        }

        //There are currently 14 different shape types defined by version 1 of the shapefile specification
        //The shape type is encoded as an integer at byte 32 in the shapefile main file (.shp) header.
        public enum ShapeTypes
        {
            Null = 0,
            Point = 1,
            PolyLine = 3,
            Polygon = 5,
            MultiPoint = 8,
            PointZ = 11,
            PolyLineZ = 13,
            PolygonZ = 15,
            MultiPointZ = 18,
            PointM = 21,
            PolyLineM = 23,
            PolygonM = 25,
            MultiPointM = 28,
            MultiPatch = 31
        }

        public static Exception[] Exceptions
        {
            get
            {
                return ReaderExceptions.ToArray();
            }
        }

        public static ShapefileReader AddReader(string path)
        {
            ShapefileReader reader = new ShapefileReader(path);
            ShapefileReaderManager.Readers.Add(reader);
            return reader;
        }

        public static ShapefileHeader[] GetAllMainHeadersInDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                string[] allShpFiles = Directory.GetFiles(directory, "*.shp");
                List<ShapefileHeader> headers = new List<ShapefileHeader>();
                foreach (string file in allShpFiles)
                {
                    ShapefileReader sfr = new ShapefileReader(file);
                    if (sfr.ReadMainHeader(false))
                    {
                        headers.Add(sfr.Header);
                    }
                }
                return headers.ToArray();
            }
            else
            {
                return new ShapefileHeader[] { };
            }
        }


        public static ShapefileHeader GetHeader(FileStream stream)
        {
            ShapefileHeader shpfileHeader = null;
            try
            {
                stream.Position = 0;
                shpfileHeader = new ShapefileHeader(ReadBytesFromStream(stream, HeaderLengthInBytes));
            }
            catch(Exception e)
            {
                ReaderExceptions.Add(e);
                shpfileHeader = null;
            }
            return shpfileHeader;
        }



        public static List<ShapefileRecord> GetAllMainRecords(FileStream stream)
        {
            List<ShapefileRecord> records = new List<ShapefileRecord>();
            byte[] streamBuffer;
            try
            {
                stream.Position = HeaderLengthInBytes;
                do
                {
                    streamBuffer = ReadBytesFromStream(stream, 8);
                    if (streamBuffer.Length < 8)
                    {
                        break;
                    }
                    else
                    {
                        ShapefileRecord myRecord = new ShapefileRecord(streamBuffer);
                        myRecord.SetRecordContents(ReadBytesFromStream(stream, myRecord.ContentLength * 2));
                        records.Add(myRecord);
                    }
                } while (streamBuffer.Length > 0);
            }
            catch
            {
                //Clear the list so it is returned empty.
                records.Clear();
            }
            return records;
        }

        public static List<ShapefileIndexRecord> GetAllIndexRecords(FileStream stream)
        {
            List<ShapefileIndexRecord> records = new List<ShapefileIndexRecord>();
            byte[] streamBuffer;
            try
            {
                stream.Position = 100;
                do
                {
                    streamBuffer = ReadBytesFromStream(stream, 8);
                    if (streamBuffer.Length < 8)
                    {
                        break;
                    }
                    else
                    {
                        ShapefileIndexRecord myRecord = new ShapefileIndexRecord(streamBuffer);
                        records.Add(myRecord);
                    }
                } while (streamBuffer.Length > 0);
            }
            catch
            {
                records.Clear();
            }
            return records;

        }

        public static ShapefileReader ReadShapefile(string path)
        {
            ShapefileReader shpfileReader = null;
            try
            {
                if (ValidShapefileParts(path))
                {
                    shpfileReader = new ShapefileReader(path, true);
                    Readers.Add(shpfileReader);
                }
                else
                {
                    throw new Exception("Missing shapefile components at " + path);
                }
            }
            catch(Exception e)
            {
                ReaderExceptions.Add(e);
                shpfileReader = null;
            }
            finally
            {
                if (shpfileReader != null)
                {
                    shpfileReader.CloseFileStream();
                }
            }
            return shpfileReader;
        }

        public static int RemoveDisposedReaders()
        {
            int removeCount = 0;
            for (int i = 0; i < Readers.Count;i++)
            {
                if (Readers[i] == null || Readers[i].isDisposed)
                {
                    Readers.RemoveAt(i--);
                    removeCount++;
                }
            }

            return removeCount;
        }

        public static string[] ActiveReaderPaths()
        {
            List<string> readerPaths = new List<string>();
            foreach (ShapefileReader reader in Readers)
            {
                if (reader != null && reader.isDisposed == false)
                {
                    readerPaths.Add(reader.Path);
                }
            }
            return readerPaths.ToArray();
        }

        public static ShapefileReader GetReaderForPath(string path){
            ShapefileReader reader = null;
            if (ActiveReaderPaths().Contains(path))
            {
                //Not actually working.
                return Readers[0];
            }
            return reader;
        }

        public static bool CloseReader(string path)
        {
            ShapefileReader reader = GetReaderForPath(path);
            if (reader != null)
            {
                return CloseReader(reader);
            }
            else
            {
                return false;
            }
        }

        public static bool CloseReader(ShapefileReader reader)
        {
            if (Readers.Contains(reader))
            {
                reader.Dispose();
                Readers.Remove(reader);
                return true;
            }
            else
            {
                return false;
            }
        }

        //Given a path to a shapefile, see if the three main file components (.shp,.shx, and .dbf) are present in the specified directory
        //We don't want to waste time trying to read shapefile that is not complete.
        public static bool ValidShapefileParts(string pathToShapefile)
        {
            string extension = Path.GetExtension(pathToShapefile);
            string filenameNoExtension = Path.GetFileNameWithoutExtension(pathToShapefile);

            if (extension == "" || extension == ".shp" || extension == ".shx" || extension == ".dbf")
            {
                string directory = Path.GetDirectoryName(pathToShapefile);
                if (Directory.Exists(directory))
                {
                    string[] allMatchingFiles = Directory.GetFiles(directory, filenameNoExtension + ".*");
                    string[] allMatchingExtensions = new string[allMatchingFiles.Length];
                    for (int i = 0; i < allMatchingFiles.Length; i++)
                    {
                        allMatchingExtensions[i] = Path.GetExtension(allMatchingFiles[i]);
                    }
                    if (allMatchingExtensions.Contains(".shp") && allMatchingExtensions.Contains(".shx") && allMatchingExtensions.Contains(".dbf"))
                    {
                        //Return true because all three main files were present.
                        return true;
                    }
                    else
                    {
                        //Return false because the three main files (.shp,.shx,.dbf) were not ALL present.
                        return false;
                    }
                }
                else
                {
                    //Return false because directory to file did not exist
                    return false;
                }
            }
            else
            {
                //Return false because bad extension was provided on filename
                return false;
            }
        }

        //Static constructor to initialize the Readers and ReaderExceptions Lists.
        static ShapefileReaderManager()
        {
            Readers = new List<ShapefileReader>();
            ReaderExceptions = new List<Exception>();
        }

    }


}
