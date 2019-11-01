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
        //Note: anchor represents (0,0,0) in the coordinate system local to the body.
        Triple Anchor{get; set;}
        void Move(Triple delta);
        void Rotate(Triple axis, double angle);
        //This will initialize with an elevation of 0 and a azimuth. It will keep track of rotations for later computation of zenith and azimuth angles.
        Triple RotationReference {get;}
        double Xmin {get;}
        double Xmax {get;}
        double Ymin {get;}
        double Ymax {get;}
        double Zmin {get;}
        double Zmax {get;}
    }
}
