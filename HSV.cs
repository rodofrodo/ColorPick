using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorPick
{
    public struct HSV
    {
        public double H; // Hue: 0-360
        public double S; // Saturation: 0-1
        public double V; // Value: 0-1
        public HSV(double h, double s, double v)
        {
            H = h;
            S = s;
            V = v;
        }
    }
}
