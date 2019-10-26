using System;
using System.IO;
using System.Collections.Generic;

namespace raysharp
{
	public class Stl
	{
		//(x1, y1, z1, x2, y2, z2, x3, y3, z3, n1, n2, n3)
		private volatile double[,] data;
		private volatile List<double[]> data_stream;
		private bool came_from_binary_file;
		public Stl(string filename)
		{
			came_from_binary_file = StlFileIsBinary(filename);
			if (came_from_binary_file)
			{
				//binary_init(filename);
			}
			else
			{
				ascii_init(filename);
			}

		}
		private void ascii_init(string filename)
		{
			int line_count = 0;
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
					}
					line = reader.ReadLine();
					line_count++;
				}
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
	}
}
