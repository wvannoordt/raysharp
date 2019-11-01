using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace raysharp
{
    public class Scene
    {
        private Background backdrop;
        private List<ILightSource> lights;
        private List<IRenderableBody> bodies;
        Triple TraceRay(Ray r, out int body_id, out double distance)
        {
            body_id = -1;
            distance = 0;
            return null;
        }
    }
}
