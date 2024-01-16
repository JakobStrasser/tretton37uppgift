
namespace tretton37uppgift
{
    internal interface IWebSiteDownloader
    {
        int Count { get; }

        Task StartDownload(string UrlToDownload, string DownloadPath, int? MaxRetries = 5);
    }
}