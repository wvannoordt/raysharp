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
			Stl binary = new Stl("binary.stl");
			Stl ascii = new Stl("ascii.stl");
		}
	}
}
