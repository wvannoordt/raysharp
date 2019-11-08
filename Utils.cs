using System;
using System.IO;
using System.Collections.Generic;

namespace raysharp
{
    public static class Utils
    {
        public static void WriteCsv(string filename, double[] contents)
        {
            List<string> lines = new List<string>();
            foreach(double g in contents)
            {
                lines.Add(g.ToString());
            }
            File.WriteAllLines(filename, lines.ToArray());
        }
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
        public static double Min(out int idx, params double[] xs)
        {
            double output = double.PositiveInfinity;
            int candidate = 0;
            idx = 0;
            foreach (double x in xs)
            {
                if (x < output)
                {
                    output = x;
                    idx = candidate;
                }
                candidate++;
            }
            return output;
        }
        public static double MinPos(out int idx, params double[] xs)
        {
            double output = double.PositiveInfinity;
            int candidate = 0;
            idx = 0;
            foreach (double x in xs)
            {
                if (x < output && x >= 0)
                {
                    output = x;
                    idx = candidate;
                }
                candidate++;
            }
            return output;
        }
        public static double MinPos(params double[] xs)
        {
            double output = double.PositiveInfinity;
            int candidate = 0;
            foreach (double x in xs)
            {
                if (x < output && x >= 0)
                {
                    output = x;
                }
                candidate++;
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
