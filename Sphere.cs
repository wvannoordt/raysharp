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
        public double XminGlobal {get{return anchor.X - radius;}}
        public double XmaxGlobal {get{return anchor.X + radius;}}
        public double YminGlobal {get{return anchor.Y - radius;}}
        public double YmaxGlobal {get{return anchor.Y + radius;}}
        public double ZminGlobal {get{return anchor.Z - radius;}}
        public double ZmaxGlobal {get{return anchor.Z + radius;}}
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
        }
        public bool CheckIncidence(Ray input, out double distance, out Triple point_of_incidence, out Triple normal_vector)
        {
            distance = -1;
            point_of_incidence = null;
            normal_vector = null;
            Triple pos_to_center = input.Position - anchor;
            double A = pos_to_center*input.Direction;
            A *= A;
            double B = pos_to_center.NormSq() - radius*radius;
            double discriminant = A - B;
            if (discriminant >= 0)
            {
                double pre = -1*input.Direction*pos_to_center;
                double sqrt = Math.Sqrt(discriminant);
                distance = Utils.MinPos(pre + sqrt, pre - sqrt) - 1e-6;
                point_of_incidence = input.Position + distance*input.Direction;
                //Console.WriteLine(point_of_incidence);
                normal_vector = (point_of_incidence - anchor).Unit();
                //Console.WriteLine(normal_vector);
                //Console.ReadLine();
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
