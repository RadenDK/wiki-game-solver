using HtmlAgilityPack;
using System;
using System.Runtime.CompilerServices;

namespace WikiGameSolver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await WikiGameSolver.StartSolver();
        }
    }
}