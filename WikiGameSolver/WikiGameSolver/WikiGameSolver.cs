using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiGameSolver
{
    internal class WikiGameSolver
    {
        public static async Task StartSolver()
        {
            PrintWelcomeMessage();

            await Console.Out.WriteLineAsync($"Enter starting subject: ");
            string startingArticleUrl = WikiArticleAccessor.GetUrlForArticle(await GetUserInput());


            await Console.Out.WriteLineAsync($"Enter ending subject: ");
            string endingArticleUrl = WikiArticleAccessor.GetUrlForArticle(await GetUserInput());


            Task<IEnumerable<string>> solvedPath = GetSolvedPath(startingArticleUrl, endingArticleUrl);

            await solvedPath;

            await Console.Out.WriteLineAsync("I am done now");
        }

        private static void PrintWelcomeMessage()
        {
            Console.WriteLine("Welcome to my attempt at making an wiki game solver");
            Console.WriteLine("How it works is simple at first you enter the starting point subject");
            Console.WriteLine("Then you enter what the ending point subject is");
            Console.WriteLine("Then i crawl through wiki and try to find a path between the two (possible the shortest)");

        }

        private static async Task<IEnumerable<string>> GetSolvedPath(string startingArticle, string endingArticle)
        {
            ConcurrentQueue<List<string>> pathQueue = new ConcurrentQueue<List<string>>();
            HashSet<string> visited = new HashSet<string>();

            List<string> startingPoint = new List<string>() { startingArticle };
            pathQueue.Enqueue(startingPoint);

            bool lookingForPath = true;
            while (lookingForPath)
            {
                List<string> currPath;
                pathQueue.TryDequeue(out currPath);
                await Console.Out.WriteLineAsync(string.Join(" -> ", currPath));
                Task<List<string>> linksInArticle = WikiArticleAccessor.GetWikiLinksFromPage(currPath.Last());
                foreach (string nextLink in linksInArticle.Result)
                {
                    if (!visited.Contains(nextLink))
                    {
                        visited.Add(nextLink);
                        List<string> newPath = new List<string>(currPath) { nextLink };
                        pathQueue.Enqueue(newPath);
                        if (nextLink == endingArticle)
                        {
                            lookingForPath = false; 
                            return newPath;
                        }
                    }
                }
            }
            return null;
        }

        private static async Task<string> GetUserInput()
        {
            string articleSubject = "";


            articleSubject = Console.ReadLine();

            while (!await WikiArticleAccessor.WikiArticleUrlIsValid(articleSubject))
            {
                await Console.Out.WriteLineAsync("Invalid article subject please try again");
                articleSubject = Console.ReadLine();
            }

            return articleSubject;
        }


    }
}
