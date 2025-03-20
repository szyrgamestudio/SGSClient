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

            //using (var client = new HttpClient())
            //{
            //    var response = await client.GetAsync(url);
            //    if (response.IsSuccessStatusCode)
            //    {
            //        byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

            //        if (!Directory.Exists(destinationPath))
            //            Directory.CreateDirectory(destinationPath);

            //        string zipFilePath = Path.Combine(destinationPath, $"{gameName}.zip");

            //        await File.WriteAllBytesAsync(zipFilePath, fileBytes);

            //        string extractPath = Path.Combine(destinationPath, gameName);
            //        Directory.CreateDirectory(extractPath);

            //        using (var archiveFile = new ArchiveFile(zipFilePath))
            //        {
            //            archiveFile.Extract(extractPath);
            //        }

            //        File.Delete(zipFilePath);
            //        shellPage?.RemoveDownload(gameName);
            //    }
            //}
        }
    }
}