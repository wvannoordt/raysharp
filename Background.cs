using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace raysharp
{
    public class Background
    {
        private Triple sky_color, floor_color, altfloor_color;
        private double floor_height, floor_checker_length;
        private const double HORIZON_THRESH = 0.01;
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
            floor_checker_length = 10;
        }
        public Triple GetBackgroundColor(Ray r)
        {
            Triple loc_floor_color = floor_color;
            if (has_floor && r.V3 <= -0.5*HORIZON_THRESH)
            {
                double scalefactor = -r.Z/r.V3;
                Triple xy_floor = new Triple(r.X+ scalefactor*r.V1, r.Y+ scalefactor*r.V2, r.Z + scalefactor*r.V3);
                bool a = (((int)(xy_floor.X / floor_checker_length) % 2) == 0) == (xy_floor.X < 0);
                bool b = (((int)(xy_floor.Y / floor_checker_length) % 2) == 0) == (xy_floor.Y  < 0);
                if (a == b) loc_floor_color = altfloor_color;
            }
            if (r.V3 > HORIZON_THRESH) return sky_color;
            else if (r.V3 <= -HORIZON_THRESH) return loc_floor_color;
            double t = (r.V3 + HORIZON_THRESH) / (2*HORIZON_THRESH);
            t *= t;
            return (1 - t) * loc_floor_color + t * sky_color;
        }
        public  RayImage BasicSerialRender(Camera c, int nx, int ny)
        {
            Ray[,] rays = c.GetRays(nx, ny);
            RayImage im = new RayImage(nx, ny);
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    im.SetPixelXY(i,j,GetBackgroundColor(rays[i,j]));
                }
            }
            return im;
        }
    }
}
