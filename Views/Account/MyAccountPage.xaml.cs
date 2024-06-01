using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.ViewModels;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.Helpers;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using SGSClient.Core.Database;
using System.Data.SqlClient;
using SGSClient.Core.Authorization;

namespace SGSClient.Views;
public sealed partial class MyAccountPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string _avatarUrl;
    private string _welcomeText;

    public string AvatarUrl
    {
        get
        {
            return _avatarUrl;
        }
        set
        {
            if (_avatarUrl != value)
            {
                _avatarUrl = value;
                OnPropertyChanged(nameof(AvatarUrl));
            }
        }
    }

    public string WelcomeText
    {
        get
        {
            return _welcomeText;
        }
        set
        {
            if (_welcomeText != value)
            {
                _welcomeText = value;
                OnPropertyChanged(nameof(WelcomeText));
            }
        }
    }
    public HomeViewModel ViewModel
    {
        get;
    }

    public MyAccountPage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        string email = "test@test.com";
        string username = "test";
        using (SqlConnection connection = new SqlConnection(Db.GetConnectionString()))
        {
            try
            {
                connection.Open();

                // Zapytanie SQL do aktualizacji hasła dla użytkownika o danym adresie e-mail
                string query = @"
select
  r.Email
, d.Name
from Registration r
inner join sgsDevelopers d on d.Id = r.DeveloperId
where r.Id = @userId";

                // Utwórz nowy obiekt SqlCommand z zapytaniem SQL i połączeniem
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Dodaj parametry do zapytania SQL
                    command.Parameters.AddWithValue("@userId", AppSession.CurrentUserSession.UserId);

                    // Wykonaj zapytanie SQL i odczytaj wynik
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Sprawdź, czy istnieją wiersze wynikowe
                        if (reader.Read())
                        {
                            // Pobierz adres e-mail z kolumny "Email" i zapisz do zmiennej email
                            email = reader["Email"].ToString();
                            username = reader["Name"].ToString();
                            // Tutaj możesz wykorzystać zmienną email do dalszej obróbki
                            AvatarUrl = GravatarHelper.GetAvatarUrl(email); // Przykładowe wykorzystanie adresu e-mail
                            WelcomeText = "Witaj, " + username + "!";
                            OnPropertyChanged(nameof(AvatarUrl)); // Poinformuj XAML, że zmieniła się wartość AvatarUrl
                            OnPropertyChanged(nameof(WelcomeText)); // Poinformuj XAML, że zmieniła się wartość AvatarUrl
                        }
                        else
                        {
                            Console.WriteLine($"Nie znaleziono użytkownika o Id: {AppSession.CurrentUserSession.UserId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas aktualizacji hasła w bazie danych: {ex.Message}");
            }
        }

        AvatarUrl = GravatarHelper.GetAvatarUrl(email); // Pobierz URL Gravatara na podstawie e-maila
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Scroller_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
    {
        if (e.FinalView.HorizontalOffset < 1)
        {
            ScrollBackBtn.Visibility = Visibility.Collapsed;
        }
        else if (e.FinalView.HorizontalOffset > 1)
        {
            ScrollBackBtn.Visibility = Visibility.Visible;
        }

        if (e.FinalView.HorizontalOffset > scroller.ScrollableWidth - 1)
        {
            ScrollForwardBtn.Visibility = Visibility.Collapsed;
        }
        else if (e.FinalView.HorizontalOffset < scroller.ScrollableWidth - 1)
        {
            ScrollForwardBtn.Visibility = Visibility.Visible;
        }
    }
    private void ScrollBackBtn_Click(object sender, RoutedEventArgs e)
    {
        scroller.ChangeView(scroller.HorizontalOffset - scroller.ViewportWidth, null, null);
    }
    private void ScrollForwardBtn_Click(object sender, RoutedEventArgs e)
    {
        scroller.ChangeView(scroller.HorizontalOffset + scroller.ViewportWidth, null, null);
    }
    private void Scroller_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateScrollButtonsVisibility();
    }
    private void UpdateScrollButtonsVisibility()
    {
        if (scroller.ScrollableWidth > 0)
        {
            ScrollForwardBtn.Visibility = Visibility.Visible;
        }
        else
        {
            ScrollForwardBtn.Visibility = Visibility.Collapsed;
        }
    }

    private void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(UploadGamePage), null, new DrillInNavigationTransitionInfo());
    }
    private void MyGamesButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(MyGamesPage), null, new DrillInNavigationTransitionInfo());
    }
}
