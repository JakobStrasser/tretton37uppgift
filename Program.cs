using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using tretton37uppgift;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


var builder = CoconaApp.CreateBuilder();

builder.Logging.AddFilter("System.Net.Http", LogLevel.Warning);

builder.Services.AddSingleton<IWebSiteDownloader, WebSiteDownloader>();

var app = builder.Build();

app.AddCommand("download", async ([Option("u", Description = @"URL to download (default: http://books.toscrape.com/index.html)")] string? URL, [Option("o", Description = @"Output folder (default: current directory)")] string? downloadfolder, IWebSiteDownloader downloader) =>
{
    string rootURL = URL ?? @"http://books.toscrape.com/index.html";
    string downloadFolderPath = downloadfolder ?? @".\booksstoscrapecom\";

    Console.WriteLine($"Downloading {rootURL} to {downloadFolderPath}");
    try
    {
        await downloader.StartDownload(rootURL, downloadFolderPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Program failed: {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine($"Downloaded a total of {downloader.Count} pages.");
    Console.WriteLine("Press Enter to exit.");
    Console.ReadLine();
});

app.Run();
