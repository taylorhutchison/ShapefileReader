using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISData.GeometryTypes
{
    public class Multipoint
    {
        public Point[] Points { get; private set; }
        public Multipoint(Point[] points)
        {
            this.Points = points;
        }
    }
}
