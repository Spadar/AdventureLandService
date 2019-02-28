using AdventureLandLibrary.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureLandLibrary.GameObjects
{
    public class Entity
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string mtype { get; set; }
        public bool citizen { get; set; }
        public string map { get; set; }
        public int max_hp { get; set; }
        public int hp { get; set; }
        public double real_x { get; set; }
        public double real_y { get; set; }
        public double range { get; set; }

        public List<Point> GetBoundary()
        {
            var points = new List<Point>();
            for(var i = 0d; i < 2*Math.PI; i += (2*Math.PI)/30)
            {
                var x = Math.Cos(i) * range + real_x;
                var y = Math.Sin(i) * range + real_y;

                var point = new Point((int)x, (int)y);

                points.Add(point);
            }

            return points;
        }
    }
}
