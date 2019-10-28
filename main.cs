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
			CustomStopWatch w = new CustomStopWatch("Load and compute");
			w.tic();
			Stl ascii = new Stl("ascii.stl");
			w.toc();
			w.Report();
		}
	}
}
