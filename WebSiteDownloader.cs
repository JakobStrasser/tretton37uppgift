using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tretton37uppgift
{
    partial class WebSiteDownloader : IWebSiteDownloader
    {

        private string rootURL;
        private Uri rootUri;
        private string downloadFolderPath;
        private int count;
        public int Count { get { return count; } }
        BlockingCollection<string> downloadedURLs = new BlockingCollection<string>();
        object lockingObject = new object();

        public WebSiteDownloader()
        {

        }

        public async Task StartDownload(string UrlToDownload, string DownloadPath)
        {
            count = 0;
            rootURL = UrlToDownload;
            rootUri = new Uri(rootURL);
            downloadFolderPath = DownloadPath;
            //Ensure downloadfolder exists
            Directory.CreateDirectory(downloadFolderPath);

            await DownloadPage(rootURL);

            downloadedURLs.CompleteAdding();
        }

        private async Task DownloadPage(string URL)
        {

            //Base case
            if (downloadedURLs.Contains(URL))
            {
                return;
            }
            else
            {
                downloadedURLs.Add(URL);
                Interlocked.Increment(ref count);
                Console.Write($"\rDownloaded {count} files.");
            }

            //Recursion
            // Download the HTML content of the current page
            using var httpClient = new HttpClient();
            byte[] htmlBytes = DownloadResource(httpClient, URL, 5);
            string htmlContent = System.Text.Encoding.UTF8.GetString(htmlBytes);
            // Parse HTML content
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            if (htmlDocument is null)
                return;

            // Create a subdirectory based on the relative path of the URL
            string relativePath = new Uri(rootURL).MakeRelativeUri(new Uri(URL)).ToString();
            string filepath = Path.Combine(downloadFolderPath, relativePath);
            string? fileDirectory = Path.GetDirectoryName(filepath);
            if (fileDirectory is not null)
                Directory.CreateDirectory(fileDirectory);
            else
                return;
            Directory.CreateDirectory(fileDirectory);
            // Write HTML file to disk
            string htmlFileName = Path.Combine(fileDirectory, Path.GetFileName(URL));
            lock (lockingObject)
            {
                using (var f = new FileStream(htmlFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    f.Write(new UTF8Encoding().GetBytes(htmlContent));
                }
            }
            //Download images and other static files
            DownloadResources(URL, htmlDocument, httpClient, "//img[@src]");
            DownloadResources(URL, htmlDocument, httpClient, "//link[@rel='stylesheet' or @rel='icon']/@href");
            DownloadResources(URL, htmlDocument, httpClient, "//script[@src]/@src");

            // Find and follow links to other pages
            var linkNodes = htmlDocument.DocumentNode.SelectNodes("//a[@href]");

            if (linkNodes != null)
            {
                List<Task> links = new List<Task>();
                foreach (var linkNode in linkNodes)
                {
                    string nextUrl = linkNode.Attributes["href"].Value;

                    // Check if the link is an absolute URL or a relative path
                    if (Uri.TryCreate(nextUrl, UriKind.Absolute, out Uri? absoluteUri))
                    {
                        //No external links
                        if (absoluteUri.Host != rootUri.Host)
                            continue;
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
                        links.Add(Task.Run(() => DownloadPage(nextUrl)));
                    }
                }

                await Task.WhenAll(links);

            }
        }

        private void DownloadResources(string url, HtmlDocument htmlDocument, HttpClient httpClient, string xpath)
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

                        byte[] resourceData = DownloadResource(httpClient, resourceUrl, 5);
                        lock (lockingObject)
                        {
                            using (var f = new FileStream(resourceFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                            {
                                f.Write(resourceData);
                            }
                        }
                        downloadedURLs.Add(resourceUrl);
                        Interlocked.Increment(ref count);
                    }
                }
            }

        }

        protected static byte[] DownloadResource(HttpClient httpClient, string resourceUrl, int maxtries)
        {
            int tries = 0;

            while (tries < maxtries)
            {
                tries++;
                try
                {
                    var resourceData = httpClient.GetByteArrayAsync(resourceUrl);
                    var result = resourceData.Result;
                    if (resourceData.IsCompletedSuccessfully)
                    {
                        return resourceData.Result;
                    }

                }
                catch (Exception ex)
                { 
                    Console.WriteLine($"Error when downloading {resourceUrl} on attempt {tries} of {maxtries}."); 
                }
            }
            throw new DownloadFailedException(tries, new Exception($"Failed after reaching retry limit of {maxtries}."));

        }


    }
}
