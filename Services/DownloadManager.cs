using SGSClient.Models;
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

        public async Task StartDownloadAsync(string gameName, string downloadUrl, string destinationPath)
        {
            var downloadItem = new DownloadItem(gameName, downloadUrl, destinationPath);
            ActiveDownloads.Add(downloadItem);
            DownloadsUpdated?.Invoke();

            await downloadItem.StartDownloadAsync(httpClient);

            DownloadsUpdated?.Invoke();
        }
    }
}