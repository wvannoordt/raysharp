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
        private const int NULL_BODY_IDX = -2;
        private const int BACKGROUND_ID = -1;
        private Camera camera;
        public Background Backdrop {get {return backdrop;} set {backdrop = value;}}
        public Camera SceneCamera {get {return camera;} set {camera=value;}}
        public Scene(Background _backdrop, Camera _camera)
        {
            backdrop = _backdrop;
            camera = _camera;
            lights = new List<ILightSource>();
            bodies = new List<IRenderableBody>();
            par_rdr = true;
        }

        private volatile Ray[,] rays;
        private volatile double[,] distance_field;
        private volatile int[,] bodyid_field;
        private volatile RayImage im;
        private int nx, ny;
        private static bool par_rdr;
        public static bool PAR_RENDER {set {par_rdr = value;}}
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
                //depth = 1 is a temporary fix!!
                Triple color = TraceRay(rays[i,j], 1, out bodyid_field[i,j], out distance_field[i,j]);
                im.SetPixelXY(i,j,color);
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
        public Triple TraceRay(Ray r, int max_depth, out int body_id, out double distance)
        {
            //The fact that the body id and the distance are returned warrants
            //separate computation for the level 0 pass. The primary pass will also need lighting rays.
            int relevant_body = -1;
            double relevant_first_body_distance = -1;
            Triple first_incident_point;
            Triple normal_vector;
            get_relevant_body(r, out relevant_body, out relevant_first_body_distance, out first_incident_point, out normal_vector);
            body_id = relevant_body;
            distance = relevant_first_body_distance;
            //trace_ray_recursive(r, 0, 1, NULL_BODY_IDX);
            if (relevant_body == BACKGROUND_ID)
            {
                Triple backdropcolor = backdrop.GetBackgroundColor(r, out relevant_first_body_distance);
                distance = relevant_first_body_distance;
                return backdropcolor;
            }
            else
            {
                //Console.WriteLine(distance);
                Triple color = bodies[relevant_body].BodyOpticalProperties.BaseColor.clone();

                //Transform the input ray as to not take up extra memory.
                r.Position = first_incident_point;
                r.Direction = r.Direction - 2*(r.Direction*normal_vector)*normal_vector;
                adjust_for_diffuse_lighting(first_incident_point, normal_vector, r, ref color, 0.4);
                return color;
            }

        }
        private void adjust_for_diffuse_lighting(Triple collision_point, Triple normal_vector_in, Ray reflected_ray, ref Triple color, double body_reflection_parameter)
        {
            foreach (ILightSource light_source in lights)
            {
                int bid;
                double dist_null;
                Triple point_null;
                Triple normal_vector_null;
                //Need the small normal correction otherwise machine error gives spottiness.
                Ray pointing_ray = light_source.ComputeDiffuseLightingRay(collision_point + (1e-6)*normal_vector_in);
                get_relevant_body(pointing_ray, out bid, out dist_null, out point_null, out normal_vector_null);
                if (bid == BACKGROUND_ID)
                {
                    double t = body_reflection_parameter*light_source.GetPercentLightReception(pointing_ray);
                    color = (1-t) * color + t*light_source.BaseColor;
                    double t2 = body_reflection_parameter*light_source.GetPercentLightReception(reflected_ray);
                    color = (1-t2) * color + t2*light_source.BaseColor;
                    double t3 = body_reflection_parameter*light_source.GetPercentLightReception(new Ray(collision_point, normal_vector_in));
                    color = (1-t3) * color + t3*light_source.BaseColor;
                }
            }
        }
        private Triple trace_ray_recursive(Ray input_ray, int current_depth, int max_depth, int force_body, Triple current_color)
        {
            double a = 0;
            bool incident = false;
            foreach (IRenderableBody body in bodies)
            {
                incident = incident || check_bounding_box_incidence(input_ray, body, out a);
            }
            if (incident) return new Triple(0, 1, 0);
            return backdrop.GetBackgroundColor(input_ray, out a);
        }

        private void get_relevant_body(Ray input, out int body_id, out double dist, out Triple point_of_incidence, out Triple normal_vector_out)
        {
            double min_dist = -1;
            double dummy;
            normal_vector_out = null;
            point_of_incidence = backdrop.GetFloorPoint(input);
            body_id = BACKGROUND_ID;
            List<int> candidate_ids = new List<int>();
            for (int i = 0; i < bodies.Count; i++)
            {
                if (check_bounding_box_incidence(input, bodies[i], out dummy))
                {
                    candidate_ids.Add(i);
                }
            }
            foreach (int cur_idx in candidate_ids)
            {
                double current_dist;
                Triple current_point_of_incidence;
                Triple normal_vector;
                if (bodies[cur_idx].CheckIncidence(input, out current_dist, out current_point_of_incidence, out normal_vector))
                {
                    if (min_dist < 0 || current_dist < min_dist)
                    {
                        point_of_incidence = current_point_of_incidence;
                        min_dist = current_dist;
                        body_id = cur_idx;
                        normal_vector_out = normal_vector;
                    }
                }
            }
            dist = min_dist;
        }

        private bool check_bounding_box_incidence(Ray r, IRenderableBody b, out double dist_estimate)
        {
            //Quadrant check
            bool xmin_possible = Math.Sign(b.XminGlobal - r.X) == Math.Sign(r.V1);
            bool xmax_possible = Math.Sign(b.XmaxGlobal - r.X) == Math.Sign(r.V1);
            bool ymin_possible = Math.Sign(b.YminGlobal - r.Y) == Math.Sign(r.V2);
            bool ymax_possible = Math.Sign(b.YmaxGlobal - r.Y) == Math.Sign(r.V2);
            bool zmin_possible = Math.Sign(b.ZminGlobal - r.Z) == Math.Sign(r.V3);
            bool zmax_possible = Math.Sign(b.ZmaxGlobal - r.Z) == Math.Sign(r.V3);
            if (!((xmin_possible||xmax_possible) && (ymin_possible||ymax_possible) && (zmin_possible||zmax_possible)))
            {
                dist_estimate = -1;
                return false;
            }

            //Bounding box check
            double[] dists = new double[]
            {
                (b.XminGlobal - r.X)/r.V1, //xmin
                (b.XmaxGlobal - r.X)/r.V1, //xmax
                (b.YminGlobal - r.Y)/r.V2, //ymin
                (b.YmaxGlobal - r.Y)/r.V2, //ymax
                (b.ZminGlobal - r.Z)/r.V3, //zmin
                (b.ZmaxGlobal - r.Z)/r.V3 //zmax
            };

            bool confirm = false;
            double cur_min_dist = -1;
            Triple xmin_position = r.Position + dists[0]*r.Direction;
            if (xmin_position.Y <= b.YmaxGlobal && xmin_position.Y > b.YminGlobal && xmin_position.Z <= b.ZmaxGlobal && xmin_position.Z > b.ZminGlobal)
            {
                if (cur_min_dist < 0 || dists[0] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[0];
                }
            }
            Triple xmax_position = r.Position + dists[1]*r.Direction;
            if (xmax_position.Y <= b.YmaxGlobal && xmax_position.Y > b.YminGlobal && xmax_position.Z <= b.ZmaxGlobal && xmax_position.Z > b.ZminGlobal)
            {
                if (cur_min_dist < 0 || dists[1] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[1];
                }
            }
            Triple ymin_position = r.Position + dists[2]*r.Direction;
            if (ymin_position.Z <= b.ZmaxGlobal && ymin_position.Z > b.ZminGlobal && ymin_position.X <= b.XmaxGlobal && ymin_position.X > b.XminGlobal)
            {
                if (cur_min_dist < 0 || dists[2] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[2];
                }
            }
            Triple ymax_position = r.Position + dists[3]*r.Direction;
            if (ymax_position.Z <= b.ZmaxGlobal && ymax_position.Z > b.ZminGlobal && ymax_position.X <= b.XmaxGlobal && ymax_position.X > b.XminGlobal)
            {
                if (cur_min_dist < 0 || dists[3] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[3];
                }
            }
            Triple zmin_position = r.Position + dists[4]*r.Direction;
            if (zmin_position.X <= b.XmaxGlobal && zmin_position.X > b.XminGlobal && zmin_position.Y <= b.YmaxGlobal && zmin_position.Y > b.YminGlobal)
            {
                if (cur_min_dist < 0 || dists[4] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[4];
                }
            }
            Triple zmax_position = r.Position + dists[5]*r.Direction;
            if (zmax_position.X <= b.XmaxGlobal && zmax_position.X > b.XminGlobal && zmax_position.Y <= b.YmaxGlobal && zmax_position.Y > b.YminGlobal)
            {
                if (cur_min_dist < 0 || dists[5] < cur_min_dist)
                {
                    confirm = true;
                    cur_min_dist = dists[5];
                }
            }
            if (confirm)
            {
                dist_estimate = cur_min_dist;
                return true;
            }
            dist_estimate = -1;
            return false;
        }
    }
}
