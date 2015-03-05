using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISData.ShapefileFormat
{
    public class Dbase
    {
        public int Version { get; private set; }
        public string LastUpdated { get; private set; }
        public int RecordCount { get; private set; }
        public int HeaderLength { get; private set; }
        public int FieldCount
        {
            get
            {
                return (HeaderLength - 33) / 32;
            }
        }
        public int RecordLength { get; private set; }

        public string[] FieldNames { get; private set; }
        public string[] FieldTypes { get; private set; }

        private void GetFieldsFromHeader(byte[] tableHeader){
            if (tableHeader.Length >= this.HeaderLength)
            {
                this.FieldNames = new string[this.FieldCount];
                this.FieldTypes = new string[this.FieldCount];
                for (int i = 1; i <= this.FieldCount; i++)
                {
                    this.FieldNames[i - 1] = System.Text.ASCIIEncoding.ASCII.GetString(tableHeader, i * 32, 10);
                    this.FieldTypes[i - 1] = System.Text.ASCIIEncoding.ASCII.GetString(tableHeader, (i * 32) + 11, 1);
                }
            }
            else
            {
                this.FieldNames = new string[0];
                this.FieldTypes = new string[0];
            }
        }

        public Dbase(byte[] tableHeader)
        {
            if (tableHeader != null && tableHeader.Length>=32)
            {
                this.Version = tableHeader[0];
                this.LastUpdated = tableHeader[1].ToString() + tableHeader[2].ToString() + tableHeader[3].ToString();
                this.RecordCount = BitConverter.ToInt32(tableHeader, 4);
                this.HeaderLength = BitConverter.ToInt16(tableHeader, 8);
                this.RecordLength = BitConverter.ToInt16(tableHeader, 10);
                this.GetFieldsFromHeader(tableHeader);
            }
        }
    }
}
