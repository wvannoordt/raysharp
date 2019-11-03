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
        private double intensity;
        public double Intensity {get {return intensity;} set {intensity = value;}}
        public Triple Direction {get {return direction;} set {direction = value.Unit();}}
        public Triple BaseColor {get {return base_color;} set {base_color = value;}}
        public GlobalLightSource(Triple _direction)
        {
            init_defaults();
            direction = _direction.Unit();
        }
        public GlobalLightSource(Triple _direction, Triple _base_color)
        {
            init_defaults();
            direction = _direction.Unit();
        }
        void init_defaults()
        {
            base_color = new Triple(1, 1, 1);
        }
        public double GetPercentLightReception(Ray input)
        {
            double dot = -1*input.Direction * direction;
            return Utils.Max(0, dot*dot*dot);
        }

    }
}
