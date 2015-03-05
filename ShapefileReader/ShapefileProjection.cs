using System;

namespace GISData.ShapefileFormat
{
    public class ShapefileProjection
    {
        public int SRID { get; private set; }
        public string Name {get; private set;}
        public string Datum { get; private set; }
        public double FalseEasting {get; private set;}
        public double FalseNorthing {get; private set;}
        public double CentralMeridian {get; private set;}

    }
}
