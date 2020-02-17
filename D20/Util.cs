using System;
using System.Linq;
using System.Collections.Generic;

namespace D20
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }

    class Roller
    {
        private static Random rng = new Random();

        public static List<int> Roll(List<int> dice)
        {
            return dice.Select(x => rng.Next(1, x + 1)).ToList();
        }
    }
}