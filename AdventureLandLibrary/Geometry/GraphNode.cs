using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriangleNet.Topology;

namespace AdventureLandLibrary.Geometry
{
    public class GraphNode
    {
        Triangle triangle;
        public PointStruct center;
        public GraphNode(TriangleNet.Topology.Triangle triangle)
        {
            this.triangle = triangle;
            this.center = new PointStruct(triangle.GetCentroid());
        }

        double sign(PointD p1, PointD p2, PointD p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        public bool point_inside_trigon(PointD s)
        {
            var a = new PointD(triangle.GetVertex(0).X, triangle.GetVertex(0).Y);
            var b = new PointD(triangle.GetVertex(1).X, triangle.GetVertex(1).Y);
            var c = new PointD(triangle.GetVertex(2).X, triangle.GetVertex(2).Y);

            double as_x = s.X - a.X;
            double as_y = s.Y - a.Y;

            bool s_ab = (b.X - a.X) * as_y - (b.Y - a.Y) * as_x > 0;

            if ((c.X - a.X) * as_y - (c.Y - a.Y) * as_x > 0 == s_ab) return false;

            if ((c.X - b.X) * (s.Y - b.Y) - (c.Y - b.Y) * (s.X - b.X) > 0 != s_ab) return false;

            return true;
        }

        public bool PointInNode(PointD pt)
        {
            var v1 = new PointD(triangle.GetVertex(0).X, triangle.GetVertex(0).Y);
            var v2 = new PointD(triangle.GetVertex(1).X, triangle.GetVertex(1).Y);
            var v3 = new PointD(triangle.GetVertex(2).X, triangle.GetVertex(2).Y);


            bool b1, b2, b3;

            b1 = sign(pt, v1, v2) < 0.0f;
            b2 = sign(pt, v2, v3) < 0.0f;
            b3 = sign(pt, v3, v1) < 0.0f;

            return ((b1 == b2) && (b2 == b3));
        }

        public LineD GetPortal(GraphNode neighbor)
        {
            List<PointD> points = new List<PointD>();

            for(var s = 0; s < 3; s++)
            {
                var v = triangle.GetVertex(s);

                for(var ns = 0; ns < 3; ns++)
                {
                    var nv = neighbor.triangle.GetVertex(ns);
                    
                    if(v.X == nv.X && v.Y == nv.Y)
                    {
                        points.Add(new PointD(v.X, v.Y));
                    }
                }
            }

            //Order points by right to left of the line from centroid to centroid.
            points = points.OrderBy(t => Math.Sign((neighbor.center.X - center.X) * (t.Y - center.Y) - (neighbor.center.Y - center.Y) * (t.X - center.X))).ToList();

            if(points.Count == 2)
            {
                return new LineD(points[0], points[1]);
            }

            //Not Neighbors...
            return null;
        }

        public List<LineD> GetNonPortal(GraphNode neighbor, Point endPoint)
        {
            List<PointD> points = new List<PointD>();
            PointD externalVertex = new PointD(endPoint.X, endPoint.Y);

            for (var s = 0; s < 3; s++)
            {
                var v = triangle.GetVertex(s);

                var matched = false;

                for (var ns = 0; ns < 3; ns++)
                {
                    var nv = neighbor.triangle.GetVertex(ns);

                    if (v.X == nv.X && v.Y == nv.Y)
                    {
                        points.Add(new PointD(v.X, v.Y));
                        matched = true;
                    }
                }
            }

            //Order points by right to left of the line from centroid to centroid.
            points = points.OrderBy(t => Math.Sign((neighbor.center.X - center.X) * (t.Y - center.Y) - (neighbor.center.Y - center.Y) * (t.X - center.X))).ToList();

            if (points.Count == 2)
            {
                var line1 = new LineD(externalVertex, points[0]);
                var line2 = new LineD(points[1], externalVertex);

                return new List<LineD>() { line1, line2 };
            }
            else
            {
                return new List<LineD>();
            }
        }
        
    }
}
