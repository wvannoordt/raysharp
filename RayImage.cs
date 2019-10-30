using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace raysharp
{
    public class RayImage : IDisposable
    {
        public Bitmap bmp { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public RayImage(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            bmp = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }
        public void SerialFill(Triple color)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    SetPixel(i,j,color);
                }
            }
        }
        public void SetPixel(int x, int y, Color color)
        {
            int index = x + (y * Width);
            int col = color.ToArgb();
            Bits[index] = col;
        }
        public void SetPixel(int x, int y, Triple color)
        {
            Color c = Color.FromArgb(255, restrict(color.X), restrict(color.Y), restrict(color.Z));
            /*Console.Write(restrict(color.X));
            Console.Write(",");
            Console.Write(restrict(color.Y));
            Console.Write(",");
            Console.WriteLine(restrict(color.Z));*/
            SetPixel(x, y, c);
        }
        private int restrict(double a)
        {
            if (a < 0.0) return 0;
            if (a > 1.0) return 255;
            return (int)(255.0*a);
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            bmp.Dispose();
            BitsHandle.Free();
        }
        public void Save(string filename)
        {
            Image imout = (Image)bmp;
            imout.Save(filename);
        }
    }
}
