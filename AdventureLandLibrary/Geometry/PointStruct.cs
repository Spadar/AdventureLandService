using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureLandLibrary.Geometry
{
    public struct PointStruct
    {
        public int X;
        public int Y;

        public PointStruct(int x, int y)
        {
            X = x;
            Y = y;
        }

        public PointStruct(TriangleNet.Geometry.Point point)
        {
            this.X = (int)point.X;
            this.Y = (int)point.Y;
        }

        public PointStruct(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public PointStruct(TriangleNet.Geometry.Vertex vertex)
        {
            this.X = (int)vertex.X;
            this.Y = (int)vertex.Y;
        }

        public double Distance(PointStruct p2)
        {
            return Math.Sqrt(Math.Pow((p2.X - X), 2) + Math.Pow((p2.Y - Y), 2));
        }
    }
}
