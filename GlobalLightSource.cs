using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public class GlobalLightSource : ILightSource
    {
        private Triple direction, base_color;
        public Triple Direction {get {return direction;} set {direction = value;}}
        public Triple BaseColor {get {return base_color;} set {base_color = value;}}
        public GlobalLightSource()
        {
            
        }
        public double GetPercentLightReception(Ray input)
        {
            return 0;
        }

    }
}
