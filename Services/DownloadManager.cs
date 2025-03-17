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

        public async Task StartDownloadAsync(ShellPage shellPage, string gameName, string url, string destinationPath)
        {
            shellPage?.AddDownload(gameName, url, destinationPath);

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(destinationPath, fileBytes);
                }
            }
        }
    }
}