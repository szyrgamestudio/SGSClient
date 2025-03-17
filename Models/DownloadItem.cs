using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SGSClient.Models
{
    public class DownloadItem : INotifyPropertyChanged
    {
        public string GameName { get; }
        public string DownloadUrl { get; }
        public string DestinationPath { get; }
        public double Progress { get; private set; }
        public bool IsCompleted { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DownloadItem(string gameName, string downloadUrl, string destinationPath)
        {
            GameName = gameName;
            DownloadUrl = downloadUrl;
            DestinationPath = destinationPath;
        }

        public async Task StartDownloadAsync(HttpClient httpClient)
        {
            using var response = await httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalSize = response.Content.Headers.ContentLength ?? 1;
            var buffer = new byte[8192];

            using var fileStream = new FileStream(DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var contentStream = await response.Content.ReadAsStreamAsync();

            int bytesRead;
            long totalRead = 0;
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;
                Progress = (double)totalRead / totalSize * 100;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }

            IsCompleted = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
        }
    }
}