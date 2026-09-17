using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using System.Text.RegularExpressions;

namespace RathodWebScraping.Components.Pages
{
    public partial class Home
    {
        private string targetUrl = "https://breadtopia.com/";

        private string extractedData = "";
        private MarkupString ExtractedDataMarkup => new MarkupString(extractedData);
        private bool isLoading = false;

        private string? responseMessage;
        private string? statusScrape;
        private string? statusAI;

        private string? imageBase64;
        private long imageFileSize;
        private string? imageUrl;
        private string? newImageURL;
        private bool isImageURL1 = false;
        private bool isImageURL2 = false;
        private List<string> imageUrls = new List<string>();
        protected async Task ServiceScrapeAndProcess()
        {
            if (string.IsNullOrEmpty(targetUrl))
            {
                statusScrape = "Please enter a URL to scrape.";
                extractedData = "";
                responseMessage = "";
                statusAI = "";
                imageUrls.Clear();
                imageUrl = null;
                isImageURL1 = false;
                isImageURL2 = false;

                return;
            }
            else if (!Uri.IsWellFormedUriString(targetUrl, UriKind.Absolute))
            {
                statusScrape = "Not a valid URL.";
                extractedData = "";
                responseMessage = "";
                statusAI = "";
                imageUrls.Clear();
                imageUrl = null;
                isImageURL1 = false;
                isImageURL2 = false;
                return;
            }
            else
            {
                isLoading = true;

                responseMessage = "";

                imageUrls.Clear();
                imageUrl = null;
                isImageURL1 = false;
                isImageURL2 = false;

                statusAI = "";
                statusScrape = "Processing...";
                try
                {
                    var ScraperResponse = await HtmlScraperService.LoadHtmlFromUrlAsync(targetUrl);
                    extractedData = ScraperResponse.ParsedText;
                    imageUrls = ExtractImageUrls(ScraperResponse.ParsedText);

                    foreach (var url in imageUrls)
                    {
                        extractedData += "<br/><img src='" + url + "' width='100' height='100' />";
                    }

                    imageUrl = imageUrls.FirstOrDefault();

                    statusScrape = "Done";
                }
                catch (Exception ex)
                {
                    statusScrape = "An error occurred while scraping the URL...." + ex.Message;
                    extractedData = "";
                    responseMessage = "";
                    statusAI = "";
                    imageUrls.Clear();
                    imageUrl = null;
                    isImageURL1 = false;
                    isImageURL2 = false;
                }
                finally
                {
                    isLoading = false;
                    StateHasChanged();
                }

            }
        }

        private List<string> ExtractImageUrls(string htmlContent)
        {

            var urls = new List<string>();
            var regex = new Regex("<img[^>]+?src=[\"'](?<url>.*?)[\"']", RegexOptions.IgnoreCase);
            var matches = regex.Matches(htmlContent);
            foreach (Match match in matches)
            {
                urls.Add(match.Groups["url"].Value);
            }
            return urls;
        }
        private async Task ProcessUrLImage(string imageUrl, int type)
        {
            isImageURL1 = false;
            isImageURL2 = false;

            imageBase64 = null;

            isLoading = true;

            if (targetUrl == null || targetUrl == "")
            {
                statusScrape = "Please enter a URL to scrape first";
                isLoading = false;
                responseMessage = "";
                extractedData = "";
                statusAI = "";
                imageUrls.Clear();
                imageUrl = null;

                return;
            }

            if (imageUrl == null)
            {
                responseMessage = "Need a valid Image URL ...Try Scraping first";
                isLoading = false;
                return;
            }
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                responseMessage = "Not a valid Image URL.";
                isLoading = false;
                return;
            }

            if (type == 1)
            {
                isImageURL1 = true;
            }
            else if (type == 2)
            {
                isImageURL2 = true;
                newImageURL = imageUrl;
            }

            statusAI = "Processing...";
            responseMessage = null;

            var message = new ChatMessage(ChatRole.User, "What's in this image");
            var httpClient = new HttpClient();

            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            var base64Image = Convert.ToBase64String(imageBytes);
            var imageData = $"data:image/jpg;base64,{base64Image}";
            imageFileSize = imageBytes.Length;

            message.Contents.Add(new DataContent(imageData, "image/jpg"));
            var response1 = await ChatClient.GetResponseAsync(message);

            responseMessage = response1.Text;

            statusAI = "Done";
            isLoading = false;

        }
        private async Task ScrapeAndProcessMSextensions()
        {
            if (targetUrl == null || targetUrl == "")
            {
                responseMessage = "Please enter a URL to scrape first";
                isLoading = false;
                extractedData = "";
                statusAI = "";
                statusScrape = "";
                imageUrls.Clear();
                imageUrl = null;
                return;
            }

            if (!string.IsNullOrEmpty(extractedData))
            {
                isLoading = true;
                isImageURL1 = false;
                isImageURL2 = false;
                responseMessage = "";
                statusAI = "Processing...";



                var message = new ChatMessage(ChatRole.User, "Give me an overall idea what this site is about nicely format your response in HTML " + extractedData);

                try
                {
                    var response1 = await ChatClient.GetResponseAsync(message);
                    responseMessage = response1.Text;
                }
                catch (Exception ex)
                {
                    responseMessage = $"An error occurred while processing the AI response: {ex.Message}";
                    statusAI = "Error";
                }
                finally
                {
                    isLoading = false;
                    statusAI = "Done";
                    StateHasChanged();
                }

            }
            else
            {
                responseMessage = "Please Scrape the URL first ";
            }
        }
    }
}