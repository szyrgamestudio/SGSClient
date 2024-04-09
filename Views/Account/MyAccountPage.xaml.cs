using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.Core.Authorization;
using SGSClient.ViewModels;

namespace SGSClient.Views;

public sealed partial class MyAccountPage : Page
{
    public MyAccountViewModel ViewModel
    {
        get;
    }

    public MyAccountPage()
    {
        ViewModel = App.GetService<MyAccountViewModel>();
        InitializeComponent();
    }
    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        // Wylogowanie użytkownika - zresetowanie stanu sesji
        AppSession.CurrentUserSession.IsLoggedIn = false;
        AppSession.CurrentUserSession.UserId = null;
        AppSession.CurrentUserSession.UserName = null;

        // Tutaj możesz dodać dodatkowe czynności po wylogowaniu, np. przejście do ekranu logowania
        // Jeśli masz oddzielną stronę logowania, możesz użyć następującego kodu:
        Frame.Navigate(typeof(LoginPage), new DrillInNavigationTransitionInfo());
    }
}
