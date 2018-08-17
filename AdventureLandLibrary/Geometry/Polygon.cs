using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriangleNet.Geometry;

namespace AdventureLandLibrary.Geometry
{
    public class Polygon
    {
        public PolygonPart[] parts;

        public TriangleNet.Geometry.Polygon polygon;

        public Polygon(PolygonPart[] parts)
        {
            this.parts = parts;
            polygon = new TriangleNet.Geometry.Polygon(parts.Length);

            foreach(var part in parts)
            {
                polygon.Add(part.contour, part.IsHole);
            }
        }
    }
}
