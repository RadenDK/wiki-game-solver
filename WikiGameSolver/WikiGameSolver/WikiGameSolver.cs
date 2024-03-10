using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace WikiGameSolver
{
    internal class WikiGameSolver
    {
        private static readonly object lockObject = new object();
        private static int _articlesVisisted;
        public static async Task StartSolver()
        {
            PrintWelcomeMessage();

            await Console.Out.WriteLineAsync($"Enter starting subject: ");
            string startingArticleUrl = WikiArticleAccessor.GetUrlForArticle(await GetUserInput());
            //string startingArticleUrl = "https://en.wikipedia.org/wiki/Henrik";


            await Console.Out.WriteLineAsync($"Enter ending subject: ");
            string endingArticleUrl = WikiArticleAccessor.GetUrlForArticle(await GetUserInput());
            //string endingArticleUrl = "https://en.wikipedia.org/wiki/Heiki_Kranich";


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
            ConcurrentQueue<LinkedList<string>> linksQueue = new ConcurrentQueue<LinkedList<string>>();
            ConcurrentDictionary<string, bool> visited = new ConcurrentDictionary<string, bool>();

            LinkedList<string> startingPath = new LinkedList<string>();
            startingPath.AddLast(startingArticle);
            linksQueue.Enqueue(startingPath);

            int articlesVisited = 0;

            while (true)
            {
                LinkedList<string> currentPath;
                linksQueue.TryDequeue(out currentPath);
                if (currentPath != null && currentPath.Count > 0)
                {
                    FindOffspringsForPath(currentPath, visited, linksQueue, endingArticle);
                }
            }

            return null;
        }

        private static async Task<LinkedList<string>> FindOffspringsForPath(LinkedList<string> currentPath, ConcurrentDictionary<string, bool> visited, ConcurrentQueue<LinkedList<string>> linksQueue, string endingArticle)
        {
            List<string> htmlLinksFromCurrentArticle = await WikiArticleAccessor.GetWikiLinksFromPage(currentPath.Last.Value);
            foreach (string link in htmlLinksFromCurrentArticle)
            {
                LinkedList<string> newPath = new LinkedList<string>(currentPath);
                if (!visited.ContainsKey(link))
                {
                    PrintAmountOfArticlesSearched();
                    visited.TryAdd(link, true);
                    newPath.AddLast(link);
                    linksQueue.Enqueue(newPath);
                }
                if (link == endingArticle)
                {
                    PrintPath(newPath);
                    Environment.Exit(0);
                }
            }
            return null;
        }
        private static void PrintAmountOfArticlesSearched()
        {
            lock (lockObject)
            {
                Interlocked.Increment(ref _articlesVisisted);
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"I have looked at : {_articlesVisisted} articles");
                Console.SetCursorPosition(0, Console.CursorTop);
            }
            
        }

        private static void PrintPath(LinkedList<string> foundPath)
        {
            Console.WriteLine("This is the solved Path");
            Console.WriteLine();
            foreach (string htmlLink in foundPath)
            {
                Console.Write(htmlLink);
                Console.WriteLine();
            }
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
