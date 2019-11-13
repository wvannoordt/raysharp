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
        private const double NORMAL_EPSILON = 1e-6;
        private static double GLOBAL_DIFFUSIVE_CONST_NORMAL = 0.06;
        private static double GLOBAL_DIFFUSIVE_CONST_REFLECT = 0.0006;
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
        public RayImage Render(out double[,] distances, out int[,] ids)
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
            distances = distance_field;
            ids = bodyid_field;
            return im;
        }
        public RayImage Render()
        {
            int[,] null1;
            double[,] null2;
            return Render(out null2, out null1);
        }
        private void render_single(int i)
        {
            //Console.WriteLine(i);
            for (int j = 0; j < ny; j++)
            {
                //depth = 1 is a temporary fix!!
                Triple color = TraceRay(rays[i,j], 0, 2, out bodyid_field[i,j], out distance_field[i,j]);
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
        public Triple TraceRay(Ray r, int current_depth, int max_depth, out int body_id, out double distance)
        {
            if (current_depth > max_depth)
            {
                body_id = -1;
                distance = -1;
                return new Triple(0,0,0);
            }
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
                r.Position = first_incident_point + NORMAL_EPSILON*normal_vector;
                r.Direction = r.Direction - 2*(r.Direction*normal_vector)*normal_vector;
                adjust_for_diffuse_lighting(first_incident_point, normal_vector, r, ref color, 0.4);
                int null1;
                double null2;
                if (bodies[relevant_body].BodyOpticalProperties.IsReflective)
                {
                    return (1-bodies[relevant_body].BodyOpticalProperties.Reflectivity)*color + bodies[relevant_body].BodyOpticalProperties.Reflectivity*TraceRay(r, current_depth + 1, max_depth, out null1, out null2);
                }
                else
                {
                    return color;
                }
            }

        }
        private void adjust_for_diffuse_lighting(Triple collision_point, Triple normal_vector_in, Ray reflected_ray, ref Triple color, double body_reflection_parameter)
        {
            bool shadow = true;
            foreach (ILightSource light_source in lights)
            {
                int bid;
                double dist_null;
                Triple point_null;
                Triple normal_vector_null;
                //Need the small normal correction otherwise machine error gives spottiness.
                Ray pointing_ray = light_source.ComputeDiffuseLightingRay(collision_point + NORMAL_EPSILON*normal_vector_in);
                get_relevant_body(pointing_ray, out bid, out dist_null, out point_null, out normal_vector_null);
                if (bid == BACKGROUND_ID)
                {
                    shadow = false;
                    double t = body_reflection_parameter*light_source.GetPercentLightReception(pointing_ray);
                    color = (1-t) * color + t*light_source.BaseColor;
                    double t2 = body_reflection_parameter*light_source.GetPercentLightReception(reflected_ray);
                    color = (1-t2) * color + t2*light_source.BaseColor;
                    double t3 = body_reflection_parameter*light_source.GetPercentLightReception(new Ray(collision_point, normal_vector_in));
                    color = (1-t3) * color + t3*light_source.BaseColor;
                }
            }
            if (shadow)
            {
                color = backdrop.Lightness*color;
                /*Ray face_normal_ray = new Ray(collision_point + NORMAL_EPSILON*normal_vector_in, normal_vector_in);
                Ray face_reflected_ray = new Ray(collision_point + NORMAL_EPSILON*normal_vector_in, reflected_ray.Direction);
                face_normal_ray.Position = face_normal_ray.Position + NORMAL_EPSILON * face_normal_ray.Direction;


                int bid_normal;
                double distance_normal;
                Triple normal_incident_point;
                Triple normal_vector_null;

                //adjust for normal proximity
                get_relevant_body(face_normal_ray, out bid_normal, out distance_normal, out normal_incident_point, out normal_vector_null);
                if (bid_normal != BACKGROUND_ID)
                {
                    double null1;
                    Triple null2, null3;
                    int id;
                    bool is_lit = false;
                    foreach (ILightSource light_source in lights)
                    {
                        Ray pointing_ray = light_source.ComputeDiffuseLightingRay(normal_incident_point);
                        pointing_ray.Position = pointing_ray.Position + NORMAL_EPSILON*normal_vector_null;
                        get_relevant_body(pointing_ray, out id, out null1, out null2, out null3);
                        if (id != BACKGROUND_ID)
                        {
                            is_lit = true;
                            break;
                        }
                    }
                    Triple body_target_color = bodies[bid_normal].BodyOpticalProperties.BaseColor;
                    double t = Math.Exp(-GLOBAL_DIFFUSIVE_CONST_NORMAL*distance_normal);
                    if (is_lit) color = t*body_target_color + (1-t)*color;
                }

                int bid_reflect;
                double distance_reflect;
                Triple reflect_incident_point;
                Triple reflect_vector_null;
                //adjust for reflected proximity
                get_relevant_body(face_reflected_ray, out bid_reflect, out distance_reflect, out reflect_incident_point, out reflect_vector_null);
                if (bid_reflect != BACKGROUND_ID)
                {
                    //Ray pointing_ray = light_source.ComputeDiffuseLightingRay(reflect_incident_point);
                    double null1;
                    Triple null2, null3;
                    int id;
                    bool is_lit = false;
                    foreach (ILightSource light_source in lights)
                    {
                        Ray pointing_ray = light_source.ComputeDiffuseLightingRay(reflect_incident_point);
                        pointing_ray.Position = pointing_ray.Position + NORMAL_EPSILON*reflect_vector_null;
                        get_relevant_body(pointing_ray, out id, out null1, out null2, out null3);
                        if (id != BACKGROUND_ID)
                        {
                            is_lit = true;
                            break;
                        }
                    }
                    Triple body_target_color = bodies[bid_reflect].BodyOpticalProperties.BaseColor;
                    double t = Math.Exp(-GLOBAL_DIFFUSIVE_CONST_REFLECT*distance_reflect);
                    if (is_lit) color = t*body_target_color + (1-t)*color;
                }*/
            }
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
            double[] bounds = new double[]
            {
                b.XminGlobal,
                b.XmaxGlobal,
                b.YminGlobal,
                b.YmaxGlobal,
                b.ZminGlobal,
                b.ZmaxGlobal
            };
            Triple null1, null2;
            return Utils.CheckBoxIncidence(r, bounds, out null1, out null2, out dist_estimate);
        }
    }
}
