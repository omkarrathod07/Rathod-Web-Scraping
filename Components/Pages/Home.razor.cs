using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace RathodWebScraping.Components.Pages
{
    public partial class Home
    {
        private string targetUrl = "https://ygminds.com/";
        private string extractedData = "";
        private MarkupString extractedDataMarkupString => new MarkupString(extractedData);
        private bool isLoading = false;
        private string? responseMessage;
        private string? statusScrape;
        private string? statusAI;
        private string? imageBase64;
        private string? imageFileSize;
        private string? imageUrl;
        private bool isImageUrl = false;
        private List<string> imageUrls = new List<string>();

        protected async Task ServiceScrapeAndProcessing()
        {
            if (string.IsNullOrEmpty(targetUrl))
            {
                TargetUrlEmpty();
            }
            else
            {
                ScrapeingProcess();
            }
        }

        private async void ScrapeingProcess()
        {
            isLoading = true;
            responseMessage = "";
            statusAI = "";
            statusScrape = "Processing . . .";
            var ScraperResponse = await htmlScraperService.LoadHtmlFromUrlAsync(targetUrl);
            extractedData = ScraperResponse.ParsedText;
            imageUrls = ExtractImageUrls(extractedData);
            if (imageUrls.Count > 0)
            {
                foreach (var url in imageUrls)
                {
                    extractedData += "<br/><img src'" + url + "'width='100' height='100'/>";
                }
                imageUrl = imageUrls.FirstOrDefault();
                StateHasChanged();
                isLoading = false;
                statusScrape = "Processing Complete!";
            }
        }

        private List<string> ExtractImageUrls(string htmlContent)
        {
            var urls = new List<string>();
            var regex = new Regex("<img[^>]+?src=[\"'](?<url>.*?)[\"'][^>]*>", RegexOptions.IgnoreCase);
            var matches = regex.Matches(htmlContent);
            foreach (Match match in matches)
            {
                if (match.Groups["url"].Success)
                {
                    urls.Add(match.Groups["url"].Value);
                }
            }
            return urls;
        }

        private void TargetUrlEmpty()
        {
            statusScrape = "Please enter a URL.";
            return;
        }
    }
}