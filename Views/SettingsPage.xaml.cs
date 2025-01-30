using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.ViewModels;
using Windows.System;

namespace SGSClient.Views;

public sealed partial class SettingsPage : Page
{
    #region Properties
    public string Version
    {
        get
        {
            var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
            return string.Format("{0}.{1}.{2}.{3}", version?.Major, version?.Minor, version?.Build, version?.Revision);
        }
    }
    public SettingsViewModel ViewModel { get; }
    #endregion

    #region Constructor
    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();

        ViewModel.LoadSession();
        accountSettings.Visibility = ViewModel.IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;
    }
    #endregion

    #region Event Handlers
    private async void bugRequestCard_Click(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://github.com/szyrgamestudio/SGSClient/issues/new/choose"));
    }
    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Logout();
        Frame.Navigate(typeof(LoginPage));
    }
    #endregion
}
