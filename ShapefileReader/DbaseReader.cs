using System;
using System.IO;

namespace GISData.ShapefileFormat
{

    public class DbaseReader
    {
        public Exception CurrentException { get; private set; }

        public FileStream Stream { get; private set; }
        
        private byte[] readBytesFromStream(int streamPosition, int numberOfBytes){
            byte[] buffer = new byte[numberOfBytes];
            try
            {
                this.Stream.Position = streamPosition;
                this.Stream.Read(buffer, 0, numberOfBytes);
            }
            catch (Exception e)
            {
                this.CurrentException = e;
            }
            return buffer;
        }
        public byte[] getNBytes(int count){
            return readBytesFromStream(0, count);
        }

        public byte[] getNBytes(int position, int count){
            return readBytesFromStream(position, count);
        }
        public int GetHeaderLength()
        {
            try
            {
                this.Stream.Position = 0;
                byte[] fileHeaderBytes = getNBytes(32);
                return BitConverter.ToInt16(fileHeaderBytes, 8);
            }
            catch (Exception e)
            {
                this.CurrentException = e;
            }
            return 0;
        }

        public DbaseReader(string path)
        {
            try
            {
                this.Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch(Exception e)
            {
                this.CurrentException = e;
            }
        }
    }
}
