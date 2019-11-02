using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace raysharp
{
    public class Scene
    {
        private Background backdrop;
        private List<ILightSource> lights;
        private List<IRenderableBody> bodies;
        private Camera camera;
        public Background Backdrop {get {return backdrop;} set {backdrop = value;}}
        public Camera SceneCamera {get {return camera;} set {camera=value;}}
        public Scene(Background _backdrop, Camera _camera)
        {
            backdrop = _backdrop;
            camera = _camera;
            lights = new List<ILightSource>();
            bodies = new List<IRenderableBody>();
        }

        private volatile Ray[,] rays;
        private volatile double[,] distance_field;
        private volatile int[,] bodyid_field;
        private volatile RayImage im;
        private int nx, ny;
        private const bool par_rdr = false;
        public RayImage Render()
        {
            rays = camera.GetRays();
            im = new RayImage(camera.NX, camera.NY);
            nx = camera.NX;
            ny = camera.NY;
            distance_field = new double[nx,ny];
            bodyid_field = new int[nx, ny];
            if (par_rdr) Parallel.For(0, nx,render_single );
            else
            {
                for (int i = 0; i < nx; i++)
                {
                    render_single(i);
                }
            }
            return im;
        }
        private void render_single(int i)
        {
            for (int j = 0; j < ny; j++)
            {
                im.SetPixelXY(i,j,TraceRay(rays[i,j], out bodyid_field[i,j], out distance_field[i,j]));
            }
        }

        public void AddBody(IRenderableBody body)
        {
            bodies.Add(body);
        }
        public void AddLight(ILightSource light)
        {
            lights.Add(light);
        }
        public Triple TraceRay(Ray r, out int body_id, out double distance)
        {
            body_id = -1;
            distance = 0;
            double a;
            Triple backgroundcol = backdrop.GetBackgroundColor(r, out a);
            return new Triple (0, 0, a/40);
        }
        private Triple trace_ray_recursive(Ray input_ray, int current_depth)
        {
            return null;
        }
    }
}
