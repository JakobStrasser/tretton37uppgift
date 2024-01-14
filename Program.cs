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

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IWebSiteDownloader, WebSiteDownloader>();

var app = builder.Build();

app.AddCommand("download", async ([Option] string? URL, [Option] string? downloadfolder, IWebSiteDownloader downloader) =>
{
    string rootURL = URL ?? @"http://quotes.toscrape.com";
    string downloadFolderPath = downloadfolder ?? @"C:\temp\quotestoscrapecom\";

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
    Console.WriteLine($"Download a total of {downloader.Count} pages.");
    Console.WriteLine("Press Enter to exit.");
    Console.ReadLine();
});

app.Run();
