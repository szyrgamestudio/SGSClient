using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using SGSClient.ViewModels;

namespace SGSClient.Views;

public sealed partial class SettingsUserPage : Page
{
    public SettingsUserViewModel ViewModel
    {
        get;
    }

    public SettingsUserPage()
    {
        ViewModel = App.GetService<SettingsUserViewModel>();
        InitializeComponent();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        //Frame.Navigate(typeof(LoginPage));
    }
    private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ChangePasswordCommand();
        //Frame.Navigate(typeof(LoginPage));
    }

}
