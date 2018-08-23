using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureLandLibrary.Pathfinding
{
    public class PathNode
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PathNode(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public PathNode(Geometry.Point point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }
    }
}
