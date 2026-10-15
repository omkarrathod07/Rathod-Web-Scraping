using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;

namespace RathodWebScraping.Services
{
    public class ScrapeResult
    {
        public string RawHtml { get; set; } = string.Empty;
        public string CleanText { get; set; } = string.Empty;
    }

    public class HtmlScraperService
    {
        private readonly HttpClient _httpClient;

        public HtmlScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ScrapeResult> LoadHtmlFromUrlAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var pageContent = await response.Content.ReadAsStringAsync();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pageContent);

            var nodesToRemove = htmlDocument.DocumentNode.SelectNodes("//script|//style");
            if (nodesToRemove != null)
            {
                foreach (var node in nodesToRemove)
                {
                    node.Remove();
                }
            }

            return new ScrapeResult
            {
                RawHtml = pageContent,
                CleanText = htmlDocument.DocumentNode.InnerText ?? string.Empty
            };
        }
    }
}