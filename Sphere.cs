using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public class Sphere : IRenderableBody
    {
        private Triple anchor;
        public Triple RotationReference {get;}
        public double XminGlobal {get{return xmin_global;}}
        public double XmaxGlobal {get{return xmax_global;}}
        public double YminGlobal {get{return ymin_global;}}
        public double YmaxGlobal {get{return ymax_global;}}
        public double ZminGlobal {get{return zmin_global;}}
        public double ZmaxGlobal {get{return zmax_global;}}
        public Triple Anchor{get{return anchor;} set{anchor=value;}}
        private OpticalProperties optical_properties;
        public OpticalProperties BodyOpticalProperties {get{return optical_properties;} set{optical_properties=value;}}
        private double radius, xmin_global, xmax_global, ymin_global, ymax_global, zmin_global, zmax_global;
        public double Radius{get{return radius;}set{radius=value;}}
        public Sphere(Triple point, double _radius)
        {
            optical_properties = new OpticalProperties();
            anchor = point;
            radius = _radius;
            compute_bounds();
        }
        private void compute_bounds()
        {
            xmin_global = anchor.X - radius;
            xmax_global = anchor.X + radius;
            ymin_global = anchor.Y - radius;
            ymax_global = anchor.Y + radius;
            zmin_global = anchor.Z - radius;
            zmax_global = anchor.Z + radius;
        }
        public bool CheckIncidence(Ray input, out double distance, out Triple point_of_incidence, out Triple normal_vector)
        {
            distance = -1;
            point_of_incidence = null;
            normal_vector = null;
            if (CheckContainsPoint(input.Position)) return false;
            Triple pos_to_center = input.Position - anchor;
            double A = pos_to_center*input.Direction;
            A *= A;
            double B = pos_to_center.NormSq() - radius*radius;
            double discriminant = A - B;
            if (discriminant >= 0)
            {
                double pre = -1*input.Direction*pos_to_center;
                double sqrt = Math.Sqrt(discriminant);
                double d1 = pre + sqrt;
                double d2 = pre - sqrt;
                double dmin = Utils.Min(d1, d2);
                double dmax = Utils.Min(d1, d2);
                if (dmin < 0 && dmax < 0) return false;
                distance = dmin;
                point_of_incidence = input.Position + distance*input.Direction;
                normal_vector = (point_of_incidence - anchor).Unit();
                return true;
            }
            else return false;
        }
        public void ComputeFullIncidence(Ray input, out Ray reflected_ray, out Ray refracted_ray, out double transmission_distance, out Ray[] light_rays)
        {
            reflected_ray = null;
            refracted_ray = null;
            transmission_distance = -1;
            light_rays = null;
        }
        public bool CheckContainsPoint(Triple point)
        {
            double dx = point.X - anchor.X;
            double dy = point.Y - anchor.Y;
            double dz = point.Z - anchor.Z;
            return Math.Sqrt(dx*dx + dy*dy + dz*dz) < radius;
        }
        //Note: anchor represents (0,0,0) in the coordinate system local to the body.

        public void Move(Triple delta)
        {
            anchor = anchor + delta;
            compute_bounds();
        }
        public void Rotate(Triple axis, double angle)
        {

        }
        public void RotateAbout(Triple axis, Triple point, double angle)
        {

        }
        //This will initialize with an elevation of 0 and an azimuth of 0. It will keep track of rotations for later computation of zenith and azimuth angles.
    }
}
