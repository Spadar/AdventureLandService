using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureLandLibrary.Geometry
{
    public class LineD
    {
        public PointD P1;
        public PointD P2;

        public LineD(PointD P1, PointD P2)
        {
            this.P1 = P1;
            this.P2 = P2;
        }

        public int Direction(PointD point)
        {
            return Math.Sign((P2.X - P1.X) * (point.Y - P1.Y) - (P2.Y - P1.Y) * (point.X - P1.X));
        }
    }
}
