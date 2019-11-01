using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    interface IRenderableBody
    {
        OpticalProperties BodyOpticalProperties {get; set;}
        bool CheckPreliminaryIncidence(Ray r);
        bool CheckIncidence(Ray intput, out Ray reflected_ray, out Ray refracted_ray, out double transmission_distance, out Ray[] light_rays);
        bool CheckContainsPoint(Triple point);
        double Xmin {get;}
        double Xmax {get;}
        double Ymin {get;}
        double Ymax {get;}
        double Zmin {get;}
        double Zmax {get;}
    }
}
