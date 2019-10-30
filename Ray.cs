using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public class Ray
    {
        private double[] xyz;
        public double X  {get{return xyz[0];}set{xyz[0] = value;}}
        public double Y  {get{return xyz[1];}set{xyz[1] = value;}}
        public double Z  {get{return xyz[2];}set{xyz[2] = value;}}
        public double V1 {get{return xyz[3];}set{xyz[3] = value;}}
        public double V2 {get{return xyz[4];}set{xyz[4] = value;}}
        public double V3 {get{return xyz[5];}set{xyz[5] = value;}}
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
            if (i > 5) Info.Kill(this, "index was greater than 2 (index = " + i + ").");
        }
        public Ray(double _x, double _y, double _z, double _v1, double _v2, double _v3)
        {
            xyz = new double[6];
            xyz[0] = _x;
            xyz[1] = _y;
            xyz[2] = _z;
            xyz[3] = _v1;
            xyz[4] = _v2;
            xyz[5] = _v3;
        }
        public Ray(Triple pos, Triple dir)
        {
            
        }
    }
}
