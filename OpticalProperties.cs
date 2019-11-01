using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public class OpticalProperties
    {
        public double RefractiveIndex {get;set;}
        public Triple BaseColor {get; set;}
        public bool IsReflective {get; set;}
        public bool IsTransparent {get; set;}
        public double AbsorptionCoefficient {get; set;}
        public OpticalProperties()
        {
            RefractiveIndex = 1.3;
            BaseColor = new Triple(0.7, 0.7, 0.7);
            IsReflective = false;
        }

    }
}
