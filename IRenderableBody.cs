using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public interface IRenderableBody
    {
        OpticalProperties BodyOpticalProperties {get; set;}
        bool CheckIncidence(Ray input, out Ray reflected_ray, out Ray refracted_ray, out double transmission_distance, out Ray[] light_rays);
        bool CheckContainsPoint(Triple point);
        //Note: anchor represents (0,0,0) in the coordinate system local to the body.
        Triple Anchor{get; set;}
        void Move(Triple delta);
        void Rotate(Triple axis, double angle);
        void RotateAbout(Triple axis, Triple point, double angle);
        Ray TransformToLocalCoords(Ray inbound);
        //This will initialize with an elevation of 0 and a azimuth. It will keep track of rotations for later computation of zenith and azimuth angles.
        Triple RotationReference {get;}
        double XminGlobal {get;}
        double XmaxGlobal {get;}
        double YminGlobal {get;}
        double YmaxGlobal {get;}
        double ZminGlobal {get;}
        double ZmaxGlobal {get;}
    }
}
