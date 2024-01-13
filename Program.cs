using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using tretton37uppgift;

string rootURL = @"http://books.toscrape.com/index.html";
string downloadFolderPath = @"C:\temp\bookstoscrapecom\";

Console.WriteLine($"Downloading {rootURL} to {downloadFolderPath}");

var downloader = new WebSiteDownloader(rootURL, downloadFolderPath);

await downloader.StartDownload();

Console.WriteLine();
Console.WriteLine($"Download a total of {downloader.Count} pages.");
Console.WriteLine("Press Enter to exit.");
Console.ReadLine();