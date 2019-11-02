using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace raysharp
{
    public class Background
    {
        private Triple sky_color, floor_color, altfloor_color, horizon_color;
        private bool disable_anti_aliasing;
        private double floor_height, floor_checker_length;
        private const double HORIZON_THRESH = 0.004;
        private const double ANTIALIAS_THRESHOLD = 0.2;
        private const double FLOOR_RES_THRESHOLD = 0.004;
        private bool has_floor;
        public bool HasFloor {get {return has_floor;} set {has_floor = value;}}
        public double FloorCheckerLength {get {return floor_checker_length;} set {floor_checker_length = value;}}
        public Background(Triple _floor_color, Triple _sky_color, double _floor_height)
        {
            sky_color = _sky_color;
            floor_color = _floor_color;
            floor_height = _floor_height;
            init_hidden_defaults();
        }
        public Background(Triple _floor_color, Triple _sky_color)
        {
            sky_color = _sky_color;
            floor_color = _floor_color;
            floor_height = -10;
            init_hidden_defaults();
        }
        public Background()
        {
            sky_color = new Triple(0.48627, 0.76078, 0.95686);
            floor_color = new Triple(0.309, 0.25, 0.25);
            altfloor_color = 0.7*floor_color;
            init_hidden_defaults();
        }
        private void init_hidden_defaults()
        {
            has_floor = true;
            floor_checker_length = 5;
            if (has_floor) horizon_color = floor_color;
            else horizon_color = 0.85*floor_color;
            disable_anti_aliasing = false;
        }
        public void SetAntiAliasing(bool input)
        {
            disable_anti_aliasing = !input;
        }
        public Triple GetBackgroundColor(Ray r)
        {
            Ray nothing;
            double also_nothing;
            return GetBackgroundColor(r, out nothing, out also_nothing);
        }
        public Triple GetBackgroundColor(Ray r, out double dist)
        {
            Ray nothing;
            return GetBackgroundColor(r, out nothing, out dist);
        }
        public Triple GetBackgroundColor(Ray r, out Ray reflected, out double distance_out)
        {
            Triple loc_floor_color = floor_color;
            double dglob = -1;
            if (has_floor && r.V3 <= -0.5*HORIZON_THRESH)
            {
                double scalefactor = -r.Z/r.V3;
                Triple new_direc = new Triple(r.V1, r.V2, -r.V3);
                //Intersection point with the floor
                Triple xy_floor = new Triple(r.X+ scalefactor*r.V1, r.Y+ scalefactor*r.V2, r.Z + scalefactor*r.V3);
                reflected = new Ray(r.Position, new_direc);
                double h = r.Z - floor_height;

                //Dtermine when to do antialiasing/"fog"
                double r_floor_1 = Math.Max(Math.Abs(xy_floor.X), Math.Abs(xy_floor.Y));
                double r_floor_2 = xy_floor.Norm();
                double r_floor = 0.5*r_floor_1 + 0.5*r_floor_2;
                double distance = Math.Sqrt(r_floor*r_floor + h*h);
                //Console.WriteLine(h);
                //Console.WriteLine(r_floor_2);
                //Console.ReadLine();
                dglob = Math.Sqrt(r_floor_2*r_floor_2 + h*h);;
                distance_out = dglob;
                //"solid angle" of one chekerboard
                double impression = h*floor_checker_length / distance;
                if (impression  < ANTIALIAS_THRESHOLD) return horizon_color;
                //Antialiasing of grid
                double x_residue = (xy_floor.X%floor_checker_length)/floor_checker_length;
                if (x_residue < 0) x_residue += 1;
                double y_residue = (xy_floor.Y%floor_checker_length)/floor_checker_length;
                if (y_residue < 0) y_residue += 1;
                double residue = Utils.Min(x_residue, y_residue, 1 - x_residue, 1 - y_residue) / (Math.Sqrt(h));
                double antialiasing_affected_factor = 1;

                //What a damn mess. Looks nice though
                if (impression  < 6*ANTIALIAS_THRESHOLD)
                {
                    antialiasing_affected_factor =Math.Pow(impression/(6*ANTIALIAS_THRESHOLD), 1.4);
                }
                double horizonfactor = 1;
                if (residue <  FLOOR_RES_THRESHOLD*Utils.Max(1, 1/impression)) horizonfactor = (residue / FLOOR_RES_THRESHOLD);
                bool a = (((int)(xy_floor.X / floor_checker_length) % 2) == 0) == (xy_floor.X < 0);
                bool b = (((int)(xy_floor.Y / floor_checker_length) % 2) == 0) == (xy_floor.Y < 0);
                if (disable_anti_aliasing)
                {
                    //lazy
                    horizonfactor =  1;
                    antialiasing_affected_factor = 1;
                }
                if (a == b) loc_floor_color = antialiasing_affected_factor * (horizonfactor*altfloor_color + (1 - horizonfactor)*horizon_color) + (1-antialiasing_affected_factor)*horizon_color;
            }
            if (r.V3 > HORIZON_THRESH)
            {
                reflected = null;
                distance_out = -1;
                return sky_color;
            }
            else if (r.V3 <= -HORIZON_THRESH)
            {
                distance_out = dglob;
                reflected = null;
                return loc_floor_color;
            }
            double t = (r.V3 + HORIZON_THRESH) / (2*HORIZON_THRESH);
            t *= t*t;
            reflected = null;
            distance_out = -1;
            return (1 - t) * horizon_color + t * sky_color;
        }
        private volatile Ray[,] rays;
        private volatile RayImage im;
        private int nx, ny;
        private const bool par_rdr = false;
        public RayImage BasicRender(Camera c)
        {
            rays = c.GetRays();
            im = new RayImage(c.NX, c.NY);
            nx = c.NX;
            ny = c.NY;
            if (par_rdr) Parallel.For(0, nx,render_single );
            else
            {
                for (int i = 0; i < c.NX; i++)
                {
                    for (int j = 0; j < c.NY; j++)
                    {
                        im.SetPixelXY(i,j,GetBackgroundColor(rays[i,j]));
                    }
                }
            }
            return im;
        }
        private void render_single(int i)
        {
            for (int j = 0; j < ny; j++)
            {
                im.SetPixelXY(i,j,GetBackgroundColor(rays[i,j]));
            }
        }
    }
}
