using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorPick
{
    public struct HSL
    {
        public double H; // Hue: 0-360
        public double S; // Saturation: 0-1
        public double L; // Lightness: 0-1
        public HSL(double h, double s, double l)
        {
            H = h;
            S = s;
            L = l;
        }
    }
}
