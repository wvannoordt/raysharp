using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
	public static class Testing
	{
		public static void Liberty()
		{
			int nx = 1950;
			int ny = 1000;
			double radius = 280;
			double cam_z = 26;
			double theta = 0.5*Math.PI;
			Camera c = new Camera(new Triple(-radius*Math.Cos(-theta), radius*Math.Sin(-theta), cam_z), 0, 0, nx, ny, 0.9);
			c.AzimuthAngle = theta;
			Background basic = new Background();
			GlobalLightSource light = new GlobalLightSource(new Triple(1, 2, -4));
			Scene main_scene = new Scene(basic, c);
			main_scene.DoShadows = true;

			Info.WriteLine("Importing...");
			Stl stl_subject = new Stl("stl/liberty.stl");
			Info.WriteLine("Done importing.");
			FacetBody subject = stl_subject.ToFacetBody(new Triple(0, 0, 250));
			RectangularPrism floor = new RectangularPrism(new Triple(0, 0, subject.ZminGlobal - 0.15), 190, 190, 0.3);

			c.Position.Z = subject.ZminGlobal;
			c.ElevationAngle = 0.78*Math.PI / 3;

			subject.BodyOpticalProperties.BaseColor = new Triple(0.8, 0.5, 0.8);
			subject.BodyOpticalProperties.IsReflective = true;
			subject.BodyOpticalProperties.Reflectivity = 0.21;

			//main_scene.AddBody(floor);
			main_scene.AddBody(subject);
			main_scene.AddLight(light);

			double[,] dist, ts;
			int[,] id;
			Info.WriteLine("Rendering...");
			RayImage picture = main_scene.Render(out dist, out id, out ts);
			RayImage dist_picture = RayImage.ArrayLogRangeXY(dist, ColorGradient.Jedi());
			RayImage t_picture = RayImage.ArrayLogRangeXY(ts, ColorGradient.Jedi());
			Info.WriteLine("Done.");

			picture.Save("frames/lib.png");
			dist_picture.Save("frames/libdists.png");
			t_picture.Save("frames/libtimes.png");
			Utils.WriteCsv("frames/dists.csv", dist);
			Utils.WriteCsv("frames/times.csv", ts);
		}
		public static void DestroyComputer()
		{
			int nx = 1950;
			int ny = 1000;
			double radius = 25;
			double cam_z = 26;
			double theta = 0.5*Math.PI;
			Camera c = new Camera(new Triple(-radius*Math.Cos(-theta), radius*Math.Sin(-theta), cam_z), 0, 0, nx, ny, 0.9);
			c.AzimuthAngle = theta;
			Background basic = new Background();
			basic.HasFloor = false;
			basic.SkyColor = new Triple(1, 1, 1);
			GlobalLightSource light = new GlobalLightSource(new Triple(1, 2, -4));
			Scene main_scene = new Scene(basic, c);
			main_scene.DoShadows = true;

			Info.WriteLine("Importing...");
			Stl stl_subject = new Stl("stl/car-a.stl", true);
			Info.WriteLine("Done importing.");
			FacetBody subject = stl_subject.ToFacetBody(new Triple(0, 0, 25));
			RectangularPrism floor = new RectangularPrism(new Triple(0, 0, subject.ZminGlobal - 0.15), 190, 190, 0.3);

			subject.BodyOpticalProperties.BaseColor = new Triple(0.5, 0.5, 0.5);
			subject.BodyOpticalProperties.IsReflective = false;
			subject.BodyOpticalProperties.Reflectivity = 0.21;

			//main_scene.AddBody(floor);
			main_scene.AddBody(subject);
			main_scene.AddLight(light);

			double[,] dist, ts;
			int[,] id;
			Info.WriteLine("Rendering...");
			RayImage picture = main_scene.Render(out dist, out id, out ts);
			RayImage dist_picture = RayImage.ArrayLogRangeXY(dist, ColorGradient.Jedi());
			RayImage t_picture = RayImage.ArrayLogRangeXY(ts, ColorGradient.Jedi());
			Info.WriteLine("Done.");

			picture.Save("cfdstuff/car.png");
			dist_picture.Save("cfdstuff/cardists.png");
			t_picture.Save("cfdstuff/cartimes.png");
		}
		public static void RdrTimeMaps()
		{
			int nx = 800;
			int ny = 600;
			double radius = 31;
			double cam_z = 30;

			int N = 60;
			double dtheta = 2*Math.PI / N;

			for (int i = 0; i < N; i++)
			{
				Info.WriteLine(i);
				double theta = 0.5*Math.PI + i*dtheta;
				Camera c = new Camera(new Triple(-radius*Math.Cos(-theta), radius*Math.Sin(-theta), cam_z), 0, 0, nx, ny, 0.9);
				c.AzimuthAngle = theta;
				Background basic = new Background();
				GlobalLightSource light = new GlobalLightSource(new Triple(1, 2, -4));
				Scene main_scene = new Scene(basic, c);

				Stl stl_subject = new Stl("stl/cat.stl");
				FacetBody subject = stl_subject.ToFacetBody(new Triple(0, 0, 25));
				RectangularPrism floor = new RectangularPrism(new Triple(0, 0, 21.3), 190, 190, 0.3);

				subject.BodyOpticalProperties.BaseColor = new Triple(0.2, 0.2, 0.9);
				subject.BodyOpticalProperties.IsReflective = true;
				subject.BodyOpticalProperties.Reflectivity = 0.21;

				main_scene.AddBody(floor);
				main_scene.AddBody(subject);
				main_scene.AddLight(light);

				double[,] dist, ts;
				int[,] id;
				RayImage picture = main_scene.Render(out dist, out id, out ts);
				RayImage dist_picture = RayImage.ArrayLogRangeXY(dist, ColorGradient.Jedi());
				RayImage t_picture = RayImage.ArrayLogRangeXY(ts, ColorGradient.Jedi());

				picture.Save("outputdata/images/" + i.ToString().PadLeft(N.ToString().Length, '0') + ".png");
				dist_picture.Save("outputdata/dists/" + i.ToString().PadLeft(N.ToString().Length, '0') + ".png");
				t_picture.Save("outputdata/times/" + i.ToString().PadLeft(N.ToString().Length, '0') + ".png");
			}

			/*double[,] a = new double[nx, ny];
			for (int x = 0; x < nx; x++)
			{
				for (int y = 0; y < ny; y++)
				{
					a[x,y] = (double)(x+y);
				}
			}
			RayImage test = RayImage.ArrayRangeXY(a, ColorGradient.Rgb());
			test.Save("frames/s.png");*/
		}
		public static void TestRayCover()
		{
			int N = 2;
			double x0 = -1;
			double x1 = 1;
			double y0 = -1;
			double y1 = 1;
			double z0 = -1;
			double z1 = 1;
			int nx = 10;
			int ny = 10;
			int nz = 10;
			double dx = (x1 - x0) / nx;
			double dy = (y1 - y0) / ny;
			double dz = (z1 - z0) / nz;

			for (int framenum = 0; framenum < N; framenum++)
			{
				string dir_name = "ray-cover-test/data/raycover" + framenum.ToString().PadLeft(N.ToString().Length, '0');
				Directory.CreateDirectory(dir_name);

				double theta1 = 0.06*framenum;
				double phi1 = 0.4*Math.Cos(1 + 0.02*framenum);

				double theta2 = 3.141592653 + theta1;
				double phi2 = 3.141592653 - phi1;

				Triple pos1 = 4*new Triple(Math.Cos(phi1)*Math.Cos(theta1), Math.Cos(phi1)*Math.Sin(theta1), Math.Sin(phi1));
				//Triple pos2 = new Triple(Math.Cos(phi2)*Math.Cos(theta2), Math.Cos(phi2)*Math.Sin(theta2), Math.Sin(phi2));
				Triple pos2 = -1*pos1;


				Random R = new Random();
				Triple dir = new Triple(2*R.NextDouble()-1, 2*R.NextDouble()-1, 2*R.NextDouble()-1);

				Ray testray = new Ray(new Triple(2*R.NextDouble()-1,2*R.NextDouble()-1,2*R.NextDouble()-1), dir);
				Triple enter, exit;
				Triple[] intersections;

				int[][] cover = Utils.ComputeBoxRayCover(testray,x0,x1,y0,y1,z0,z1,nx,ny,nz,dx,dy,dz, out enter, out exit, out intersections);
				double[] box_def =  new double[] {x0,x1,y0,y1,z0,z1,dx,dy,dz,(double)nx, (double)ny, (double)nz};
				Utils.WriteCsv(dir_name + "/boundingbox.csv", box_def);
				Utils.WriteCsv(dir_name + "/ray.csv", new double[] {testray.Position.X, testray.Position.Y, testray.Position.Z, testray.Direction.X, testray.Direction.Y, testray.Direction.Z});
				for (int i = 0; i < cover.Length; i++)
				{
					int[] coords = cover[i];
					double[] cur_box = new double[]
					{
						x0 + coords[0]*dx,
						x0 + (coords[0]+1)*dx,
						y0 + coords[1]*dy,
						y0 + (coords[1]+1)*dy,
						z0 + coords[2]*dz,
						z0 + (coords[2]+1)*dz,
					};
					Utils.WriteCsv(dir_name + "/coverbox" + i.ToString().PadLeft(cover.Length.ToString().Length, '0') + ".csv", cur_box);
				}
				if (cover.Length > 0)
				{
					Utils.WriteCsv(dir_name + "/entry.csv", new double[] {enter.X, enter.Y, enter.Z});
					Utils.WriteCsv(dir_name + "/exit.csv", new double[] {exit.X, exit.Y, exit.Z});
					Utils.WriteCsv(dir_name + "/intersections.csv", intersections);
				}
				Info.WriteLine("Cover contains " + cover.Length + " elements.");
			}
		}
		public static double[] RenderTeapot()
		{
			int nx = 1320;
			int ny = 768;
			int N = 1;
			double dtheta = 2*Math.PI/N;
			Triple cube_pos = new Triple (0,0,12);
			Background basic = new Background();
			basic.HasFloor = true;
			double radius = 25;
			double elev = -0.15;
			double height = 15;

			//rdr times
			List<double> times = new List<double>();

			GlobalLightSource light = new GlobalLightSource(new Triple(1, 2, -4));

			Triple pos = new Triple (0,0,height);
			Camera c = new Camera(pos, elev, 0, nx, ny, 0.9);
			Sphere ball = new Sphere(cube_pos, 3);

			Stl teapot_stl = new Stl("stl/frog.stl");

			FacetBody teapot = teapot_stl.ToFacetBody(cube_pos + new Triple(5, 1.5, 0));

			teapot.WriteAsciiStl("stl/TESTING.stl");

			RectangularPrism floor = new RectangularPrism(new Triple(0, 0, 0), 70, 70, teapot.ZminGlobal * 2);
			RectangularPrism eq = new RectangularPrism(cube_pos, 10, 10, 10);

			ball.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.2, 0.2);
			eq.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.2, 0.2);
			teapot.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.2, 0.2);
			teapot.BodyOpticalProperties.IsReflective = true;
			teapot.BodyOpticalProperties.Reflectivity = 0.55;

			Scene main_scene = new Scene(basic, c);
			//main_scene.AddBody(ball);
			main_scene.AddBody(floor);
			main_scene.AddBody(teapot);
			//main_scene.AddBody(eq);

			main_scene.AddLight(light);

			Scene.PAR_RENDER = false;

			CustomStopWatch w = new CustomStopWatch();
			for (int i = 0; i < N; i++)
			{
				double theta = i*dtheta;

				pos = new Triple(-radius*Math.Cos(-theta), radius*Math.Sin(-theta), height);// + 5*Math.Sin(theta));
				ball.Move(new Triple(0, 0, -0.9));
				//pos = new Triple(-radius, 0, height);
				main_scene.SceneCamera.AzimuthAngle = theta;
				//main_scene.SceneCamera.ElevationAngle = -0.19740*Math.Sin(theta);
				main_scene.SceneCamera.Position = pos;
				double[,] dist, times_d;
				int[,] id;
				w.tic();
				RayImage r = main_scene.Render(out dist, out id, out times_d);
				times.Add(w.toc());
				w.Report("render " + i.ToString());
				r.Save("frames/img" + (i).ToString().PadLeft(3, '0') + ".png");
				Utils.WriteCsv("outputdata/ids.csv", id);
				Utils.WriteCsv("outputdata/dists.csv", dist);
				Utils.WriteCsv("outputdata/pxtimes.csv", times_d);
			}
			return times.ToArray();
		}
		public static double[] RenderSphere()
		{
			int nx = 1320;
			int ny = 768;
			int N = 2;
			double dtheta = 2*Math.PI/N;
			Triple cube_pos = new Triple (0,0,10);
			Background basic = new Background();
			basic.HasFloor = true;
			double radius = 25;
			double elev = -0.15;
			double height = 15;

			List<double> times = new List<double>();

			GlobalLightSource light = new GlobalLightSource(new Triple(1, 2, -4));

			Triple pos = new Triple (0,0,height);
			Camera c = new Camera(pos, elev, 0, nx, ny, 0.9);
			Sphere ball = new Sphere(cube_pos, 3);

			RectangularPrism floor = new RectangularPrism(new Triple(0, 0, 2), 70, 70, 1);
			RectangularPrism ceil = new RectangularPrism(new Triple(0, 0, 30), 70, 70, 1);

			ball.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.2, 0.2);

			Scene main_scene = new Scene(basic, c);
			main_scene.AddBody(ball);
			main_scene.AddBody(floor);
			main_scene.AddBody(ceil);

			main_scene.AddLight(light);
			ceil.Move(new Triple(0, 81*(70.0/(N-1)), 0));

			Scene.PAR_RENDER = true;

			CustomStopWatch w = new CustomStopWatch();
			for (int i = 0; i < N; i++)
			{
				double theta = i*dtheta;

				//pos = new Triple(-radius*Math.Cos(-theta), radius*Math.Sin(-theta), height);// + 5*Math.Sin(theta));
				ball.Move(new Triple(0, 0, -0.9));
				pos = new Triple(-radius, 0, height);
				//main_scene.SceneCamera.AzimuthAngle = theta;
				//main_scene.SceneCamera.ElevationAngle = -0.19740*Math.Sin(theta);
				main_scene.SceneCamera.Position = pos;
				w.tic();
				double[,] dist, ts;
				int[,] id;
				RayImage r = main_scene.Render(out dist, out id, out ts);
				times.Add(w.toc());
				Utils.WriteCsv("outputdata/ids.csv", id);
				Utils.WriteCsv("outputdata/dists.csv", dist);
				w.Report("render " + i.ToString());
				r.Save("frames/img" + (i).ToString().PadLeft(3, '0') + ".png");
			}
			return times.ToArray();
		}
		public static double[] OrbitCube()
		{
			int nx = 1320;
			int ny = 768;
			int N = 90;
			double dtheta = 2*Math.PI/N;
			Triple cube_pos = new Triple (0,0,10);
			Background basic = new Background();
			basic.HasFloor = true;
			double radius = 25;
			double elev = -0.15;
			double height = 15;

			List<double> times = new List<double>();

			GlobalLightSource light = new GlobalLightSource(new Triple(1, 2, -4));

			Triple pos = new Triple (0,0,height);
			Camera c = new Camera(pos, elev, 0, nx, ny, 0.9);
			RectangularPrism cu1 = new RectangularPrism(cube_pos, 5, 3, 4);
			RectangularPrism cu2 = new RectangularPrism(cube_pos + new Triple(6, 0, 0), 4, 3, 5);
			RectangularPrism cu3 = new RectangularPrism(cube_pos + new Triple(0, 6, 0), 3, 5, 4);
			RectangularPrism cu4 = new RectangularPrism(cube_pos + new Triple(0, 0, 6), 4, 5, 3);
			RectangularPrism cu5 = new RectangularPrism(cube_pos + new Triple(0, 6, 6), 5, 5, 3);
			RectangularPrism cu6 = new RectangularPrism(cube_pos + new Triple(6, 6, 6), 4, 5, 4);
			RectangularPrism cu7 = new RectangularPrism(cube_pos + new Triple(6, 6, 0), 3, 3, 5);
			RectangularPrism cu8 = new RectangularPrism(cube_pos + new Triple(6, 0, 6), 3, 3, 3);

			RectangularPrism floor = new RectangularPrism(new Triple(0, 0, 2), 40, 40, 0.3);

			cu1.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.2, 0.2);
			cu2.BodyOpticalProperties.BaseColor = new Triple(0.2, 0.7, 0.2);
			cu3.BodyOpticalProperties.BaseColor = new Triple(0.2, 0.2, 0.7);
			cu4.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.2, 0.7);
			cu5.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.7, 0.7);
			cu6.BodyOpticalProperties.BaseColor = new Triple(0.2, 0.7, 0.7);
			cu7.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.7, 0.2);
			cu8.BodyOpticalProperties.BaseColor = new Triple(0.67, 0.2, 0.4);
			floor.BodyOpticalProperties.BaseColor = new Triple(0.67, 0.67, 0.67);

			Scene main_scene = new Scene(basic, c);
			main_scene.AddBody(cu1);
			main_scene.AddBody(cu2);
			main_scene.AddBody(cu3);
			main_scene.AddBody(cu4);
			main_scene.AddBody(cu5);
			main_scene.AddBody(cu6);
			main_scene.AddBody(cu7);
			main_scene.AddBody(cu8);
			main_scene.AddBody(floor);

			main_scene.AddLight(light);
			CustomStopWatch w = new CustomStopWatch();
			for (int i = 0; i < N; i++)
			{
				double theta = i*dtheta;

				pos = new Triple(-radius*Math.Cos(-theta), radius*Math.Sin(-theta), height + 5*Math.Sin(theta));
				//pos = new Triple(-radius, 0, height);
				main_scene.SceneCamera.AzimuthAngle = theta;
				main_scene.SceneCamera.ElevationAngle = -0.19740*Math.Sin(theta);
				main_scene.SceneCamera.Position = pos;
				w.tic();
				RayImage r = main_scene.Render();
				times.Add(w.toc());
				w.Report("render " + i.ToString());
				r.Save("frames/img" + i.ToString().PadLeft(3, '0') + ".png");
			}
			return times.ToArray();
		}
		public static void RenderCube()
		{
			int nx = 1320;
			int ny = 768;
			int N = 10;
			double dtheta = 2*Math.PI/N;
			Triple cube_pos = new Triple (0,0,10);
			Background basic = new Background();
			basic.HasFloor = true;
			double radius = 10;
			double elev = -0.15;
			double height = 15;
			Triple pos = new Triple (0,0,height);
			Camera c = new Camera(pos, elev, 0, nx, ny, 0.9);
			RectangularPrism cu = new RectangularPrism(cube_pos, 4);
			RectangularPrism cu2 = new RectangularPrism(cube_pos + new Triple(6, 0, 0), 4);
			RectangularPrism cu3 = new RectangularPrism(cube_pos + new Triple(0, 6, 0), 4);
			RectangularPrism cu4 = new RectangularPrism(cube_pos + new Triple(0, 0, 6), 4);
			cu.BodyOpticalProperties.BaseColor  = new Triple(0.7, 0.2, 0.2);
			cu2.BodyOpticalProperties.BaseColor = new Triple(0.2, 0.7, 0.2);
			cu3.BodyOpticalProperties.BaseColor = new Triple(0.2, 0.2, 0.7);
			cu4.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.2, 0.7);

			Scene main_scene = new Scene(basic, c);
			main_scene.AddBody(cu);
			main_scene.AddBody(cu2);
			CustomStopWatch w = new CustomStopWatch();
			for (int i = 0; i < N; i++)
			{
				double theta = i*dtheta;

				//pos = new Triple(-radius*Math.Cos(theta), radius*Math.Sin(theta), height);
				pos = new Triple(-radius, 0, height);
				main_scene.SceneCamera.AzimuthAngle = theta;
				main_scene.SceneCamera.Position = pos;
				w.tic();
				RayImage r = main_scene.Render();
				w.toc();
				w.Report("render " + i.ToString());
				r.Save("frames/img" + i.ToString().PadLeft(3, '0') + ".png");
			}
		}
		public static void CompareWithAliasing()
		{
			int nx = 1320;
			int ny = 768;
			int N = 40;
			double theta = 0;
			double height = 30;
			double elev = 0;
			Triple pos = new Triple(0, 0, height);
			Camera c = new Camera(pos, elev, theta, nx, ny);
			Background basic = new Background();
			basic.HasFloor = true;
			RayImage r = basic.BasicRender(c);
			r.Save("frames/withaa.png");
			basic.SetAntiAliasing(false);
			RayImage r2 = basic.BasicRender(c);
			r2.Save("frames/withnoaa.png");
		}
		public static void RenderZoom()
		{
			//Confirms that 1.7 is a good number.
			int nx = 1320;
			int ny = 768;
			int N = 15;
			for (int i = 0; i < N; i++)
			{
				double theta = 0;
				double height = 30;
				double elev = 0;
				Triple pos = new Triple(0, 0, height);
				Camera c = new Camera(pos, elev, theta, nx, ny, 0.1 + 0.1*i);
				Background basic = new Background();
				basic.HasFloor = true;
				RayImage r = basic.BasicRender(c);
				r.Save("frames/img" + i.ToString().PadLeft(3, '0') + ".png");
			}
		}
		public static void MakeOrbitFrames()
		{
			int nx = 1320;
			int ny = 768;
			int N = 400;
			double dtheta = 2*Math.PI / (N-1);
			for (int i = 0; i < N; i++)
			{
				Info.WriteLine(i);
				double theta = i*dtheta;
				double height = 28 - 5*Math.Cos(theta);
				double elev = -0.17 + 0.45*Math.Cos(theta);
				Triple pos = new Triple(3*Math.Sin(theta),3*Math.Cos(theta),height);
				Camera c = new Camera(pos, elev, theta, nx, ny);
				Background basic = new Background();
				basic.HasFloor = true;
				RayImage r = basic.BasicRender(c);
				r.Save("frames/img" + i.ToString().PadLeft(3, '0') + ".png");
			}
		}
		public static void RenderHeights()
		{
			int nx = 1320;
			int ny = 768;
			int N = 15;
			CustomStopWatch w = new CustomStopWatch();
			for (int i = 0; i < N; i++)
			{
				w.tic();
				double height = 0.5 + 5*i;
				Triple pos = new Triple(0, 0, height);
				Camera c = new Camera(pos, -0.07, 0.2*i, nx, ny);
				Background basic = new Background();
				basic.HasFloor = true;
				RayImage r = basic.BasicRender(c);
				w.toc();
				w.Report("render " + i.ToString());
				w.tic();
				r.Save("frames/img" + i.ToString().PadLeft(3, '0') + ".png");
				w.toc();
				w.Report("save " + i.ToString());
			}
		}
	}
}
