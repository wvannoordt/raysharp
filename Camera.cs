using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
	public class Camera
	{
        private Triple position;
        private Triple direction, x_axis, y_axis;
        double x_aspect, y_aspect, screen_dist, elevation, zenith;
		public double XAspect {get {return x_aspect;} set {x_aspect = value;}}
		public double YAspect {get {return y_aspect;} set {y_aspect = value;}}
		public double ScreenDist {get {return screen_dist;} set {screen_dist = value;}}
		public Triple XAxis {get {return x_axis;}}
		public Triple YAxis {get {return y_axis;}}
		public Triple Direction {get {return direction;}}
		public Triple Position {get {return position;}}
		public Camera(Triple _position, double _elevation, double _zenith, double _x_aspect, double _y_aspect, double _screen_dist)
		{
			x_aspect = _x_aspect;
			y_aspect = _y_aspect;
			screen_dist = _screen_dist;
			elevation = _elevation;
			zenith = _zenith;
			compute_axes();
            //HERE
		}
		private void compute_axes()
		{
			if (2*elevation < -Math.PI || 2*elevation > Math.PI) Info.Kill(this, "elevation must be between -0.5*PI and 0.5*PI.");
			double sin_e = Math.Sin(elevation);
			double cos_e = Math.Cos(elevation);

			double sin_ep90 = Math.Sin(elevation + 0.5*Math.PI);
			double cos_ep90 = Math.Cos(elevation + 0.5*Math.PI);

			double sin_z = Math.Sin(zenith);
			double cos_z = Math.Cos(zenith);

			double sin_z_m90 = Math.Sin(zenith - Math.PI*0.5);
			double cos_z_m90 = Math.Cos(zenith - Math.PI*0.5);
            direction = new Triple(cos_e*cos_z, cos_e*sin_z, sin_e);
            x_axis = new Triple(cos_z_m90, sin_z_m90, 0);
			y_axis = new Triple(cos_ep90*cos_z, cos_ep90*sin_z, sin_ep90);
		}
	}
}
