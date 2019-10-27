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
			CustomStopWatch s = new CustomStopWatch("initialize");
			s.tic();
			Stl ascii = new Stl("ascii.stl");
			s.toc();
			s.Report();
		}
	}
}
