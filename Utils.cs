using System;
using System.IO;

namespace raysharp
{
    public static class Utils
    {
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
    }
}
