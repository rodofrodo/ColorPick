using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorPick
{
    public struct CMYK
    {
        public double C { get; set; }
        public double M { get; set; }
        public double Y { get; set; }
        public double K { get; set; }
        public CMYK(double c, double m, double y, double k)
        {
            C = c;
            M = m;
            Y = y;
            K = k;
        }
    }
}
