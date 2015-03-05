using System;
using System.Collections.Generic;

namespace GISData.ShapefileFormat
{
    public class ShapefileHeader
    {
        //That ShapefileMain Header defines the basic file 

        //The first four bytes of a shapefile define the "file code".
        //This file code will always be 9994 for the ESRI ShapefileMain Format.
        //This const int is used to validate the byte[].
        public int FileLength { get; set; }
        public int Version { get; set; }
        public int ShapeType { get; set; }
        public double XMin { get; set; }
        public double YMin { get; set; }
        public double XMax { get; set; }
        public double YMax { get; set; }
        public double ZMin { get; set; }
        public double ZMax { get; set; }
        public double MMin { get; set; }
        public double MMax { get; set; }

        public ShapefileHeader()
        {
            //Empty constructor
        }

        public ShapefileHeader(byte[] headerBytes)
        {
            if (headerBytes.Length >= 100)
            {
                //The file length of a shapefile is stored as an Integer starting at byte 24.
                //It is stored in big endian byte order so that is why it is fed backwards into
                //the new byte array. this of course would fail to produce the correct integer
                //on a big endian processor. All other data that we want from the header is in
                //little endian byte order so creating a reverse order byte is unnecessary.
                this.FileLength = BitConverter.ToInt32(new byte[4] { headerBytes[27], headerBytes[26], headerBytes[25], headerBytes[24] }, 0);

                //Take 4 or 8 bytes and convert it to a int/double.
                this.Version = BitConverter.ToInt32(headerBytes, 28);
                this.ShapeType = BitConverter.ToInt32(headerBytes, 32);
                this.XMin = BitConverter.ToDouble(headerBytes, 36);
                this.YMin = BitConverter.ToDouble(headerBytes, 44);
                this.XMax = BitConverter.ToDouble(headerBytes, 52);
                this.YMax = BitConverter.ToDouble(headerBytes, 60);
                this.ZMin = BitConverter.ToDouble(headerBytes, 68);
                this.ZMax = BitConverter.ToDouble(headerBytes, 76);
                this.MMin = BitConverter.ToDouble(headerBytes, 84);
                this.MMax = BitConverter.ToDouble(headerBytes, 92);
            }
        }

    }
}
