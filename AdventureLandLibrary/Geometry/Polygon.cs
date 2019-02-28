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
        public List<PolygonPart> parts;

        public TriangleNet.Geometry.Polygon polygon;

        public Polygon(List<PolygonPart> parts)
        {
            this.parts = parts;
            polygon = new TriangleNet.Geometry.Polygon(parts.Count);

            foreach(var part in parts)
            {
                polygon.Add(part.contour, part.IsHole);
            }
        }

        public void RegeneratePolygon()
        {
            polygon = new TriangleNet.Geometry.Polygon(parts.Count);
            foreach (var part in parts)
            {
                if(part.IsHole)
                {
                    polygon.Add(part.contour, part.IsHole);
                }
            }

            foreach (var part in parts)
            {
                if (!part.IsHole)
                {
                    polygon.Add(part.contour, part.IsHole);
                }
            }
        }
    }
}
