using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			int nx = 1320;
			int ny = 768;
			double ratio = (double)nx / (double)ny;
			double xl = ratio;
			double yl = 1;
			Triple pos = new Triple(0,0,50);
			Camera c = new Camera(pos, 0, 0.1, xl, yl, 1.7);
			Background basic = new Background();
			basic.HasFloor = true;
			RayImage r = basic.BasicSerialRender(c, nx, ny);
			r.Save("first.png");
		}
	}
}
