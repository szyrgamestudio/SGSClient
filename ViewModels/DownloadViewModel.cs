using SGSClient.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SGSClient.ViewModels
{
    public class DownloadViewModel : INotifyPropertyChanged
    {
        public static DownloadViewModel Instance { get; } = new();

        private ObservableCollection<DownloadItem> _activeDownloads = new();
        public ObservableCollection<DownloadItem> ActiveDownloads
        {
            get => _activeDownloads;
            set
            {
                _activeDownloads = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void CancelDownload(DownloadItem item)
        {
            if (ActiveDownloads.Contains(item))
            {
                ActiveDownloads.Remove(item);
            }
        }

    }
}
