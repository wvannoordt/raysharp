using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
	public static class Testing
	{
		public static void MakeOrbitFrames()
		{
			int nx = 1320;
			int ny = 768;
			double ratio = (double)nx / (double)ny;
			double xl = ratio;
			double yl = 1;
			int N = 400;
			double dtheta = 2*Math.PI / (N-1);
			for (int i = 0; i < N; i++)
			{
				Console.WriteLine(i);
				double theta = i*dtheta;
				double height = 28 - 5*Math.Cos(theta);
				double elev = -0.17 + 0.45*Math.Cos(theta);
				Triple pos = new Triple(3*Math.Sin(theta),3*Math.Cos(theta),height);
				Camera c = new Camera(pos, elev, theta, xl, yl, 1.7);
				Background basic = new Background();
				basic.HasFloor = true;
				RayImage r = basic.BasicSerialRender(c, nx, ny);
				r.Save("frames/img" + i.ToString().PadLeft(3, '0') + ".png");
			}
		}
	}
}
