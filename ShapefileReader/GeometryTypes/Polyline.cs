using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISData.GeometryTypes
{
    public class Polyline
    {
        public Multipoint[] Parts { get; private set; }
        public Polyline(Multipoint[] parts)
        {
            this.Parts = parts;
        }
        public Polyline(Multipoint part)
        {
            this.Parts = new Multipoint[] { part };
        }
    }
}
