using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public class FacetBody : IRenderableBody
    {
        private const int XMIN = 0;
		private const int XMAX = 1;
		private const int YMIN = 2;
		private const int YMAX = 3;
		private const int ZMIN = 4;
		private const int ZMAX = 5;

		private const int X1 = 0;
		private const int Y1 = 1;
		private const int Z1 = 2;
		private const int X2 = 3;
		private const int Y2 = 4;
		private const int Z2 = 5;
		private const int X3 = 6;
		private const int Y3 = 7;
		private const int Z3 = 8;
		private const int N1 = 9;
		private const int N2 = 10;
		private const int N3 = 11;

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
        private double xmin_global, xmax_global, ymin_global, ymax_global, zmin_global, zmax_global;
        public int FaceCount {get {return face_count;}}

        private int face_count;
        private double[,] data;
        private double delta_x, delta_y, delta_z;
        private int x_box_count, y_box_count, z_box_count;
        private int[][] facet_lookup_edge_adjacencies;
        private int[][] facet_lookup_vertex_adjacencies;
        private int[][][] facet_lookup_box_covers;
        private int[,,][] box_lookup_facet_covers;
        double[] facet_localcoord_bounds;

        public FacetBody(Triple point)
        {
            optical_properties = new OpticalProperties();
            anchor = point;
        }

        public void PassRawData(int _face_count, double[,] _geom_data, double[] _bounds)
        {
            face_count = _face_count;
            data = _geom_data;
            facet_localcoord_bounds = _bounds;
            compute_bounds();
        }

        public void PassCoords(double _delta_x, double _delta_y, double _delta_z, int _x_box_count, int _y_box_count, int _z_box_count)
        {
            delta_x = _delta_x;
            delta_y = _delta_y;
            delta_z = _delta_z;
            x_box_count = _x_box_count;
            y_box_count = _y_box_count;
            z_box_count = _z_box_count;
        }

        public void PassMetaData(int[][] _facet_lookup_edge_adjacencies, int[][] _facet_lookup_vertex_adjacencies, int[][][] _facet_lookup_box_covers, int[,,][] _box_lookup_facet_covers)
        {
            facet_lookup_edge_adjacencies = _facet_lookup_edge_adjacencies;
            facet_lookup_vertex_adjacencies = _facet_lookup_vertex_adjacencies;
            facet_lookup_box_covers = _facet_lookup_box_covers;
            box_lookup_facet_covers = _box_lookup_facet_covers;
        }

        private void compute_bounds()
        {
            xmin_global = anchor.X + facet_localcoord_bounds[XMIN];
            xmax_global = anchor.X + facet_localcoord_bounds[XMAX];
            ymin_global = anchor.Y + facet_localcoord_bounds[YMIN];
            ymax_global = anchor.Y + facet_localcoord_bounds[YMAX];
            zmin_global = anchor.Z + facet_localcoord_bounds[ZMIN];
            zmax_global = anchor.Z + facet_localcoord_bounds[ZMAX];
        }
        public bool CheckIncidence(Ray input, out double distance, out Triple point_of_incidence, out Triple normal_vector)
        {
            distance = 1;
            point_of_incidence = new Triple(1,1,1);
            normal_vector = new Triple(1,1,1);
            return true;
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
            return false;
        }
        //Note: anchor represents (0,0,0) in the coordinate system local to the body.

        public void Move(Triple delta)
        {
            anchor = anchor + delta;
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
