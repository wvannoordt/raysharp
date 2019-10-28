using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace raysharp
{
	public class Stl
	{
		private const double EPSILON = 1e-15;

		//Needs testing to determine "break-even" point... might be system-specific.
		private bool parallelize = false;

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

		//(x1, y1, z1, x2, y2, z2, x3, y3, z3, n1, n2, n3)
		private volatile double[,] data;

		//Adjacency "matrix"
		private volatile List<int>[] edge_adjacencies;
		private volatile List<int>[] vertex_adjacencies;

		//Rectangular cover information for each facet, spatial lookup
		private volatile ConcurrentBag<int>[,,] box_covers_threadsafe;
		private volatile int[,,][] box_covers;

		//Undetermined number of vertices
		private volatile List<double[]> data_stream;

		private volatile int[,] facet_index_bounds;

		private bool came_from_binary_file;
		private volatile int face_count;

		private double delta_x, delta_y, delta_z;

		private volatile int x_box_count, y_box_count, z_box_count;

		//(xmin xmax ymin ymax zmin zmax)
		private volatile double[] bounds;

		public int FaceCount {get {return face_count;}}
		public double Xmin {get {return bounds[XMIN];}}
		public double Xmax {get {return bounds[XMAX];}}
		public double Ymin {get {return bounds[YMIN];}}
		public double Ymax {get {return bounds[YMAX];}}
		public double Zmin {get {return bounds[ZMIN];}}
		public double Zmax {get {return bounds[ZMAX];}}

		public Stl(string filename)
		{
			bounds = new double[]
			{
				double.PositiveInfinity,
				double.NegativeInfinity,
				double.PositiveInfinity,
				double.NegativeInfinity,
				double.PositiveInfinity,
				double.NegativeInfinity
			};
			came_from_binary_file = StlFileIsBinary(filename);
			if (came_from_binary_file)
			{
				binary_init(filename);
			}
			else
			{
				ascii_init(filename);
			}
			array_init();
			metadata_init();

			//DEBUGGING
			int min = 10000;
			int max = 0;
			List<int> debug = new List<int>();
			for (int i = 0; i < face_count; i++)
			{
				int ct = edge_adjacencies[i].Count;
				min = ct < min ? ct : min;
				max = ct > max ? ct : max;
				if (ct != 3)
				{
					debug.Add(i);
					Console.WriteLine("FACE " + i + ": " + ct + " adj");
					int imin = facet_index_bounds[i, XMIN];
					int imax = facet_index_bounds[i, XMAX];
					int jmin = facet_index_bounds[i, YMIN];
					int jmax = facet_index_bounds[i, YMAX];
					int kmin = facet_index_bounds[i, ZMIN];
					int kmax = facet_index_bounds[i, ZMAX];
					Console.WriteLine("imin: " + imin + "   imax: " + imax);
					Console.WriteLine("jmin: " + jmin + "   jmax: " + jmax);
					Console.WriteLine("kmin: " + kmin + "   kmax: " + kmax);
				}
			}
			Console.WriteLine(min);
			Console.WriteLine(max);
			DEBUG_write_ascii_exclude_face(debug.ToArray(), "test.stl");
			//DEBUGGING
		}
		private void metadata_init()
		{
			//Compute rectangular cover data
			if (parallelize)
			{
				Parallel.For(0, face_count, compute_cover_single);
				Parallel.For(0, face_count, compute_adjacency_single);
				Parallel.For(0, x_box_count, optimize_cover_data_sinlge);
			}
			else
			{
				for (int i = 0; i < face_count; i++)
				{
					compute_cover_single(i);
				}
				for (int i = 0; i < face_count; i++)
				{
					compute_adjacency_single(i);
				}
				for (int i = 0; i < x_box_count; i++)
				{
					optimize_cover_data_sinlge(i);
				}
			}
		}
		private void optimize_cover_data_sinlge(int cur_idx)
		{
			for (int j = 0; j < y_box_count; j++)
			{
				for (int k = 0; k < z_box_count; k++)
				{
					if (box_covers_threadsafe[cur_idx,j,k] != null)
					{
						box_covers[cur_idx, j, k] = box_covers_threadsafe[cur_idx,j,k].ToArray();
					}
					else
					{
						box_covers[cur_idx, j, k] = new int[0];
					}
					box_covers_threadsafe[cur_idx,j,k] = null;
				}
			}
		}
		private void compute_cover_single(int cur_idx)
		{
			//Compute rectangular box covers for each facet (thread independent). can be optimized a little by computing covers only for edges!
			int local_xmin_index = (int)Math.Floor((Utils.Min(data[cur_idx,X1], data[cur_idx,X2], data[cur_idx,X3]) - bounds[XMIN])/delta_x);
			int local_xmax_index = (int)Math.Floor((Utils.Max(data[cur_idx,X1], data[cur_idx,X2], data[cur_idx,X3]) - bounds[XMIN])/delta_x);

			int local_ymin_index = (int)Math.Floor((Utils.Min(data[cur_idx,Y1], data[cur_idx,Y2], data[cur_idx,Y3]) - bounds[YMIN])/delta_y);
			int local_ymax_index = (int)Math.Floor((Utils.Max(data[cur_idx,Y1], data[cur_idx,Y2], data[cur_idx,Y3]) - bounds[YMIN])/delta_y);

			int local_zmin_index = (int)Math.Floor((Utils.Min(data[cur_idx,Z1], data[cur_idx,Z2], data[cur_idx,Z3]) - bounds[ZMIN])/delta_z);
			int local_zmax_index = (int)Math.Floor((Utils.Max(data[cur_idx,Z1], data[cur_idx,Z2], data[cur_idx,Z3]) - bounds[ZMIN])/delta_z);

			if (local_xmax_index >= x_box_count) local_xmax_index = x_box_count - 1;
			if (local_ymax_index >= y_box_count) local_ymax_index = y_box_count - 1;
			if (local_zmax_index >= z_box_count) local_zmax_index = z_box_count - 1;

			facet_index_bounds[cur_idx, XMIN] = local_xmin_index;
			facet_index_bounds[cur_idx, XMAX] = local_xmax_index;
			facet_index_bounds[cur_idx, YMIN] = local_ymin_index;
			facet_index_bounds[cur_idx, YMAX] = local_ymax_index;
			facet_index_bounds[cur_idx, ZMIN] = local_zmin_index;
			facet_index_bounds[cur_idx, ZMAX] = local_zmax_index;


			// basic implementation for now, can definitely be optimized
			for (int i = local_xmin_index; i <= local_xmax_index; i++)
			{
				for (int j = local_ymin_index; j <= local_ymax_index; j++)
				{
					for (int k = local_zmin_index; k <= local_zmax_index; k++)
					{
						if (box_covers_threadsafe[i,j,k] == null) box_covers_threadsafe[i,j,k] = new ConcurrentBag<int>();
						box_covers_threadsafe[i,j,k].Add(cur_idx);
					}
				}
			}
		}
		private void compute_adjacency_single(int cur_idx)
		{
			edge_adjacencies[cur_idx] = new List<int>();
			vertex_adjacencies[cur_idx] = new List<int>();

			int imin = facet_index_bounds[cur_idx, XMIN];
			int imax = facet_index_bounds[cur_idx, XMAX];
			int jmin = facet_index_bounds[cur_idx, YMIN];
			int jmax = facet_index_bounds[cur_idx, YMAX];
			int kmin = facet_index_bounds[cur_idx, ZMIN];
			int kmax = facet_index_bounds[cur_idx, ZMAX];

			//Only check over the covers
			for (int i = imin; i <= imax; i++)
			{
				for (int j = jmin; j <= jmax; j++)
				{
					for (int k = kmin; k <= kmax; k++)
					{
						if (box_covers_threadsafe[i,j,k] != null)
						{
							foreach(int test_idx in box_covers_threadsafe[i,j,k])
							{
								if (test_idx != cur_idx)
								{
									bool edge_adj, vertex_adj;
									get_adjacency(cur_idx, test_idx, out edge_adj, out vertex_adj);
									if (edge_adj && !edge_adjacencies[cur_idx].Contains(test_idx)) edge_adjacencies[cur_idx].Add(test_idx);
									if (vertex_adj && !edge_adjacencies[cur_idx].Contains(test_idx)) vertex_adjacencies[cur_idx].Add(test_idx);
								}
							}
						}
					}
				}
			}
		}
		private void get_adjacency(int n1, int n2, out bool edge, out bool vertex)
		{
			double x1_1 = data[n1, X1];
			double x2_1 = data[n1, X2];
			double x3_1 = data[n1, X3];
			double y1_1 = data[n1, Y1];
			double y2_1 = data[n1, Y2];
			double y3_1 = data[n1, Y3];
			double z1_1 = data[n1, Z1];
			double z2_1 = data[n1, Z2];
			double z3_1 = data[n1, Z3];

			double x1_2 = data[n2, X1];
			double x2_2 = data[n2, X2];
			double x3_2 = data[n2, X3];
			double y1_2 = data[n2, Y1];
			double y2_2 = data[n2, Y2];
			double y3_2 = data[n2, Y3];
			double z1_2 = data[n2, Z1];
			double z2_2 = data[n2, Z2];
			double z3_2 = data[n2, Z3];

			//Ugly, but check all pairs of vertices. Probably a more efficient way to do this.
			double[] deltas = new double[]
			{
				compute_dist(x1_1, y1_1, z1_1, x1_2, y1_2, z1_2),
				compute_dist(x1_1, y1_1, z1_1, x2_2, y2_2, z2_2),
				compute_dist(x1_1, y1_1, z1_1, x3_2, y3_2, z3_2),

				compute_dist(x2_1, y2_1, z2_1, x1_2, y1_2, z1_2),
				compute_dist(x2_1, y2_1, z2_1, x2_2, y2_2, z2_2),
				compute_dist(x2_1, y2_1, z2_1, x3_2, y3_2, z3_2),

				compute_dist(x3_1, y3_1, z3_1, x1_2, y1_2, z1_2),
				compute_dist(x3_1, y3_1, z3_1, x2_2, y2_2, z2_2),
				compute_dist(x3_1, y3_1, z3_1, x3_2, y3_2, z3_2)
			};
			int num_zeros = 0;
			for (int i = 0; i < deltas.Length; i++)
			{
				if (deltas[i] < EPSILON) num_zeros++;
			}
			edge = (num_zeros > 1);
			vertex = (num_zeros > 0);
		}
		private double compute_dist(double x1, double y1, double z1, double x2, double y2, double z2)
		{
			double dx = x2 - x1;
			double dy = y2 - y1;
			double dz = z2 - z1;
			return Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}
		private void array_init()
		{
			data = new double[face_count, 12];

			//Probably needs to be parallelized.
			for (int i = 0; i < face_count; i++)
			{
				double[] cur_array = data_stream[i];
				for (int j = 0; j < 12; j++)
				{
					data[i,j] = cur_array[j];
				}
			}

			//Clear data so no duplicates stored. Probably a better way to do this.
			data_stream = new List<double[]>();
		}
		private void binary_init(string filename)
		{
			//Reads data from file, computes bounding box, computes the length control parameters
			Info.Kill(this, "binary initialization not implemented.");
		}
		private void ascii_init(string filename)
		{
			//Reads data from file, computes bounding box, computes the length control parameters
			//initialize data.
			int line_count = 0;
			double sum_x_length = 0;
			double sum_y_length = 0;
			double sum_z_length = 0;

			data_stream = new List<double[]>();
			double local_xmin, local_xmax, local_ymin, local_ymax, local_zmin, local_zmax;
			using (StreamReader reader = new StreamReader(filename))
			{
				string line = reader.ReadLine();
				line_count++;
				bool exit = line.Contains("facet normal");
				while(!exit)
				{
					line = reader.ReadLine();
					line_count++;
					exit = line.Contains("facet normal");
				}
				while (line != null)
				{
					if (line.Contains("facet normal"))
					{
						double x1, y1, z1, x2, y2, z2, x3, y3, z3, n1, n2, n3;

						//   facet normal n1 n2 n3
						string[] split = line.Trim().Split(' ', '\t');
						if (split.Length < 5) Info.Kill(this, "too few entries near line " + line_count);
						if (!double.TryParse(split[2], out n1)) Info.Kill(this, "couldn't parse entry \"" + split[2] +"\" near line " + line_count);
						if (!double.TryParse(split[3], out n2)) Info.Kill(this, "couldn't parse entry \"" + split[3] +"\" near line " + line_count);
						if (!double.TryParse(split[4], out n3)) Info.Kill(this, "couldn't parse entry \"" + split[4] +"\" near line " + line_count);

						//outer loop
						reader.ReadLine();
						line_count++;

						//vertex x1 y1 z1
						line = reader.ReadLine();
						line_count++;
						split = line.Trim().Split(' ', '\t');
						if (split.Length < 4) Info.Kill(this, "too few entries near line " + line_count);
						if (!double.TryParse(split[1], out x1)) Info.Kill(this, "couldn't parse entry \"" + split[1] +"\" near line " + line_count);
						if (!double.TryParse(split[2], out y1)) Info.Kill(this, "couldn't parse entry \"" + split[2] +"\" near line " + line_count);
						if (!double.TryParse(split[3], out z1)) Info.Kill(this, "couldn't parse entry \"" + split[3] +"\" near line " + line_count);

						//vertex x2 y2 z2
						line = reader.ReadLine();
						line_count++;
						split = line.Trim().Split(' ', '\t');
						if (split.Length < 4) Info.Kill(this, "too few entries near line " + line_count);
						if (!double.TryParse(split[1], out x2)) Info.Kill(this, "couldn't parse entry \"" + split[1] +"\" near line " + line_count);
						if (!double.TryParse(split[2], out y2)) Info.Kill(this, "couldn't parse entry \"" + split[2] +"\" near line " + line_count);
						if (!double.TryParse(split[3], out z2)) Info.Kill(this, "couldn't parse entry \"" + split[3] +"\" near line " + line_count);

						//vertex x3 y3 z3
						line = reader.ReadLine();
						line_count++;
						split = line.Trim().Split(' ', '\t');
						if (split.Length < 4) Info.Kill(this, "too few entries near line " + line_count);
						if (!double.TryParse(split[1], out x3)) Info.Kill(this, "couldn't parse entry \"" + split[1] +"\" near line " + line_count);
						if (!double.TryParse(split[2], out y3)) Info.Kill(this, "couldn't parse entry \"" + split[2] +"\" near line " + line_count);
						if (!double.TryParse(split[3], out z3)) Info.Kill(this, "couldn't parse entry \"" + split[3] +"\" near line " + line_count);

						//endloop
						reader.ReadLine();
						line_count++;

						//endfacet
						reader.ReadLine();
						line_count++;

						data_stream.Add(new double[] {x1, y1, z1, x2, y2, z2, x3, y3, z3, n1, n2, n3});

						//(xmin xmax ymin ymax zmin zmax)
						local_xmin = Utils.Min(x1, x2, x3);
						local_ymin = Utils.Min(y1, y2, y3);
						local_zmin = Utils.Min(z1, z2, z3);

						local_xmax = Utils.Max(x1, x2, x3);
						local_ymax = Utils.Max(y1, y2, y3);
						local_zmax = Utils.Max(z1, z2, z3);

						sum_x_length += local_xmax - local_xmin;
						sum_y_length += local_ymax - local_ymin;
						sum_z_length += local_zmax - local_zmin;

						bounds[XMIN] = local_xmin < bounds[XMIN] ? local_xmin : bounds[XMIN];
						bounds[XMAX] = local_xmax > bounds[XMAX] ? local_xmax : bounds[XMAX];
						bounds[YMIN] = local_ymin < bounds[YMIN] ? local_ymin : bounds[YMIN];
						bounds[YMAX] = local_ymax > bounds[YMAX] ? local_ymax : bounds[YMAX];
						bounds[ZMIN] = local_zmin < bounds[ZMIN] ? local_zmin : bounds[ZMIN];
						bounds[ZMAX] = local_zmax > bounds[ZMAX] ? local_zmax : bounds[ZMAX];
					}
					line = reader.ReadLine();
					line_count++;
				}

				face_count = data_stream.Count;

				double avg_delta_x = sum_x_length / face_count;
				double avg_delta_y = sum_y_length / face_count;
				double avg_delta_z = sum_z_length / face_count;

				//xmin + x_box_count*delta_x = xmax, etc.
				x_box_count = (int)Math.Ceiling((bounds[XMAX] - bounds[XMIN]) / avg_delta_x);
				y_box_count = (int)Math.Ceiling((bounds[YMAX] - bounds[YMIN]) / avg_delta_y);
				z_box_count = (int)Math.Ceiling((bounds[ZMAX] - bounds[ZMIN]) / avg_delta_z);

				delta_x = (bounds[XMAX] - bounds[XMIN]) / x_box_count;
				delta_y = (bounds[YMAX] - bounds[YMIN]) / y_box_count;
				delta_z = (bounds[ZMAX] - bounds[ZMIN]) / z_box_count;

				edge_adjacencies = new List<int>[face_count];
				vertex_adjacencies = new List<int>[face_count];
				box_covers_threadsafe = new ConcurrentBag<int>[x_box_count, y_box_count, z_box_count];
				box_covers = new int[x_box_count, y_box_count, z_box_count][];
				facet_index_bounds = new int[face_count, 6];
			}
		}
		public static bool StlFileIsBinary(string filename, int search_depth = 20)
		{
			string line;
			int current_count = 0;
			int good_lines = 0;
			using (StreamReader reader = new StreamReader(filename))
			{
				while((line = reader.ReadLine()) != null && current_count < search_depth)
				{
					if (line.Contains("facet") || line.Contains("loop") || line.Contains("normal") || line.Contains("solid"))
					{
						good_lines++;
					}
					current_count++;
				}
			}
			return good_lines < 3;
		}

		private void DEBUG_write_ascii_exclude_face(int[] faces_to_exclude, string filename)
		{
			using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("solid Default");
				for (int i = 0; i < face_count; i++)
				{
					if (!Utils.IntArrayContains(faces_to_exclude, i))
					{
						sw.WriteLine("  facet normal " + data[i, N1] + " " + data[i, N2] + " " + data[i, N3]);
						sw.WriteLine("    outer loop");
						sw.WriteLine("      vertex " + data[i, X1] + " " + data[i, Y1] + " " + data[i, Z1]);
						sw.WriteLine("      vertex " + data[i, X2] + " " + data[i, Y2] + " " + data[i, Z2]);
						sw.WriteLine("      vertex " + data[i, X3] + " " + data[i, Y3] + " " + data[i, Z3]);
						sw.WriteLine("    endloop");
						sw.WriteLine("  endfacet");
					}
				}
				sw.WriteLine("endsolid Default");
            }
		}
	}
}
