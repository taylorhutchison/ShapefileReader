using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISData.ShapefileFormat
{
    public static class DbaseTasks
    {
        public static void OutputDbaseHeader(byte[] dbfBytes)
        {
            if (dbfBytes.Length >= 29)
            {
                Console.WriteLine("Records: {0}", BitConverter.ToInt32(dbfBytes, 4));
                Console.WriteLine("Bytes in the Header: {0}", BitConverter.ToInt16(dbfBytes, 8));
                Console.WriteLine("Bytes in the Record: {0}", BitConverter.ToInt16(dbfBytes, 10));
                Console.WriteLine("Imcomplete Transaction: {0}", BitConverter.ToBoolean(dbfBytes, 14));
                Console.WriteLine("Encrypted: {0}", BitConverter.ToBoolean(dbfBytes, 14));
                Console.WriteLine("Language Driver ID: {0}", dbfBytes[29]);
            }
            else
            {
                Console.WriteLine("Not enough bytes provided to output dbase header info.");
            }
        }

        public static void GetAllFields(byte[] dbfHeaderFields)
        {
            if (dbfHeaderFields.Length % 32 != 0)
            {
                Console.WriteLine("Incorrect number of bytes provided. {0} is not evenly divisible by 32", dbfHeaderFields.Length);
            }
            else
            {
                for (int i = 0; i < dbfHeaderFields.Length / 32; i++)
                {
                    DbaseTasks.OutputDbaseFieldDescriptor(dbfHeaderFields,i);
                }
            }
        }

        public static void OutputDbaseFieldDescriptor(byte[] dbfFieldBytes, int fieldNum)
        {
            if (dbfFieldBytes.Length >= 32)
            {
                
                Console.WriteLine("Field {0}", fieldNum);
                Console.WriteLine(System.Text.Encoding.ASCII.GetString(dbfFieldBytes,0 + (fieldNum * 32),10));
                Console.WriteLine(System.Text.Encoding.ASCII.GetString(dbfFieldBytes, 11 + (fieldNum * 32), 1));
            }
            else
            {
                Console.WriteLine("Not enough bytes provided to output dbase field descriptor.");
            }
        }
    }
}
