using HtmlAgilityPack;
using System;
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
            string endingArticleUrl = WikiArticleAccessor.GetUrlForArticle(await GetUserInput()) ;


            List<string> solvedPath = await GetSolvedPath(startingArticleUrl, endingArticleUrl);

            foreach (string path in solvedPath)
            {
                await Console.Out.WriteLineAsync(path);
            }

            await Console.Out.WriteLineAsync("I am done now");
        }

        private static void PrintWelcomeMessage()
        {
            Console.WriteLine("Welcome to my attempt at making an wiki game solver");
            Console.WriteLine("How it works is simple at first you enter the starting point subject");
            Console.WriteLine("Then you enter what the ending point subject is");
            Console.WriteLine("Then i crawl through wiki and try to find a path between the two (possible the shortest)");

        }
        private static async Task<List<string>> GetSolvedPath(string currentArticle, string endingArticle, List<string> path = null)
        {
            if (path == null)
            {
                path = new List<string>();
            }


            path.Add(currentArticle);

            if (currentArticle == endingArticle)
            {
                return path;
            }

            List<string> htmlLinks = await WikiArticleAccessor.GetWikiLinksFromPage(currentArticle);

            foreach (string currentLink in htmlLinks)
            {
                if (!path.Contains(currentLink))
                {
                    await Console.Out.WriteLineAsync($"Currently checking {currentLink}, from {currentArticle}");

                    List<string> solvedPath = await GetSolvedPath(currentLink, endingArticle, path);

                    if (solvedPath != null)
                    {
                        return solvedPath;
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
