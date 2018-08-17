using System;
using System.Collections.Generic;
using System.Text;

namespace AdventureLandLibrary.Geometry
{
    public class Point
    {
        public int X;
        public int Y;

        public int erodeX;
        public int erodeY;

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Point(TriangleNet.Geometry.Vertex vertex)
        {
            this.X = (int)vertex.X;
            this.Y = (int)vertex.Y;
        }

        public double Angle(Point p2)
        {
            float xDiff = p2.X - X;
            float yDiff = p2.Y - Y;
            return Math.Atan2(yDiff, xDiff);
        }

        public double Distance(Point p2)
        {
            return Math.Sqrt(Math.Pow((p2.X - X), 2) + Math.Pow((p2.Y - Y), 2));
        }
    }
}
