using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.ViewModels;

namespace SGSClient.Views;

public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel { get; }

    public ProfilePage()
    {
        ViewModel = App.GetService<ProfileViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ViewModel.LoadRecentlyPlayed();
    }

    private void ButtonGame_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button clickedButton)
        {
            string? gameName = clickedButton.Tag?.ToString();
            if (!string.IsNullOrEmpty(gameName))
            {
                Frame.Navigate(typeof(GameBasePage), gameName, new DrillInNavigationTransitionInfo());
            }
        }
    }

}
