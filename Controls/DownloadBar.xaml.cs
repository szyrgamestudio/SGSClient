using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.Models;
using SGSClient.ViewModels;

namespace SGSClient.Controls
{
    public sealed partial class DownloadBar : UserControl
    {
        public DownloadViewModel ViewModel => DownloadViewModel.Instance;

        public DownloadBar()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        private void CancelDownload_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DownloadItem item)
            {
                item.Cancel();
                DownloadViewModel.Instance.CancelDownload(item);
            }
        }

    }
}
