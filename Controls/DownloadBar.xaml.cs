using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.Models;
using SGSClient.ViewModels;

namespace SGSClient.Controls
{
    public sealed partial class DownloadBar : UserControl
    {
        #region Properties
        public DownloadViewModel ViewModel => DownloadViewModel.Instance;
        private bool IsDownloadsVisible = true;
        #endregion

        #region Ctor
        public DownloadBar()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
        #endregion

        #region Event Handlers
        private void CancelDownload_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DownloadItem item)
            {
                item.Cancel();
                DownloadViewModel.Instance.CancelDownload(item);
            }
        }
        private void ToggleDownloads_Click(object sender, RoutedEventArgs e)
        {
            IsDownloadsVisible = !IsDownloadsVisible;
            DownloadsContent.Visibility = IsDownloadsVisible ? Visibility.Visible : Visibility.Collapsed;
            MinimizeIcon.Glyph = IsDownloadsVisible ? "\uE921" : "\uE96D"; // Minus ↔ Down Arrow
        }
        #endregion
    }
}
