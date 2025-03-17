using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SGSClient.Models
{
    public class DownloadItem : INotifyPropertyChanged
    {
        public string GameName { get; }
        public string DownloadUrl { get; }
        public string DestinationPath { get; }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressText)); // Aktualizuje tekst procentowy
            }
        }

        public string ProgressText => $"{Progress:F1}%";

        public bool IsCompleted { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
            }

            IsCompleted = true;
            OnPropertyChanged(nameof(IsCompleted));
        }
    }
}
