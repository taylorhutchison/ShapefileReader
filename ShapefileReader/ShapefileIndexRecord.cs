using System;
using System.Collections.Generic;

namespace GISData.ShapefileFormat
{
    public class ShapefileIndexRecord
    {
        public int Offset { get; private set; }
        public int Length { get; private set; }

        public ShapefileIndexRecord(byte[] recordHeader)
        {
            if (recordHeader.Length == 8)
            {
                Array.Reverse(recordHeader);
                this.Length = BitConverter.ToInt32(recordHeader, 0);
                this.Offset = BitConverter.ToInt32(recordHeader, 4);
            }
        }
    }
}
