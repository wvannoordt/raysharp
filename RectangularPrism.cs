using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public class RectangularPrism : IRenderableBody
    {
        private OpticalProperties optical_properties;
        public OpticalProperties BodyOpticalProperties {get {return optical_properties;} set{optical_properties = value;}}
        private Triple anchor, rotaion_reference;
        public Triple RotationReference {get {return rotaion_reference;}}
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
            rotaion_reference = new Triple(0, 0, 1);
        }
        public RectangularPrism(Triple _anchor, double _side_length_x, double _side_length_y, double _side_length_z)
        {
            anchor = _anchor;
            side_length_x = _side_length_x;
            side_length_y = _side_length_y;
            side_length_z = _side_length_z;
            compute_global_box_bounds();
            optical_properties = new OpticalProperties();
            rotaion_reference = new Triple(0, 0, 1);
        }
        public bool CheckIncidence(Ray r, out double distance, out Triple point_of_incidence)
        {
            point_of_incidence = null;
            bool xmin_possible = Math.Sign(xmin_global - r.X) == Math.Sign(r.V1);
            bool xmax_possible = Math.Sign(xmax_global - r.X) == Math.Sign(r.V1);
            bool ymin_possible = Math.Sign(ymin_global - r.Y) == Math.Sign(r.V2);
            bool ymax_possible = Math.Sign(ymax_global - r.Y) == Math.Sign(r.V2);
            bool zmin_possible = Math.Sign(zmin_global - r.Z) == Math.Sign(r.V3);
            bool zmax_possible = Math.Sign(zmax_global - r.Z) == Math.Sign(r.V3);
            if (!((xmin_possible||xmax_possible) && (ymin_possible||ymax_possible) && (zmin_possible||zmax_possible)))
            {
                distance = -1;
                return false;
            }

            //Bounding box check
            double[] dists = new double[]
            {
                (xmin_global - r.X)/r.V1, //xmin
                (xmax_global - r.X)/r.V1, //xmax
                (ymin_global - r.Y)/r.V2, //ymin
                (ymax_global - r.Y)/r.V2, //ymax
                (zmin_global - r.Z)/r.V3, //zmin
                (zmax_global - r.Z)/r.V3 //zmax
            };

            bool confirm = false;
            double cur_min_dist = -1;
            Triple xmin_position = r.Position + dists[0]*r.Direction;
            if (xmin_position.Y <= ymax_global && xmin_position.Y > ymin_global && xmin_position.Z <= zmax_global && xmin_position.Z > zmin_global)
            {
                if (cur_min_dist < 0 || dists[0] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[0];
                    point_of_incidence = xmin_position;
                }
            }
            Triple xmax_position = r.Position + dists[1]*r.Direction;
            if (xmax_position.Y <= ymax_global && xmax_position.Y > ymin_global && xmax_position.Z <= zmax_global && xmax_position.Z > zmin_global)
            {
                if (cur_min_dist < 0 || dists[1] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[1];
                    point_of_incidence = xmax_position;
                }
            }
            Triple ymin_position = r.Position + dists[2]*r.Direction;
            if (ymin_position.Z <= zmax_global && ymin_position.Z > zmin_global && ymin_position.X <= xmax_global && ymin_position.X > xmin_global)
            {
                if (cur_min_dist < 0 || dists[2] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[2];
                    point_of_incidence = ymin_position;
                }
            }
            Triple ymax_position = r.Position + dists[3]*r.Direction;
            if (ymax_position.Z <= zmax_global && ymax_position.Z > zmin_global && ymax_position.X <= xmax_global && ymax_position.X > xmin_global)
            {
                if (cur_min_dist < 0 || dists[3] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[3];
                    point_of_incidence = ymax_position;
                }
            }
            Triple zmin_position = r.Position + dists[4]*r.Direction;
            if (zmin_position.X <= xmax_global && zmin_position.X > xmin_global && zmin_position.Y <= ymax_global && zmin_position.Y > ymin_global)
            {
                if (cur_min_dist < 0 || dists[4] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[4];
                    point_of_incidence = zmin_position;
                }
            }
            Triple zmax_position = r.Position + dists[5]*r.Direction;
            if (zmax_position.X <= xmax_global && zmax_position.X > xmin_global && zmax_position.Y <= ymax_global && zmax_position.Y > ymin_global)
            {
                if (cur_min_dist < 0 || dists[5] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[5];
                    point_of_incidence = zmax_position;
                }
            }
            if (confirm)
            {
                distance = cur_min_dist;
                return true;
            }
            distance = -1;
            return false;
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

        }
        public void Rotate(Triple axis, double angle)
        {

        }
        public void RotateAbout(Triple axis, Triple point, double angle)
        {

        }
    }
}
