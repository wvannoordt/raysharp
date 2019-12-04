using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

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
            SetPixel(x, y, c);
        }
        public void SetPixelXY(int x, int y, Color c)
        {
            SetPixel(x, -1 + Height - y, c);
        }
        public void SetPixelXY(int x, int y, Triple c)
        {
            SetPixel(x, -1 + Height - y, c);
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
            if (!Directory.Exists(Path.GetDirectoryName(filename))) Info.Kill(this, " could not find part of \"" + filename + "\".");
            Image imout = (Image)bmp;
            imout.Save(filename);
        }
        public static RayImage ArrayRange(double[,] imagedata, ColorGradient c)
        {
            int ni = imagedata.GetLength(0);
            int nj = imagedata.GetLength(1);
            RayImage output = new RayImage(ni,nj);
            double minn = double.PositiveInfinity;
            double maxx = double.NegativeInfinity;
            for (int i = 0; i < ni; i++)
            {
                for (int j = 0; j < nj; j++)
                {
                    maxx = (imagedata[i,j] > maxx) ? imagedata[i,j] : maxx;
                    minn = (imagedata[i,j] < minn) ? imagedata[i,j] : minn;
                }
            }
            for (int i = 0; i < ni; i++)
            {
                for (int j = 0; j < nj; j++)
                {
                    output.SetPixel(i,j,c.GetColor(imagedata[i,j], minn, maxx));
                }
            }
            return output;
        }
        public static RayImage ArrayRangeXY(double[,] imagedata, ColorGradient c)
        {
            int ni = imagedata.GetLength(0);
            int nj = imagedata.GetLength(1);
            RayImage output = new RayImage(ni,nj);
            double minn = double.PositiveInfinity;
            double maxx = double.NegativeInfinity;
            for (int i = 0; i < ni; i++)
            {
                for (int j = 0; j < nj; j++)
                {
                    maxx = (imagedata[i,j] > maxx) ? imagedata[i,j] : maxx;
                    minn = (imagedata[i,j] < minn) ? imagedata[i,j] : minn;
                }
            }
            for (int i = 0; i < ni; i++)
            {
                for (int j = 0; j < nj; j++)
                {
                    Triple col = c.GetColor(imagedata[i,j], minn, maxx);
                    output.SetPixelXY(i,j,col);
                }
            }
            return output;
        }
        public static RayImage ArrayLogRangeXY(double[,] imagedata_in, ColorGradient c)
        {
            int ni = imagedata_in.GetLength(0);
            int nj = imagedata_in.GetLength(1);
            double[,] imagedata = new double[ni,nj];
            for (int i = 0; i < ni; i++)
            {
                for (int j = 0; j < nj; j++)
                {
                    imagedata[i,j] = Math.Log10(Math.Abs(imagedata_in[i,j]) + 1e-10);
                }
            }
            RayImage output = new RayImage(ni,nj);
            double minn = double.PositiveInfinity;
            double maxx = double.NegativeInfinity;
            for (int i = 0; i < ni; i++)
            {
                for (int j = 0; j < nj; j++)
                {
                    maxx = (imagedata[i,j] > maxx) ? imagedata[i,j] : maxx;
                    minn = (imagedata[i,j] < minn) ? imagedata[i,j] : minn;
                }
            }
            for (int i = 0; i < ni; i++)
            {
                for (int j = 0; j < nj; j++)
                {
                    output.SetPixelXY(i,j,c.GetColor(imagedata[i,j], minn, maxx));
                }
            }
            return output;
        }
        public static RayImage ArrayLogRange(double[,] imagedata_in, ColorGradient c)
        {
            int ni = imagedata_in.GetLength(0);
            int nj = imagedata_in.GetLength(1);
            double[,] imagedata = new double[ni,nj];
            for (int i = 0; i < ni; i++)
            {
                for (int j = 0; j < nj; j++)
                {
                    imagedata[i,j] = Math.Log10(Math.Abs(imagedata_in[i,j]) + 1e-10);
                }
            }
            RayImage output = new RayImage(ni,nj);
            double minn = double.PositiveInfinity;
            double maxx = double.NegativeInfinity;
            for (int i = 0; i < ni; i++)
            {
                for (int j = 0; j < nj; j++)
                {
                    maxx = (imagedata[i,j] > maxx) ? imagedata[i,j] : maxx;
                    minn = (imagedata[i,j] < minn) ? imagedata[i,j] : minn;
                }
            }
            for (int i = 0; i < ni; i++)
            {
                for (int j = 0; j < nj; j++)
                {
                    output.SetPixel(i,j,c.GetColor(imagedata[i,j], minn, maxx));
                }
            }
            return output;
        }
    }
}
