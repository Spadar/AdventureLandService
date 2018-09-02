using System;
using System.Collections.Generic;
using System.Text;

namespace AdventureLandLibrary.Geometry
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Point(TriangleNet.Geometry.Point point)
        {
            this.X = (int)point.X;
            this.Y = (int)point.Y;
        }

        public Point(TriangleNet.Geometry.Vertex vertex)
        {
            this.X = (int)vertex.X;
            this.Y = (int)vertex.Y;
        }

        public Point(TriangleNet.Geometry.Point point, int xOffset, int yOffset)
        {
            this.X = (int)point.X - xOffset;
            this.Y = (int)point.Y - yOffset;
        }

        public Point(TriangleNet.Geometry.Vertex vertex, int xOffset, int yOffset)
        {
            this.X = (int)vertex.X - xOffset;
            this.Y = (int)vertex.Y - yOffset;
        }

        public Point(PointStruct point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        public Point(PointD point)
        {
            this.X = (int)point.X;
            this.Y = (int)point.Y;
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

        //public override int GetHashCode()
        //{
        //    int hash = 17;
        //    // Suitable nullity checks etc, of course :)
        //    hash = hash * 23 + X.GetHashCode();
        //    hash = hash * 23 + Y.GetHashCode();
        //    return hash;
        //}

        //public override bool Equals(object obj)
        //{
        //    var otherPoint = obj as Point;
        //    if (otherPoint == null)
        //    {
        //        return false;
        //    }

        //    return X == otherPoint.X && Y == otherPoint.Y;
        //}
    }
}
