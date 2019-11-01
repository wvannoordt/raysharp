using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    interface ILightSource
    {
        Triple BaseColor {get; set;}
        double GetPercentLightReception(Ray input);
    }
}
