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

                var limit = Math.Ceiling(P1.Distance(P2));
                var angle = P1.Angle(P2);

                double xDir = Math.Cos(angle);
                double yDir = Math.Sin(angle);

                HashSet<Point> pointList = new HashSet<Point>();

                var curPoint = P1;
                double curX = P1.X;
                double curY = P1.Y;

                while ((curPoint.X != P2.X || curPoint.Y != P2.Y) && pointList.Count <= limit)
                {
                    pointList.Add(curPoint);
                    curX = curX + xDir;
                    curY = curY + yDir;
                    curPoint = new Point((int)curX, (int)curY);
                }

                pointList.Add(P2);

                _points = new Point[pointList.Count];
                pointList.CopyTo(_points);

        }
    }
}
