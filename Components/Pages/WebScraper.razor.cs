using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using HtmlAgilityPack;
using RathodWebScraping.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RathodWebScraping.Components.Pages
{
    public partial class WebScraper
    {
        [CascadingParameter(Name = "IsDarkMode")]
        public bool GlobalIsDarkMode { get; set; }

        private string targetUrl = "https://ygminds.com";

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
                ResetState("Please enter a URL to scrape.");
                return;
            }

            if (!Uri.IsWellFormedUriString(targetUrl, UriKind.Absolute))
            {
                ResetState("Not a valid URL.");
                return;
            }

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
                var scraperResponse = await HtmlScraperService.LoadHtmlFromUrlAsync(targetUrl);
                extractedData = scraperResponse.CleanText;
                imageUrls = ExtractImageUrlsFromHtml(scraperResponse.RawHtml);
                imageUrl = imageUrls.FirstOrDefault();

                statusScrape = "Done";
            }
            catch (Exception ex)
            {
                ResetState($"An error occurred while scraping the URL: {ex.Message}");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private List<string> ExtractImageUrlsFromHtml(string htmlContent)
        {
            var urls = new List<string>();
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var imgNodes = doc.DocumentNode.SelectNodes("//img[@src]");
            if (imgNodes != null)
            {
                foreach (var node in imgNodes)
                {
                    string src = node.GetAttributeValue("src", "").Trim();
                    if (!string.IsNullOrEmpty(src))
                    {
                        if (Uri.IsWellFormedUriString(src, UriKind.Relative) && Uri.TryCreate(new Uri(targetUrl), src, out var absoluteUri))
                        {
                            urls.Add(absoluteUri.ToString());
                        }
                        else if (Uri.IsWellFormedUriString(src, UriKind.Absolute))
                        {
                            urls.Add(src);
                        }
                    }
                }
            }
            return urls.Distinct().ToList();
        }

        private async Task ProcessUrLImage(string targetImageUrl, int type)
        {
            isImageURL1 = false;
            isImageURL2 = false;
            imageBase64 = null;
            isLoading = true;

            if (string.IsNullOrEmpty(targetUrl))
            {
                ResetState("Please enter a URL to scrape first");
                isLoading = false;
                return;
            }

            if (string.IsNullOrEmpty(targetImageUrl))
            {
                responseMessage = "Need a valid Image URL... Try Scraping first";
                isLoading = false;
                return;
            }

            if (!Uri.IsWellFormedUriString(targetImageUrl, UriKind.Absolute))
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
                newImageURL = targetImageUrl;
            }

            statusAI = "Processing...";
            responseMessage = null;

            try
            {
                var message = new ChatMessage(ChatRole.User, "What's in this image");
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(targetImageUrl);
                imageFileSize = imageBytes.Length;
                message.Contents.Add(new DataContent(new BinaryData(imageBytes), "image/jpeg"));

                var response1 = await ChatClient.GetResponseAsync(message);
                responseMessage = response1.Text;
                statusAI = "Done";
            }
            catch (Exception ex)
            {
                responseMessage = $"Vision process exception: {ex.Message}";
                statusAI = "Error";
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private async Task ScrapeAndProcessMSextensions()
        {
            if (string.IsNullOrEmpty(targetUrl))
            {
                ResetState("Please enter a URL to scrape first");
                isLoading = false;
                return;
            }

            if (!string.IsNullOrEmpty(extractedData))
            {
                await OpenAIResponse();
            }
            else
            {
                responseMessage = "Please Scrape the URL first";
            }
        }

        private async Task OpenAIResponse()
        {
            isLoading = true;
            isImageURL1 = false;
            isImageURL2 = false;
            responseMessage = "";
            statusAI = "Processing...";

            var message = new ChatMessage(ChatRole.User, "Give me an overall idea what this site is about based on this text. Nicely format your response using standard HTML blocks: " + extractedData);

            try
            {
                var response1 = await ChatClient.GetResponseAsync(message);
                responseMessage = response1.Text;
                statusAI = "Done";
            }
            catch (Exception ex)
            {
                responseMessage = $"An error occurred while processing the AI response: {ex.Message}";
                statusAI = "Error";
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private void ResetState(string scrapeStatusMessage)
        {
            statusScrape = scrapeStatusMessage;
            extractedData = "";
            responseMessage = "";
            statusAI = "";
            imageUrls.Clear();
            imageUrl = null;
            isImageURL1 = false;
            isImageURL2 = false;
        }
    }
}