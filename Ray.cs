using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public class Ray
    {
        private Triple position, direction;
        public Triple Position {get {return position;} set{position = value;}}
        public Triple Direction {get {return direction;} set{direction = value.Unit();}}
        public double X  {get{return position[0];}set{position[0] = value;}}
        public double Y  {get{return position[1];}set{position[1] = value;}}
        public double Z  {get{return position[2];}set{position[2] = value;}}
        public double V1 {get{return direction[0];}set{direction[0] = value;}}
        public double V2 {get{return direction[1];}set{direction[1] = value;}}
        public double V3 {get{return direction[2];}set{direction[2] = value;}}
        public override string ToString()
        {
            return "{" + position[0] + "," + position[1] + "," + position[2] + "} -> {" + direction[0] + "," + direction[1] + "," + direction[2] + "}";
        }
        public Ray(double _x, double _y, double _z, double _v1, double _v2, double _v3)
        {
            position[0] = _x;
            position[1] = _y;
            position[2] = _z;
            direction[3] = _v1;
            direction[4] = _v2;
            direction[5] = _v3;
        }
        public Ray(Triple pos, Triple dir)
        {
            Triple d_unit = dir.Unit();
            position = pos;
            direction = d_unit;
        }
    }
}
