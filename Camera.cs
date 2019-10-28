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
        private Triple x_axis, y_axis, z_axis;
        double x_aspect, y_aspect, screen_dist;
		public Camera(Triple _position, Triple _x_axis, Triple _y_axis, double _x_aspect, double _y_aspect, double screen_dist)
		{
            x_axis = _x_axis.Unit();
            y_axis = _y_axis.Unit();
            z_axis = x_axis%y_axis;
            //HERE
		}
	}
}
