using Microsoft.UI.Xaml.Controls;

namespace SGSClient.Helpers
{
    public enum LauncherStatus
    {
        pageLauched,
        readyNoGame,
        ready,
        failed,
        downloadingGame,
        downloadingUpdate
    }

    public static class LauncherStatusHelper
    {
        public static void UpdateStatus(Button PlayButton, Button CheckUpdateButton, Button UninstallButton, ProgressBar DownloadProgressBorder, LauncherStatus status, string gameZip)
        {
            switch (status)
            {
                case LauncherStatus.pageLauched:
                    PlayButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    break;
                case LauncherStatus.readyNoGame:
                    PlayButton.Content = "Zainstaluj";
                    PlayButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    break;
                case LauncherStatus.ready:
                    PlayButton.Content = "Graj";
                    PlayButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    break;
                case LauncherStatus.failed:
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    break;
                case LauncherStatus.downloadingGame:
                    PlayButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    break;
                case LauncherStatus.downloadingUpdate:
                    PlayButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
    }
}
