using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISData.GeometryTypes
{
    public class GeometryManager<T>
    {
        
        public string Name { get; private set; }

        public Type GeometryType { get {
            return typeof(T);
            }
        }

        public List<T> Records { get; private set; }

        public GeometryManager(string name)
        {
            this.Name = name;
        }
    }
}
