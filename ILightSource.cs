using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public interface ILightSource
    {
        Triple BaseColor {get; set;}
        double Intensity {get; set;}
        double GetPercentLightReception(Ray input);
        Ray ComputeDiffuseLightingRay(Triple relevant_point);
    }
}
