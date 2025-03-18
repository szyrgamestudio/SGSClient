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
        private bool IsDownloadsVisible = true; // Domyślnie pasek jest widoczny

        private void ToggleDownloads_Click(object sender, RoutedEventArgs e)
        {
            IsDownloadsVisible = !IsDownloadsVisible;
            DownloadsContent.Visibility = IsDownloadsVisible ? Visibility.Visible : Visibility.Collapsed;

            // Zmiana ikony przycisku
            MinimizeIcon.Glyph = IsDownloadsVisible ? "\uE921" : "\uE96D"; // Minus ↔ Strzałka w dół
        }

    }
}
