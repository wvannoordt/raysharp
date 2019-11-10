using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public class RectangularPrism : IRenderableBody
    {
        private const double NORMAL_EPSILON = 1e-6;
        private OpticalProperties optical_properties;
        public OpticalProperties BodyOpticalProperties {get {return optical_properties;} set{optical_properties = value;}}
        private Triple anchor, rotation_reference;
        public Triple RotationReference {get {return rotation_reference;}}
        public Triple Anchor{get {return anchor;} set {anchor = value;}}
        private double side_length_x,side_length_y,side_length_z,xmin_global,xmax_global,ymin_global,ymax_global,zmin_global,zmax_global;
        public double SideLengthX {get {return side_length_x;}}
        public double SideLengthY {get {return side_length_y;}}
        public double SideLengthZ {get {return side_length_z;}}
        public double XminGlobal {get {return xmin_global;}}
        public double XmaxGlobal {get {return xmax_global;}}
        public double YminGlobal {get {return ymin_global;}}
        public double YmaxGlobal {get {return ymax_global;}}
        public double ZminGlobal {get {return zmin_global;}}
        public double ZmaxGlobal {get {return zmax_global;}}
        public RectangularPrism(Triple _anchor, double _side_length)
        {
            anchor = _anchor;
            side_length_x = _side_length;
            side_length_y = _side_length;
            side_length_z = _side_length;
            compute_global_box_bounds();
            optical_properties = new OpticalProperties();
            rotation_reference = new Triple(0, 0, 1);
        }
        public RectangularPrism(Triple _anchor, double _side_length_x, double _side_length_y, double _side_length_z)
        {
            anchor = _anchor;
            side_length_x = _side_length_x;
            side_length_y = _side_length_y;
            side_length_z = _side_length_z;
            compute_global_box_bounds();
            optical_properties = new OpticalProperties();
            rotation_reference = new Triple(0, 0, 1);
        }
        public bool CheckIncidence(Ray r, out double distance, out Triple point_of_incidence, out Triple normal_vector)
        {
            double[] bounds = new double[]
            {
                xmin_global,
                xmax_global,
                ymin_global,
                ymax_global,
                zmin_global,
                zmax_global
            };
            return Utils.CheckBoxIncidence(r, bounds, out point_of_incidence, out normal_vector, out distance);
        }
        //Need to think carefully about how to  do this.
        public void ComputeFullIncidence(Ray input, out Ray reflected_ray, out Ray refracted_ray, out double transmission_distance, out Ray[] light_rays)
        {
            reflected_ray= null;
            refracted_ray = null;
            transmission_distance = 0;
            light_rays = null;
        }
        private void compute_global_box_bounds()
        {
            //WILL NEED TO ACCOUNT FOR ROTATION LATER
            double l_x = 0.5*side_length_x;
            double l_y = 0.5*side_length_y;
            double l_z = 0.5*side_length_z;
            xmin_global = anchor.X-l_x;
            xmax_global = anchor.X+l_x;
            ymin_global = anchor.Y-l_y;
            ymax_global = anchor.Y+l_y;
            zmin_global = anchor.Z-l_z;
            zmax_global = anchor.Z+l_z;
        }
        public bool CheckContainsPoint(Triple point)
        {
            return false;
        }
        public void Move(Triple delta)
        {
            anchor = anchor + delta;
            compute_global_box_bounds();
        }
        public void Rotate(Triple axis, double angle)
        {

        }
        public void RotateAbout(Triple axis, Triple point, double angle)
        {

        }
    }
}
