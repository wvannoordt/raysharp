using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
	public class Triple
	{
        private double[] xyz;
        public double X {get{return xyz[0];}set{xyz[0] = value;}}
        public double Y {get{return xyz[1];}set{xyz[1] = value;}}
        public double Z {get{return xyz[2];}set{xyz[2] = value;}}
        public double this[int i]
        {
            get
            {
                validate_index(i);
                return xyz[i];
            }
            set
            {
                validate_index(i);
                xyz[i] = value;
            }
        }
        private void validate_index(int i)
        {
            if (i < 0) Info.Kill(this, "index was negative (index = " + i + ").");
            if (i > 2) Info.Kill(this, "index was greater than 2 (index = " + i + ").");
        }
		public Triple clone()
		{
			return new Triple(xyz[0], xyz[1], xyz[2]);
		}
		public Triple(double _x, double _y, double _z)
		{
			xyz = new double[3];
            xyz[0] = _x;
            xyz[1] = _y;
            xyz[2] = _z;
		}
        public Triple Unit()
        {
            double norm = Norm();
            return new Triple(xyz[0]/norm, xyz[1]/norm, xyz[2]/norm);
        }

		public double Norm()
		{
			return Math.Sqrt(xyz[0]*xyz[0] + xyz[1]*xyz[1] + xyz[2]*xyz[2]);
		}
		public double NormSq()
		{
			return xyz[0]*xyz[0] + xyz[1]*xyz[1] + xyz[2]*xyz[2];
		}

        public static Triple operator *(double a, Triple T)
        {
            return new Triple(a*T.X, a*T.Y, a*T.Z);
        }
        public static Triple operator *(Triple T, double a)
        {
            return new Triple(a*T.X, a*T.Y, a*T.Z);
        }
        public static Triple operator +(Triple a, Triple b)
        {
            return new Triple(a.X+b.X, a.Y+b.Y, a.Z+b.Z);
        }
        public static Triple operator +(Triple a, double b)
        {
            return new Triple(a.X+b, a.Y+b, a.Z+b);
        }
        public static Triple operator +(double b, Triple a)
        {
            return new Triple(a.X+b, a.Y+b, a.Z+b);
        }
        public static Triple operator -(Triple a, Triple b)
        {
            return new Triple(a.X-b.X, a.Y-b.Y, a.Z-b.Z);
        }
        public static double operator *(Triple a, Triple b)
        {
            return a.X*b.X + a.Y*b.Y + a.Z*b.Z;
        }
        public static Triple operator %(Triple a, Triple b)
        {
            return new Triple(a.Y*b.Z - a.Z*b.Y,a.X*b.Z - a.Z*b.X,a.X*b.Y - a.Y*b.X);
        }
		public string ToCsvString()
		{
			return xyz[0] + "," + xyz[1] + "," + xyz[2];
		}
		public override string ToString()
		{
			return "{" + xyz[0] + "," + xyz[1] + "," + xyz[2] + "}";
		}
	}
}
