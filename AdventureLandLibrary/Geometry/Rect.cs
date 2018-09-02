using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureLandLibrary.Geometry
{
    public class Rect
    {
        int minX;
        int maxX;

        int minY;
        int maxY;

        private Point[] _points;
        public Point[] Points
        {
            get
            {
                if (_points == null)
                {
                    var points = new List<Point>();

                    for(var x = minX; x <= maxX; x++)
                    {
                        for(var y = minY; y <= maxY; y++)
                        {
                            points.Add(new Point(x, y));
                        }
                    }

                    _points = points.ToArray();
                }

                return _points;

            }
        }

        public Rect(Line line, int offsetX, int offsetY)
        {
            if(line.P1.X < line.P2.X)
            {
                minX = line.P1.X - offsetX;
                maxX = line.P2.X + offsetX;
            }
            else
            {
                minX = line.P2.X - offsetX;
                maxX = line.P1.X + offsetX;
            }

            if (line.P1.Y < line.P2.Y)
            {
                minY = line.P1.Y - offsetY;
                maxY = line.P2.Y + offsetY;
            }
            else
            {
                minY = line.P2.Y - offsetY;
                maxY = line.P1.Y + offsetY;
            }
        }

        public Rect(Line line, int offsetMinX, int offsetMaxX, int offsetMinY, int offsetMaxY)
        {
            if (line.P1.X < line.P2.X)
            {
                minX = line.P1.X - offsetMinX;
                maxX = line.P2.X + offsetMaxX;
            }
            else
            {
                minX = line.P2.X - offsetMinX;
                maxX = line.P1.X + offsetMaxX;
            }

            if (line.P1.Y < line.P2.Y)
            {
                minY = line.P1.Y - offsetMinY;
                maxY = line.P2.Y + offsetMaxY;
            }
            else
            {
                minY = line.P2.Y - offsetMinY;
                maxY = line.P1.Y + offsetMaxY;
            }
        }
    }
}
