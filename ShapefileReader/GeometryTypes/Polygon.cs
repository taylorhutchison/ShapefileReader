using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISData.GeometryTypes
{
    public class Polygon:Polyline
    {
        public Polygon(Multipoint[] parts)
            :base(parts)
        {

        }
        public Polygon(Multipoint part)
            : base(part)
        {

        }
    }
}
