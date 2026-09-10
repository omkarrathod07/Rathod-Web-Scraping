using HtmlAgilityPack;
using Microsoft.VisualBasic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RathodWebScraping.Services
{
    public class HtmlScraperService
    {
        private readonly HttpClient _httpClient;
        public HtmlScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<HtmlDocument> LoadHtmlFromUrlAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            var pageContent = await response.Content.ReadAsStringAsync();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pageContent);
            
            return htmlDocument;
        }
    }
}
