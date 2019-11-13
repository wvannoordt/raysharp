using System;
using System.IO;
using System.Collections.Generic;

namespace raysharp
{
    public static class Utils
    {
        private static double EPSILON =1e-8;
        public static bool CheckMachineZero(double a)
        {
            return Math.Abs(a) < EPSILON;
        }
        public static int[][] ComputeBoxRayCover(Ray input, double x0, double x1, double y0, double y1, double z0, double z1, int nx, int ny, int nz, double dx, double dy, double dz)
        {
            Triple entry_point, exit_point;
            Triple null1;
            double null2;
            //need a box containment check here.
            if (!CheckBoxIncidence(input, new double[] {x0, x1, y0, y1, z0, z1}, out entry_point, out null1, out null2)) return new int[0][];
            bool x_impossible = Math.Abs(input.Direction.X) < 1e-10;
            bool y_impossible = Math.Abs(input.Direction.Y) < 1e-10;
            bool z_impossible = Math.Abs(input.Direction.Z) < 1e-10;
            double[] extreme_dists = new double[6];
            extreme_dists[0] = x_impossible ? double.PositiveInfinity : (x0 - entry_point.X)/input.Direction.X;
            extreme_dists[1] = x_impossible ? double.PositiveInfinity : (x1 - entry_point.X)/input.Direction.X;
            extreme_dists[2] = y_impossible ? double.PositiveInfinity : (y0 - entry_point.Y)/input.Direction.Y;
            extreme_dists[3] = y_impossible ? double.PositiveInfinity : (y1 - entry_point.Y)/input.Direction.Y;
            extreme_dists[4] = z_impossible ? double.PositiveInfinity : (z0 - entry_point.Z)/input.Direction.Z;
            extreme_dists[5] = z_impossible ? double.PositiveInfinity : (z1 - entry_point.Z)/input.Direction.Z;
            double min_pos_dist = double.PositiveInfinity;
            for(int i = 0; i < 6; i++)
            {
                extreme_dists[i] = extreme_dists[i] < 1e-8 ? double.PositiveInfinity : extreme_dists[i];
                min_pos_dist = (min_pos_dist > extreme_dists[i]) ? extreme_dists[i] : min_pos_dist;
            }
            exit_point = entry_point + min_pos_dist*input.Direction;

            Utils.WriteCsv("outputdata/bounds.csv", new double[] {x0,x1,y0,y1,z0,z1});
            Utils.WriteCsv("outputdata/entry.csv", new double[] {entry_point.X, entry_point.Y, entry_point.Z});
            Utils.WriteCsv("outputdata/exit.csv", new double[] {exit_point.X, exit_point.Y, exit_point.Z});
            Utils.WriteCsv("outputdata/origin.csv", new double[] {input.Position.X, input.Position.Y, input.Position.Z});

            return new int[0][];
        }
        public static bool CheckBoxIncidence(Ray r, double[] bounds, out Triple point_of_incidence, out Triple normal_vector, out double distance)
        {
            point_of_incidence = null;
            normal_vector = null;
            distance = -1;

            double xmin_global = bounds[0];
            double xmax_global = bounds[1];
            double ymin_global = bounds[2];
            double ymax_global = bounds[3];
            double zmin_global = bounds[4];
            double zmax_global = bounds[5];

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
                    normal_vector = new Triple(-1, 0, 0);
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
                    normal_vector = new Triple(1, 0, 0);
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
                    normal_vector = new Triple(0, -1, 0);
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
                    normal_vector = new Triple(0, 1, 0);
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
                    normal_vector = new Triple(0, 0, -1);
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
                    normal_vector = new Triple(0, 0, 1);
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
        public static bool CheckBoxIncidence(Ray r, double[] bounds)
        {
            Triple null1, null2;
            double null3;
            return CheckBoxIncidence(r, bounds, out null1, out null2, out null3);
        }
        public static void WriteCsv(string filename, double[] contents)
        {
            List<string> lines = new List<string>();
            foreach(double g in contents)
            {
                lines.Add(g.ToString());
            }
            File.WriteAllLines(filename, lines.ToArray());
        }
        public static void WriteCsv(string filename, double[,] contents)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < contents.GetLength(0); i++)
            {
                string line = contents[i,0].ToString();
                for (int j = 0; j < contents.GetLength(1); j++)
                {
                    line += ("," + contents[i,j].ToString());
                }
                lines.Add(line);
            }
            File.WriteAllLines(filename, lines.ToArray());
        }
        public static void WriteCsv(string filename, int[,] contents)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < contents.GetLength(0); i++)
            {
                string line = contents[i,0].ToString();
                for (int j = 0; j < contents.GetLength(1); j++)
                {
                    line += ("," + contents[i,j].ToString());
                }
                lines.Add(line);
            }
            File.WriteAllLines(filename, lines.ToArray());
        }
        public static bool IntArrayContains(int[] stuff, int i)
        {
            for (int k = 0; k < stuff.Length; k++)
            {
                if (stuff[k] == i) return true;
            }
            return false;
        }
        public static double Min(params double[] xs)
        {
            double output = double.PositiveInfinity;
            foreach (double x in xs)
            {
                if (x < output) output = x;
            }
            return output;
        }
        public static double Min(out int idx, params double[] xs)
        {
            double output = double.PositiveInfinity;
            int candidate = 0;
            idx = 0;
            foreach (double x in xs)
            {
                if (x < output)
                {
                    output = x;
                    idx = candidate;
                }
                candidate++;
            }
            return output;
        }
        public static double MinPos(out int idx, params double[] xs)
        {
            double output = double.PositiveInfinity;
            int candidate = 0;
            idx = 0;
            foreach (double x in xs)
            {
                if (x < output && x >= 0)
                {
                    output = x;
                    idx = candidate;
                }
                candidate++;
            }
            return output;
        }
        public static double MinPos(params double[] xs)
        {
            double output = double.PositiveInfinity;
            int candidate = 0;
            foreach (double x in xs)
            {
                if (x < output && x >= 0)
                {
                    output = x;
                }
                candidate++;
            }
            return output;
        }
        public static double Max(params double[] xs)
        {
            double output = double.NegativeInfinity;
            foreach (double x in xs)
            {
                if (x > output) output = x;
            }
            return output;
        }

        public static int Min(params int[] xs)
        {
            int output = int.MaxValue;
            foreach (int x in xs)
            {
                if (x < output) output = x;
            }
            return output;
        }
        public static int Max(params int[] xs)
        {
            int output = int.MinValue;
            foreach (int x in xs)
            {
                if (x > output) output = x;
            }
            return output;
        }
        public static void SortAccording<T>(T[] arr, double[] accord)
        {
            quick_sort_acc<T>(accord, 0, arr.Length - 1, arr);
        }
        private static void quick_sort_acc<T>(double[] arr, int left, int right, T[] stuff)
        {
            if (left < right)
            {
                int pivot = Partition<T>(arr, left, right, stuff);

                if (pivot > 1) {
                    quick_sort_acc(arr, left, pivot - 1, stuff);
                }
                if (pivot + 1 < right) {
                    quick_sort_acc(arr, pivot + 1, right, stuff);
                }
            }

        }

        private static int Partition<T>(double[] arr, int left, int right, T[] stuff)
        {
            double pivot = arr[left];
            while (true)
            {

                while (arr[left] < pivot)
                {
                    left++;
                }

                while (arr[right] > pivot)
                {
                    right--;
                }

                if (left < right)
                {
                    if (arr[left] == arr[right]) return right;

                    double temp = arr[left];
                    arr[left] = arr[right];
                    arr[right] = temp;

                    T temp2 = stuff[left];
                    stuff[left] = stuff[right];
                    stuff[right] = temp2;


                }
                else
                {
                    return right;
                }
            }
        }
    }
}
