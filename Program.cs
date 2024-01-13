using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;


string rootURL = @"http://books.toscrape.com/index.html";
Uri rootUri = new Uri(rootURL);
string downloadFolderPath = @"C:\temp\bookstoscrapecom\";
List<string> downloadedURLs = new List<string>();
int Count = 0;

Directory.CreateDirectory(downloadFolderPath);

using HttpClient httpClient = new HttpClient();

Console.WriteLine($"Downloading {rootURL} to {downloadFolderPath}");

await DownloadPage(rootURL, downloadFolderPath, httpClient, downloadedURLs);

Console.WriteLine($"Download {Count} pages.");

async Task DownloadPage(string URL, string downloadFolderPath, HttpClient httpClient, List<string> downloadedURLs)
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
        Console.Write($"\rDownloaded {Count} files.");
    }

    //Recursion
    // Download the HTML content of the current page
    string htmlContent = await httpClient.GetStringAsync(URL);

    // Parse HTML content
    HtmlDocument htmlDocument = new HtmlDocument();
    htmlDocument.LoadHtml(htmlContent);

    if (htmlDocument is null)
        return;

    // Create a subdirectory based on the relative path of the URL
    string relativePath = new Uri(rootURL).MakeRelativeUri(new Uri(URL)).ToString();
    string filepath =Path.Combine(downloadFolderPath, relativePath);
    string? fileDirectory = Path.GetDirectoryName(filepath);
    if (fileDirectory is not null)
        Directory.CreateDirectory(fileDirectory);
    else
        return;
    Directory.CreateDirectory(fileDirectory);
    // Write HTML file to disk
    string htmlFileName = Path.Combine(fileDirectory, Path.GetFileName(URL));
    File.WriteAllText(htmlFileName, htmlContent);

    //Download images and other static files
    await DownloadResources(URL, htmlDocument, fileDirectory, httpClient, "//img[@src]");
    await DownloadResources(URL, htmlDocument, fileDirectory, httpClient, "//link[@rel='stylesheet' or @rel='icon']/@href");
    await DownloadResources(URL, htmlDocument, fileDirectory, httpClient, "//script[@src]/@src");

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
                await DownloadPage(nextUrl, downloadFolderPath, httpClient, downloadedURLs);
            }
        }
    }
}

async Task DownloadResources(string url, HtmlDocument htmlDocument, string downloadFolderPath, HttpClient httpClient, string xpath)
{
    var resourceNodes = htmlDocument.DocumentNode.SelectNodes(xpath);
    if (resourceNodes != null)
    {
        foreach (var resourceNode in resourceNodes)
        {
            //src if not href
            string resourceUrl = resourceNode.GetAttributeValue("href", resourceNode.GetAttributeValue("src", ""));
            if (Uri.TryCreate(resourceUrl, UriKind.Absolute, out Uri? absoluteUri))
            {
                //Don't download external files
                if (absoluteUri.Host != rootUri.Host)
                    continue;
                // If it's an absolute URL, use it as is
                resourceUrl = absoluteUri.ToString();
            }
            else
            {
                // If it's a relative path, combine it with the base URL
                resourceUrl = new Uri(new Uri(url), resourceUrl).ToString();
            }
            if (downloadedURLs.Contains(resourceUrl)) continue;
            else
            {

                string resourceRelativePath = new Uri(rootURL).MakeRelativeUri(new Uri(resourceUrl)).ToString();
                string resourceCombinedPath = Path.Combine(downloadFolderPath, resourceRelativePath);
                string? resourceDirectory = Path.GetDirectoryName(resourceCombinedPath);
                if (resourceDirectory is not null)
                    Directory.CreateDirectory(resourceDirectory);
                else
                    continue;
                string resourceFileName = Path.Combine(resourceDirectory, Path.GetFileName(resourceUrl));

                byte[] resourceData = await httpClient.GetByteArrayAsync(resourceUrl);
                File.WriteAllBytes(resourceFileName, resourceData);
                downloadedURLs.Add(resourceUrl);
                Count++;
            }
        }
    }
}