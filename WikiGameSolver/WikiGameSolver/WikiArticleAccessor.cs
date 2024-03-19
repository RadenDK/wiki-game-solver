using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WikiGameSolver
{
    internal class WikiArticleAccessor
    {
        private readonly string _baseWikiUrl = "https://en.wikipedia.org/wiki/";

        private HttpClient _httpClient;

        private ILogger<WikiArticleAccessor> _logger;

        public WikiArticleAccessor(ILoggerFactory loggerFactory)
        {
            _httpClient = new HttpClient();
            _logger = loggerFactory.CreateLogger<WikiArticleAccessor>();
        }

        public async Task<List<string>> FetchWikiLinksFromUrl(string wikiPageUrl)
        {

            string html = await FetchHtmlFromUrl(wikiPageUrl);

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(html);

            HtmlNodeCollection htmlLinkNodes = htmlDocument.DocumentNode.SelectNodes("//div[@id='mw-content-text']//a[not(ancestor::table)]/@href");

            List<string> wikiLinks = new List<string>();

            _logger.LogInformation($"Started finding HTML links from {wikiPageUrl}");

            if (htmlLinkNodes != null)
            {
                foreach (HtmlNode node in htmlLinkNodes)
                {
                    string wikiLink = node.GetAttributeValue("href", "");

                    string pathDelimiter = "/wiki/";

                    if (wikiLink.Contains(pathDelimiter))
                    {
                        string link = _baseWikiUrl + wikiLink.Substring(pathDelimiter.Length);
                        wikiLinks.Add(link);
                    }
                }
            }

            _logger.LogInformation($"Finished finding HTML links from {wikiPageUrl}");

            return wikiLinks;
        }

        private async Task<string> FetchHtmlFromUrl(string url)
        {

            _logger.LogInformation($"Started fetching HTML from {url}");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);

            string html = "";

            try
            {
                response.EnsureSuccessStatusCode();
                html = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Finished fetching HTML from {url}");


            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Failed to fetch HTML from {url}");
            }



            return html;
        }

        public async Task<string> GetFoundUrlFromArticleSubject(string articleSubject)
        {
            string wikiPageUrl = _baseWikiUrl + articleSubject.Replace(" ", "_");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, wikiPageUrl);

            string foundUrl = null;

            try
            {
                await Console.Out.WriteLineAsync($"Checking if there is an article for {articleSubject}");

                _logger.LogInformation($"Checking if there is an article for {articleSubject}");

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                foundUrl = response.RequestMessage.RequestUri.ToString();

                await Console.Out.WriteLineAsync("Article found!");

                _logger.LogInformation($"An article has been found for {articleSubject} the url is {foundUrl}");

            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.Message);
                _logger.LogError(e, $"{wikiPageUrl} is not valid");
            }

            return foundUrl;
        }

        public string GetUrlForArticle(string articleSubject)
        {
            return _baseWikiUrl + articleSubject.Replace(" ", "_");
        }
    }
}