using System;
using System.Collections.Generic;

namespace GISData.ShapefileFormat
{
    public class ShapefileRecord
    {
        public int RecordNumber { get; set; }
        public int ContentLength { get; set; }
        public byte[] RecordContents { get; private set; }

        public bool SetRecordContents(byte[] contents)
        {
            if (contents.Length == ContentLength * 2)
            {
                RecordContents = contents;
                return true;
            }
            else
            {
                return false;
            }
        }

        public ShapefileRecord(byte[] recordContents, bool noHeader)
        {
            this.RecordContents = recordContents;
        }

        public ShapefileRecord(byte[] recordHeader)
        {
            if (recordHeader.Length == 8)
            {
                //Reverse array because record headers are stored in Big-Endian notation.
                Array.Reverse(recordHeader);

                //Content length is measured in 16-bit words.
                this.ContentLength = BitConverter.ToInt32(recordHeader, 0);

                this.RecordNumber = BitConverter.ToInt32(recordHeader, 4);
                this.RecordContents = new byte[ContentLength * 2];
            }

        }

    }
}
