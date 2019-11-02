using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace raysharp
{
	public class Camera
	{
		private bool par_ray_gen = false;
		private int nx, ny;
        private Triple position;
        private Triple direction, x_axis, y_axis, lower_left;
        double x_aspect, y_aspect, screen_dist, elevation, azimuth, delta_x, delta_y;
		public double XAspect {get {return x_aspect;} set {x_aspect = value;}}
		public double YAspect {get {return y_aspect;} set {y_aspect = value;}}
		public double ScreenDist {get {return screen_dist;} set {screen_dist = value;}}
		public double ElevationAngle {get {return elevation;} set {elevation = value; compute_axes();compute_ll();}}
		public double AzimuthAngle {get {return azimuth;} set {azimuth = value; compute_axes();compute_ll();}}
		public int NX {get {return nx;}}
		public int NY {get {return ny;}}
		public Triple XAxis {get {return x_axis;}}
		public Triple YAxis {get {return y_axis;}}
		public Triple Direction {get {return direction;}}
		public Triple Position {get {return position;} set {position = value;compute_ll();}}
		public Triple LowerLeft {get {return lower_left;}}
		public Camera(Triple _position, double _elevation, double _azimuth, int _nx, int _ny, double _screen_dist = 1.7)
		{
			double ratio = (double)_nx / (double)_ny;
			nx = _nx;
			ny = _ny;
			x_aspect = ratio;
			y_aspect = 1;
			screen_dist = _screen_dist;
			elevation = _elevation;
			azimuth = _azimuth;
			position = _position;
			compute_axes();
			compute_ll();
		}
		private void compute_ll()
		{
			lower_left = position + (screen_dist*direction) - (0.5*y_aspect*y_axis) - (0.5*x_aspect*x_axis);
		}
		private void compute_axes()
		{
			if (2*elevation < -Math.PI || 2*elevation > Math.PI) Info.Kill(this, "elevation must be between -0.5*PI and 0.5*PI."); //there might actually not be a need for this...
			double sin_e = Math.Sin(elevation);
			double cos_e = Math.Cos(elevation);

			double sin_ep90 = Math.Sin(elevation + 0.5*Math.PI);
			double cos_ep90 = Math.Cos(elevation + 0.5*Math.PI);

			double sin_z = Math.Sin(azimuth);
			double cos_z = Math.Cos(azimuth);

			double sin_z_m90 = Math.Sin(azimuth - Math.PI*0.5);
			double cos_z_m90 = Math.Cos(azimuth - Math.PI*0.5);
            direction = new Triple(cos_e*cos_z, cos_e*sin_z, sin_e);
            x_axis = new Triple(cos_z_m90, sin_z_m90, 0);
			y_axis = new Triple(cos_ep90*cos_z, cos_ep90*sin_z, sin_ep90);
		}

		private volatile Ray[,] rays_output;
		public Ray[,] GetRays()
		{
			rays_output = new Ray[nx, ny];
			delta_x = x_aspect / (nx - 1);
			delta_y = y_aspect / (ny - 1);
			if (par_ray_gen) Parallel.For(0, nx, make_ray_single);
			else {for (int ix = 0; ix < nx; ix++) make_ray_single(ix);}
			return rays_output;
		}
		private void make_ray_single(int ix)
		{
			for (int iy = 0; iy < ny; iy++)
			{
				Triple screen_point = lower_left + (ix*delta_x)*x_axis + (iy*delta_y)*y_axis;
				Triple vec = (screen_point - position);
				rays_output[ix, iy] = new Ray(position, vec);
			}
		}
	}
}
