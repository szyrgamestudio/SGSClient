using SGSClient.Models;
using System.Collections.ObjectModel;

namespace SGSClient.ViewModels
{
    public class DownloadViewModel
    {
        public ObservableCollection<DownloadItem> ActiveDownloads { get; set; } = new();
    }
}
