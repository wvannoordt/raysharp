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
        double[] face_areas;
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
            compute_areas();
        }
        private void compute_areas()
        {
            face_areas = new double[face_count];
            for (int i = 0; i < face_count; i++)
            {
                Triple v1 = new Triple(data[i,X1], data[i,Y1], data[i,Z1]);
                Triple v2 = new Triple(data[i,X2], data[i,Y2], data[i,Z2]);
                Triple v3 = new Triple(data[i,X3], data[i,Y3], data[i,Z3]);
                face_areas[i] = get_area(v1, v2, v3);
            }
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
            distance = -1;
            normal_vector = null;
            double[] bounds = new double[] {xmin_global, xmax_global, ymin_global, ymax_global, zmin_global, zmax_global};
            //return Utils.CheckBoxIncidence(input, bounds, out point_of_incidence, out normal_vector, out distance);
            int id = compute_candidate_face_id(input, out point_of_incidence, out distance);
            if (id >= 0)
            {
                normal_vector = new Triple(data[id, N1], data[id, N2], data[id, N3]);
                return true;
            }
            return false;
        }
        private int compute_candidate_face_id(Ray input, out Triple point_of_incidence, out double distance)
        {
            //Sort by closest vertex
            int output = -1;
            point_of_incidence = null;
            distance = -1;

            /*List<int[]> cover = new List<int[]>();
            List<double> distances = new List<double>();
            for (int i = 0; i < x_box_count; i++)
            {
                for (int j = 0; j < y_box_count; j++)
                {
                    for (int k = 0; k < z_box_count; k++)
                    {
                        double dist;
                        if (true)//check_box_incidence(i,j,k,input,out dist))
                        {
                            cover.Add(new int[] {i,j,k});
                            distances.Add(1);//dist);
                        }
                    }
                }
            }

            int[][] cover_arr = new int[cover.Count][];
            double[] dist_arr = distances.ToArray();
            for (int i = 0; i < cover.Count; i++) cover_arr[i] = cover[i];*/

            /*Utils.SortAccording<int[]>(cover_arr, dist_arr);
            for (int i = 0; i < cover_arr.Length; i++)
            {
                double current_min_face_distance = double.PositiveInfinity;
                int[] cur_idx = cover_arr[i];
                int[] current_faces = box_lookup_facet_covers[cur_idx[0], cur_idx[1], cur_idx[2]];
                for (int j = 0; j < 4; j++)//current_faces.Length; j++)
                {
                    double current_face_distance;
                    Triple current_point_of_incidence;
                    //if (check_face_incidence(current_faces[j], input, out current_point_of_incidence, out current_face_distance))
                    if (check_face_incidence(j, input, out current_point_of_incidence, out current_face_distance))
                    {
                        if (current_face_distance < current_min_face_distance)
                        {
                            output = j;//current_faces[j];
                            current_min_face_distance = current_face_distance;
                            point_of_incidence = current_point_of_incidence;
                        }
                    }
                }
            }*/

            for (int j = 0; j < face_count; j++)
            {
                double current_distance;
                Triple current_point;
                if (check_face_incidence(j, input, out current_point, out current_distance))
                {
                    if (distance < 0 || current_distance < distance)
                    {
                        output = j;
                        distance = current_distance;
                        point_of_incidence = current_point;
                    }
                }
            }
            return output;
        }
        private bool check_face_incidence(int i, Ray input, out Triple point_of_incidence, out double distance)
        {
            point_of_incidence = null;
            distance = -1;

            double c_p_n = (data[i,X1] + anchor.X - input.Position.X)*data[i,N1] + (data[i,Y1] + anchor.Y - input.Position.Y)*data[i,N2] + (data[i,Z1] + anchor.Z - input.Position.Z)*data[i,N3];
            double r_n = input.Direction.X*data[i,N1] + input.Direction.Y*data[i,N2] + input.Direction.Z*data[i,N3];
            double t = c_p_n/r_n;
            if (t < 0) return false;

            //Definitely on-plane
            Triple tentative_point_of_incidence = input.Position + t*input.Direction - anchor;
            if (check_face_contains(tentative_point_of_incidence, i))
            {
                point_of_incidence = tentative_point_of_incidence;
                distance = t;
                return true;
            }
            return false;
        }
        private bool check_face_contains(Triple tentative_point_of_incidence, int i)
        {
            //Console.WriteLine("i: " + i);
            Triple v1 = new Triple(data[i,X1], data[i,Y1], data[i,Z1]);
            Triple v2 = new Triple(data[i,X2], data[i,Y2], data[i,Z2]);
            Triple v3 = new Triple(data[i,X3], data[i,Y3], data[i,Z3]);
            double area1 = get_area(v1, v2, tentative_point_of_incidence);
            double area2 = get_area(v1, v3, tentative_point_of_incidence);
            double area3 = get_area(v2, v3, tentative_point_of_incidence);
            return Utils.CheckMachineZero(area1 + area2 + area3 - face_areas[i]);
        }
        private double get_area(Triple a, Triple b, Triple c)
        {
            return 0.5*((b-a)%(b-c)).Norm();
        }
        private bool check_box_incidence(int i, int j, int k, Ray input, out double dist)
        {
            double[] bounds = new double[]
            {
                xmin_global + i*delta_x,
                xmin_global + (i+1)*delta_x,
                ymin_global + j*delta_y,
                ymin_global + (j+1)*delta_y,
                zmin_global + k*delta_z,
                zmin_global + (k+1)*delta_z
            };
            Triple null1, null2;
            //This  might be useful to pass out at some point.
            return Utils.CheckBoxIncidence(input, bounds, out null1, out null2, out dist);
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
        public void WriteAsciiStl(string filename)
		{
			using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("solid Default");
				for (int i = 0; i < face_count; i++)
				{
					sw.WriteLine("  facet normal " + data[i, N1] + " " + data[i, N2] + " " + data[i, N3]);
					sw.WriteLine("    outer loop");
					sw.WriteLine("      vertex " + data[i, X1] + " " + data[i, Y1] + " " + data[i, Z1]);
					sw.WriteLine("      vertex " + data[i, X2] + " " + data[i, Y2] + " " + data[i, Z2]);
					sw.WriteLine("      vertex " + data[i, X3] + " " + data[i, Y3] + " " + data[i, Z3]);
					sw.WriteLine("    endloop");
					sw.WriteLine("  endfacet");
				}
				sw.WriteLine("endsolid Default");
            }
		}
        //This will initialize with an elevation of 0 and an azimuth of 0. It will keep track of rotations for later computation of zenith and azimuth angles.
    }
}
