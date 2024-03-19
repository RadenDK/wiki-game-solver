using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
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
        private const int MaxConcurrentTasks = 20;

        private readonly object _articleNumPrintLock = new object();
        private int _articlesVisisted;

        private CancellationTokenSource cts = new CancellationTokenSource();


        private WikiArticleAccessor _wikiArticleAccessor;

        private ILogger _logger;

        public WikiGameSolver(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WikiGameSolver>();
            _wikiArticleAccessor = new WikiArticleAccessor(loggerFactory);
        }


        public async Task StartSolver()
        {
            PrintWelcomeMessage();

            await Console.Out.WriteLineAsync($"Enter starting subject: ");
            string startingArticleUrl = await GetValidUrlFromUserArticleInput();

            await Console.Out.WriteLineAsync($"Enter ending subject: ");
            string endingArticleUrl = await GetValidUrlFromUserArticleInput();


            LinkedList<string> solvedPath = await SolvePathBetweenArticles(startingArticleUrl, endingArticleUrl);

            PrintPath(solvedPath);
        }

        private void PrintWelcomeMessage()
        {
            Console.WriteLine("Welcome to my attempt at making an wiki game solver");
            Console.WriteLine("How it works is simple at first you enter the starting point subject");
            Console.WriteLine("Then you enter what the ending point subject is");
            Console.WriteLine("Then i crawl through wiki and try to find a path between the two (possible the shortest)");
        }

        private async Task<LinkedList<string>> SolvePathBetweenArticles(string startingArticle, string endingArticle)
        {
            ConcurrentQueue<LinkedList<string>> currentUnsolvedPathsQueue = new ConcurrentQueue<LinkedList<string>>();
            ConcurrentDictionary<string, bool> visitedLinks = new ConcurrentDictionary<string, bool>();

            LinkedList<string> startingPath = new LinkedList<string>();
            startingPath.AddLast(startingArticle);
            currentUnsolvedPathsQueue.Enqueue(startingPath);

            LinkedList<string> solvedPath = null;

            _logger.LogInformation($"Begins going through queue");

            List<Task<LinkedList<string>>> tasks = new List<Task<LinkedList<string>>>();

            while (!cts.Token.IsCancellationRequested)
            {
                LinkedList<string> currentPath;
                currentUnsolvedPathsQueue.TryDequeue(out currentPath);

                if (currentPath != null && currentPath.Count > 0)
                {
                    var task = FindOffspringsForPath(currentPath, visitedLinks, currentUnsolvedPathsQueue, endingArticle);
                    tasks.Add(task);

                    if (tasks.Count >= MaxConcurrentTasks)
                    {
                        var completedTask = await Task.WhenAny(tasks);
                        tasks.Remove(completedTask);

                        if (completedTask.Result != null)
                        {
                            solvedPath = completedTask.Result;
                        }
                    }
                }
            }

            return solvedPath;

        }

        private async Task<LinkedList<string>> FindOffspringsForPath(LinkedList<string> currentPath, ConcurrentDictionary<string, bool> visited, ConcurrentQueue<LinkedList<string>> linksQueue, string endingArticle)
        {
            List<string> htmlLinksFromCurrentArticle = await _wikiArticleAccessor.FetchWikiLinksFromUrl(currentPath.Last.Value);
            PrintAmountOfArticlesSearched();
            foreach (string link in htmlLinksFromCurrentArticle)
            {

                LinkedList<string> newPath = new LinkedList<string>(currentPath);
                if (!visited.ContainsKey(link))
                {
                    visited.TryAdd(link, true);
                    newPath.AddLast(link);
                    linksQueue.Enqueue(newPath);

                }
                if (link == endingArticle)
                {
                    _logger.LogInformation($"Found ending the article");

                    cts.Cancel();
                    return newPath;
                }

            }
            _logger.LogInformation($"Added {htmlLinksFromCurrentArticle.Count} articles to Queue after looking at {currentPath.Last.Value}");
            
            return null;
        }

        private void PrintAmountOfArticlesSearched()
        {
            lock (_articleNumPrintLock)
            {
                _articlesVisisted++;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"I have looked at : {_articlesVisisted} articles");
                Console.SetCursorPosition(0, Console.CursorTop);
            }

        }

        private void PrintPath(LinkedList<string> foundPath)
        {
            Console.WriteLine("This is the solved Path");
            foreach (string htmlLink in foundPath)
            {
                Console.Write(htmlLink);
                Console.WriteLine();
            }
        }

        private async Task<string> GetValidUrlFromUserArticleInput()
        {
            _logger.LogInformation($"Getting an article from the user");

            string validUrl = null;

            while (validUrl == null)
            {
                string userArticleInput = Console.ReadLine();
                validUrl = await _wikiArticleAccessor.GetFoundUrlFromArticleSubject(userArticleInput);
            }

            return validUrl;
        }
    }
}
