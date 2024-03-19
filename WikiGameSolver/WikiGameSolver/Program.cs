using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using Serilog;

namespace WikiGameSolver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ILoggerFactory loggerFactory = CreateLogger();

            WikiGameSolver wikiSolver = new WikiGameSolver(loggerFactory);

            await wikiSolver.StartSolver();

        }

        static ILoggerFactory CreateLogger()
        {
            string logFilePath = "C:\\Users\\Rasmus\\Documents\\GitHub\\wiki-game-solver\\WikiGameSolver\\log.txt";

            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }

            Log.Logger = new LoggerConfiguration()
        .WriteTo.File(logFilePath,
         rollOnFileSizeLimit: true,
         retainedFileCountLimit: 1)
        .CreateLogger();


            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddSerilog() // Use Serilog as the logging provider
                    .SetMinimumLevel(LogLevel.Debug); // Set log level
            });

            return loggerFactory;

        }

    }
}