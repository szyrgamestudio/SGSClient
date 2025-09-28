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
        public static void UpdateStatus(Button playButton, Button checkUpdateButton, Button uninstallButton, LauncherStatus status, string gameZip)
        {
            playButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            checkUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            uninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

            switch (status)
            {
                case LauncherStatus.readyNoGame:
                    playButton.Content = "Zainstaluj";
                    playButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    break;

                case LauncherStatus.ready:
                    playButton.Content = "Graj";
                    playButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    uninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    break;

                case LauncherStatus.failed:
                    uninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    break;
            }
        }
    }
}
