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
			Utils.WriteCsv("times.csv", Testing.RenderSphere());
		}
	}
}
