using SevenZipExtractor;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SGSClient.Models
{
    public class DownloadItem : INotifyPropertyChanged
    {
        private string _gameIcon;
        public string GameIcon
        {
            get => _gameIcon;
            set
            {
                _gameIcon = value;
                OnPropertyChanged();
            }
        }

        public string GameName { get; }
        public string DownloadUrl { get; }
        public string DestinationPath { get; }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    ProgressText = $"{_progress:F1}%";
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProgressText));
                }
            }
        }


        private CancellationTokenSource _cts = new();

        public void Cancel()
        {
            _cts.Cancel();
        }


        private string _progressText = "0.0%";
        public string ProgressText
        {
            get => _progressText;
            private set
            {
                if (_progressText != value)
                {
                    _progressText = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCompleted { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DownloadItem(string gameName, string downloadUrl, string destinationPath, string gameIcon = "ms-appx:///Assets/placeholder.png")
        {
            GameName = gameName;
            DownloadUrl = downloadUrl;
            DestinationPath = destinationPath;
            GameIcon = gameIcon;
        }

        public async Task StartDownloadAsync(HttpClient httpClient)
        {
            using var response = await httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalSize = response.Content.Headers.ContentLength ?? 1;
            var buffer = new byte[8192];

            // Obsługa błędu podczas otwierania pliku
            FileStream? fileStream = null;
            string filePath = Path.Combine(DestinationPath, $"{GameName}.zip");
            try
            {
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd przy otwieraniu pliku: {ex.Message}");
                return;
            }

            try
            {
                using var contentStream = await response.Content.ReadAsStreamAsync();
                int bytesRead;
                long totalRead = 0;
                var dispatcherQueue = App.MainWindow.DispatcherQueue;
                double lastReportedProgress = 0;
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cts.Token)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, _cts.Token);
                    totalRead += bytesRead;
                    double progressValue = (double)totalRead / totalSize * 100;

                    if (progressValue - lastReportedProgress >= 5 || progressValue == 100)
                    {
                        lastReportedProgress = progressValue;
                        dispatcherQueue.TryEnqueue(() =>
                        {
                            Progress = progressValue;
                        });
                    }
                }
                IsCompleted = true;
                dispatcherQueue.TryEnqueue(() => OnPropertyChanged(nameof(IsCompleted)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd podczas pobierania: {ex.Message}");
            }
            finally
            {
                fileStream?.Dispose(); // Zamknięcie pliku, jeśli został otwarty
            }
        }

    }
}