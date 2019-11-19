using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
	public class ColorGradient
	{
        private Triple[] colors;
        int n;
        double dx;
        public ColorGradient(Triple[] _colors)
        {
            colors = _colors;
            n = colors.Length;
            dx = 1.0 / (n-1);
        }
        public ColorGradient(List<Triple> _colors)
        {
            colors = _colors.ToArray();
            n = colors.Length;
            dx = 1.0 / (n-1);
        }
        public Triple GetColor(double t)
        {
            int i = (int)Math.Floor(t/dx);
            if (t < 0) return colors[0];
            if (t >= 1) return colors[n-1];
            double t_local = (t - i*dx) / dx;
            return t_local*colors[i+1] + (1-t_local)*colors[i];
        }
        public Triple GetColor(double val, double min, double max)
        {
            double t = (val-min)/(max-min);
            return GetColor(t);
        }
        public static ColorGradient Rgb()
        {
            Triple[] stuff = new Triple[]
            {
                new Triple(0, 0, 1),
                new Triple(0, 1, 0),
                new Triple(1, 0, 0)
            };
            return new ColorGradient(stuff);
        }
        public static ColorGradient Jedi()
        {
            Triple[] stuff = new Triple[]
            {
                new Triple(0, 0, 0),
                new Triple(0, 0, 0.5),
                new Triple(0, 0, 1),
                new Triple(0, 1, 1),
                new Triple(1, 1, 1)
            };
            return new ColorGradient(stuff);
        }
	}
}
