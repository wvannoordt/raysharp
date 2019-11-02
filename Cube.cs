using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public class Cube : IRenderableBody
    {
        private OpticalProperties optical_properties;
        public OpticalProperties BodyOpticalProperties {get {return optical_properties;} set{optical_properties = value;}}
        private Triple anchor, rotaion_reference;
        public Triple RotationReference {get {return rotaion_reference;}}
        public Triple Anchor{get {return anchor;} set {anchor = value;}}
        private double side_length,xmin_global,xmax_global,ymin_global,ymax_global,zmin_global,zmax_global;
        public double Side_length {get {return side_length;}}
        public double XminGlobal {get {return xmin_global;}}
        public double XmaxGlobal {get {return xmax_global;}}
        public double YminGlobal {get {return ymin_global;}}
        public double YmaxGlobal {get {return ymax_global;}}
        public double ZminGlobal {get {return zmin_global;}}
        public double ZmaxGlobal {get {return zmax_global;}}
        public Cube(Triple _anchor, double _side_length)
        {
            anchor = _anchor;
            side_length = _side_length;
            compute_global_box_bounds();
            optical_properties = new OpticalProperties();
            rotaion_reference = new Triple(0, 0, 1);
        }
        public Ray TransformToLocalCoords(Ray inbound)
        {
            return inbound;
        }
        public bool CheckIncidence(Ray input, out Ray reflected_ray, out Ray refracted_ray, out double transmission_distance, out Ray[] light_rays)
        {
            reflected_ray= null;
            refracted_ray = null;
            transmission_distance = 0;
            light_rays = null;
            return false;
        }
        private void compute_global_box_bounds()
        {
            double l = 0.867*side_length;
            xmin_global = anchor.X-l;
            xmax_global = anchor.X+l;
            ymin_global = anchor.Y-l;
            ymax_global = anchor.Y+l;
            zmin_global = anchor.Z-l;
            zmax_global = anchor.Z+l;
        }
        public bool CheckContainsPoint(Triple point)
        {
            return false;
        }
        public void Move(Triple delta)
        {

        }
        public void Rotate(Triple axis, double angle)
        {

        }
        public void RotateAbout(Triple axis, Triple point, double angle)
        {

        }
    }
}
