using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubbishRumble.Helper
{
    public static class RandomGenerator
    {
        private static readonly Random _random = new();
        public static double NextDouble() { return _random.NextDouble(); }
        public static int Next(int min, int max) { return _random.Next(min, max); }
    }
}
