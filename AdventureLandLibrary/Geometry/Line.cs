using System;
using System.Collections.Generic;
using System.Text;

namespace AdventureLandLibrary.Geometry
{
    public class Line
    {
        private Point[] _points;

        public Point[] Points
        {
            get
            {
                if(_points == null)
                {
                    GetPoints();
                }

                return _points;
            }
        }

        private Point _p1;
        public Point P1
        {
            get
            {
                return _p1;
            }
            set
            {
                _p1 = value;
                _points = null;
            }
        }

        private Point _p2;
        public Point P2
        {
            get
            {
                return _p2;
            }
            set
            {
                _p2 = value;
                _points = null;
            }
        }

        public Line(Point p1, Point p2)
        {
            this.P1 = p1;
            this.P2 = p2;
        }

        private void GetPoints()
        {

            //var limit = Math.Ceiling(P1.Distance(P2));
            //var angle = P1.Angle(P2);

            //double xDir = Math.Cos(angle);
            //double yDir = Math.Sin(angle);

            //HashSet<Point> pointList = new HashSet<Point>();

            //var curPoint = P1;
            //double curX = P1.X;
            //double curY = P1.Y;

            //while ((curPoint.X != P2.X || curPoint.Y != P2.Y) && pointList.Count <= limit)
            //{
            //    pointList.Add(curPoint);
            //    curX = curX + xDir;
            //    curY = curY + yDir;
            //    curPoint = new Point((int)curX, (int)curY);
            //}

            //pointList.Add(P2);

            //_points = new Point[pointList.Count];
            //pointList.CopyTo(_points);

            List<Point> points = new List<Point>();

            int x2 = P2.X;
            int x = P1.X;
            int y2 = P2.Y;
            int y = P1.Y;

            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                points.Add(new Point(x, y));
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }

            _points = points.ToArray();
        }
    }
}
