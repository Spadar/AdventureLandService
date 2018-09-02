using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AdventureLandLibrary.Geometry
{
    public class Line
    {
        private Point[] _points;

        [JsonIgnore]
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

        public Point FindClosestPoint(Point pt)
        {
            Point closest = null;
            float dx = P2.X - P1.X;
            float dy = P2.Y - P1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = new Point(P1.X, P1.Y);
                return closest;
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - P1.X) * dx + (pt.Y - P1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Point(P1.X, P1.Y);
                return closest;
            }
            else if (t > 1)
            {
                closest = new Point(P2.X, P2.Y);
            }
            else
            {
                closest = new Point((int)(P1.X + t * dx),(int)(P1.Y + t * dy));
                return closest;
            }

            return closest;
        }

        public bool IsPointOnLine(Point point)
        {
            var pointMatch = Points.Where(p => p.X == point.X && p.Y == point.Y).ToArray();

            return pointMatch.Length > 0;
        }

        /// <summary>
        /// Sourced from http://silverbling.blogspot.com/2010/06/2d-line-segment-intersection-detection.html
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public Point Intersection(Line line)
        {
            float firstLineSlopeX, firstLineSlopeY, secondLineSlopeX, secondLineSlopeY;

            firstLineSlopeX = this.P2.X - this.P1.X;
            firstLineSlopeY = this.P2.Y - this.P1.Y;

            secondLineSlopeX = line.P2.X - line.P1.X;
            secondLineSlopeY = line.P2.Y - line.P1.Y;

            float s, t;
            s = (-firstLineSlopeY * (this.P1.X - line.P1.X) + firstLineSlopeX * (this.P1.Y - line.P1.Y)) / (-secondLineSlopeX * firstLineSlopeY + firstLineSlopeX * secondLineSlopeY);
            t = (secondLineSlopeX * (this.P1.Y - line.P1.Y) - secondLineSlopeY * (this.P1.X - line.P1.X)) / (-secondLineSlopeX * firstLineSlopeY + firstLineSlopeX * secondLineSlopeY);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                float intersectionPointX = this.P1.X + (t * firstLineSlopeX);
                float intersectionPointY = this.P1.Y + (t * firstLineSlopeY);

                //// Collision detected
                //intersectionPoint = new Vector3(intersectionPointX, intersectionPointY, 0);
                var intersectionPoint = new Point((int)intersectionPointX, (int)intersectionPointY);
                return intersectionPoint;
            }

            return null; // No collision
        }

        // Given three colinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        bool onSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        // To find orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are colinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        int orientation(Point p, Point q, Point r)
        {
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
            // for details of below formula.
            int val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0;  // colinear

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        // The main function that returns true if line segment 'p1q1'
        // and 'p2q2' intersect.
        public bool doIntersect(Line line)
        {
            // Find the four orientations needed for general and
            // special cases
            int o1 = orientation(P1, P2, line.P1);
            int o2 = orientation(P1, P2, line.P2);
            int o3 = orientation(line.P1, line.P2, P1);
            int o4 = orientation(line.P1, line.P2, P2);

            // General case
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1
            if (o1 == 0 && onSegment(P1, line.P1, P2)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1
            if (o2 == 0 && onSegment(P1, line.P2, P2)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2
            if (o3 == 0 && onSegment(line.P1, P1, line.P2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2
            if (o4 == 0 && onSegment(line.P1, P2, line.P2)) return true;

            return false; // Doesn't fall in any of the above cases
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
