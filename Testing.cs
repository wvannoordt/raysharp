using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
	public static class Testing
	{
		public static void OrbitCube()
		{
			int nx = 1320;
			int ny = 768;
			int N = 1;
			double dtheta = 2*Math.PI/N;
			Triple cube_pos = new Triple (0,0,10);
			Background basic = new Background();
			basic.HasFloor = true;
			double radius = 10;
			double elev = -0.15;
			double height = 15;
			Triple pos = new Triple (0,0,height);
			Camera c = new Camera(pos, elev, 0, nx, ny, 0.9);
			Cube cu = new Cube(cube_pos, 4);

			Scene main_scene = new Scene(basic, c);
			main_scene.AddBody(cu);

			for (int i = 0; i < N; i++)
			{
				Console.WriteLine(i);
				double theta = i*dtheta;

				pos = new Triple(-radius*Math.Cos(theta), radius*Math.Sin(theta), height);
				main_scene.SceneCamera.AzimuthAngle = theta;
				main_scene.SceneCamera.Position = pos;

				RayImage r = main_scene.Render();

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
			int N = 40;
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
