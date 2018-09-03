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
