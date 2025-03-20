using SevenZipExtractor;
using SGSClient.Models;
using SGSClient.Views;
using System.Collections.ObjectModel;

namespace SGSClient.Services
{
    public class DownloadManager
    {
        private static readonly Lazy<DownloadManager> instance = new(() => new DownloadManager());
        public static DownloadManager Instance => instance.Value;

        public ObservableCollection<DownloadItem> ActiveDownloads { get; } = new();

        private readonly HttpClient httpClient = new();

        public event Action? DownloadsUpdated;

        private DownloadManager() { }

        public static async Task StartDownloadAsync(ShellPage shellPage, string gameName, string url, string destinationPath, string gameLogo)
        {
            shellPage?.AddDownload(gameName, url, destinationPath, gameLogo);
        }
    }
}