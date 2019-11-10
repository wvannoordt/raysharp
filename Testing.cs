using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
	public static class Testing
	{
		public static double[] RenderTeapot()
		{
			int nx = 1320;
			int ny = 768;
			int N = 10;
			double dtheta = 2*Math.PI/N;
			Triple cube_pos = new Triple (0,0,12);
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

			Stl teapot_stl = new Stl("stl/tetra_scaled.stl");

			FacetBody teapot = teapot_stl.ToFacetBody(cube_pos + new Triple(5, 0, 0));

			teapot.WriteAsciiStl("stl/TESTING.stl");

			RectangularPrism floor = new RectangularPrism(new Triple(0, 0, 2), 70, 70, 1);
			RectangularPrism eq = new RectangularPrism(cube_pos, 10, 10, 10);

			ball.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.2, 0.2);
			eq.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.2, 0.2);
			teapot.BodyOpticalProperties.BaseColor = new Triple(0.7, 0.2, 0.2);

			Scene main_scene = new Scene(basic, c);
			//main_scene.AddBody(ball);
			main_scene.AddBody(floor);
			main_scene.AddBody(teapot);
			//main_scene.AddBody(eq);

			main_scene.AddLight(light);

			Scene.PAR_RENDER = true;

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
				w.tic();
				RayImage r = main_scene.Render();
				times.Add(w.toc());
				w.Report("render " + i.ToString());
				r.Save("frames/img" + (i).ToString().PadLeft(3, '0') + ".png");
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
				RayImage r = main_scene.Render();
				times.Add(w.toc());
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
				Console.WriteLine(i);
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
