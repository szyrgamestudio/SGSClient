using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.ViewModels;
using Windows.System;

namespace SGSClient.Views;

public sealed partial class SettingsPage : Page
{
    // Property to display the version
    public string Version
    {
        get
        {
            var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
            return string.Format("{0}.{1}.{2}.{3}", version?.Major, version?.Minor, version?.Build, version?.Revision);
        }
    }

    // ViewModel for SettingsPage
    public SettingsViewModel ViewModel { get; }

    // Constructor to inject dependencies
    public SettingsPage()
    {
        // Set the injected IAppUser instance
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();

        // Load the session
        //var session = _appUser.LoadSession();
        //if (session != null && session.IsLoggedIn)
        //    accountSettings.Visibility = Visibility.Visible;
        //else
        //    accountSettings.Visibility = Visibility.Collapsed;
    }

    // Button click event for bug request
    private async void bugRequestCard_Click(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://github.com/szyrgamestudio/SGSClient/issues/new/choose"));
    }

    // Logout button click event
    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Logout();
        Frame.Navigate(typeof(LoginPage));
    }
}
