using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiGameSolver
{
    internal class WikiArticleAccessor
    {

        private static readonly string _baseWikiUrl = "https://en.wikipedia.org/wiki/";
        public static async Task<List<string>> GetWikiLinksFromPage(string wikiPageUrl)
        {
            string html = await GetHtmlFromUrl(wikiPageUrl);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            HtmlNodeCollection htmlLinkNodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='mw-content-text']//a[not(ancestor::table)]/@href");

            List<string> wikiLinks = new List<string>();
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

            return wikiLinks;
        }

        private static async Task<string> GetHtmlFromUrl(string url)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request);

            string html = "";

            try
            {
                response.EnsureSuccessStatusCode();
                html = await response.Content.ReadAsStringAsync();

            }
            catch (HttpRequestException)
            {
            }
            return html;
        }

        public static async Task<bool> WikiArticleUrlIsValid(string articleSubject)
        {
            string wikiPageUrl = _baseWikiUrl + articleSubject.Replace(" ", "_");


            var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, wikiPageUrl);

            bool isValid = true;
            try
            {
                await Console.Out.WriteLineAsync($"Checking if there is an article for {articleSubject}");
                await Console.Out.WriteLineAsync($"Checking url {wikiPageUrl}");

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                await Console.Out.WriteLineAsync("Article found!");
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.Message);
                isValid = false;
            }

            return isValid;
        }

        public static string GetUrlForArticle(string articleSubject)
        {
            return _baseWikiUrl + articleSubject.Replace(" ", "_");
        }
    }
}
