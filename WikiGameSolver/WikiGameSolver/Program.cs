using HtmlAgilityPack;
using System;
using System.Runtime.CompilerServices;

namespace WikiGameSolver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 0 || args.Length != 2)
            {
                await WikiGameSolver.StartSolver();

            }
            if (args.Length != 2)
            {
                await WikiGameSolver.StartSolver(args[1] args[2]);

            }
            else
            {
                await Console.Out.WriteLineAsync("Invalid Args input");


            }
        }
    }
}