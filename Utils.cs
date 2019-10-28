using System;
using System.IO;

namespace raysharp
{
    public static class Utils
    {
        public static bool IntArrayContains(int[] stuff, int i)
        {
            for (int k = 0; k < stuff.Length; k++)
            {
                if (stuff[k] == i) return true;
            }
            return false;
        }
        public static double Min(params double[] xs)
        {
            double output = double.PositiveInfinity;
            foreach (double x in xs)
            {
                if (x < output) output = x;
            }
            return output;
        }
        public static double Max(params double[] xs)
        {
            double output = double.NegativeInfinity;
            foreach (double x in xs)
            {
                if (x > output) output = x;
            }
            return output;
        }

        public static int Min(params int[] xs)
        {
            int output = int.MaxValue;
            foreach (int x in xs)
            {
                if (x < output) output = x;
            }
            return output;
        }
        public static int Max(params int[] xs)
        {
            int output = int.MinValue;
            foreach (int x in xs)
            {
                if (x > output) output = x;
            }
            return output;
        }
    }
}
