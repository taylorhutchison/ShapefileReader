using System;
using System.Collections.Generic;

namespace GISData.ShapefileFormat
{

    public class ShapefileMain
    {
        public ShapefileHeader Header { get; private set; }
        public List<ShapefileRecord> Records { get; private set; }

        public int SizeInBytes
        {
            get
            {
                if (Header != null)
                {
                    return Header.FileLength * 2;
                }
                else
                {
                    return -1;
                }
            }
        }

        public bool SetHeader(ShapefileHeader header)
        {
            if (header != null && this.Header==null)
            {
                this.Header = header;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SetRecords(List<ShapefileRecord> records)
        {
            if (records != null && records.Count > 0)
            {
                this.Records = records;
                return true;
            }
            else
            {
                return false;
            }
        }


    }

}
