using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;


string rootURL = @"http://books.toscrape.com/index.html";
string downloadFolderPath = @"C:\temp\bookstoscrapecom\";
List<string> downloadedURLs = new List<string>();
int Count = 0;

Directory.CreateDirectory(downloadFolderPath);

using HttpClient httpClient = new HttpClient();

Console.WriteLine($"Downloading {rootURL} to {downloadFolderPath}");

await downloadPage(rootURL, downloadFolderPath, httpClient, downloadedURLs);

Console.WriteLine($"Download {Count} pages.");

async Task downloadPage(string URL, string downloadFolderPath, HttpClient httpClient, List<string> downloadedURLs)
{
    //Base case
    if (downloadedURLs.Contains(URL))
    {
        return;
    }
    else
    {
        downloadedURLs.Add(URL);
        Count++;
        Console.Write($"\rDownloaded {Count} pages.");
    }

    //Recursion
    // Download the HTML content of the current page
    string htmlContent = await httpClient.GetStringAsync(URL);

    // Parse HTML content
    HtmlDocument htmlDocument = new HtmlDocument();
    htmlDocument.LoadHtml(htmlContent);

    // Create a subdirectory based on the relative path of the URL
    string relativePath = new Uri(rootURL).MakeRelativeUri(new Uri(URL)).ToString()
    string subdirectory = Path.Combine(downloadFolderPath, relativePath);
    Directory.CreateDirectory(subdirectory);
    // Write HTML file to disk
    string htmlFileName = Path.Combine(subdirectory, Path.GetFileName(URL));
    File.WriteAllText(htmlFileName, htmlContent);

    // Find and follow links to other pages
    var linkNodes = htmlDocument.DocumentNode.SelectNodes("//a[@href]");
    if (linkNodes != null)
    {
        foreach (var linkNode in linkNodes)
        {
            string nextUrl = linkNode.Attributes["href"].Value;

            // Check if the link is an absolute URL or a relative path
            if (Uri.TryCreate(nextUrl, UriKind.Absolute, out Uri? absoluteUri))
            {
                // If it's an absolute URL, use it as is
                nextUrl = absoluteUri.ToString();
            }
            else
            {
                // If it's a relative path, combine it with the base URL
                nextUrl = new Uri(new Uri(URL), nextUrl).ToString();
            }
            // Recursively crawl the next URL
            if (downloadedURLs.Contains(nextUrl))
            {
                continue;
            }
            else
            {
                await downloadPage(nextUrl, downloadFolderPath, httpClient, downloadedURLs);
            }
        }
    }
}