using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;


string rootURL = @"http://books.toscrape.com/index.html";
string downloadFolderPath = @"C:\temp\bookstoscrapecom\";
List<string> downloadedURLs = new List<string>();

Directory.CreateDirectory(downloadFolderPath);

using HttpClient httpClient = new HttpClient();

await downloadPage(rootURL, downloadFolderPath, httpClient, downloadedURLs);

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
    }

    //Recursion



}