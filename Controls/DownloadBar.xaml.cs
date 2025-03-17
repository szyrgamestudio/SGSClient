using Microsoft.UI.Xaml.Controls;
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
    }
}
