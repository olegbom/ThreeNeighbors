using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ThreeNeighbors
{
    static class Helper
    {
        public static Point ToPoint(this Vector v) => new Point(v.X, v.Y);
    }
}
