using System;

namespace OrionApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            WebHost test = new WebHost(90);
            Console.ReadLine();
        }
    }
}