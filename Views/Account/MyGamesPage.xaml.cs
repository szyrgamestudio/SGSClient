using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.Models;
using SGSClient.ViewModels;

namespace SGSClient.Views
{
    public sealed partial class MyGamesPage : Page
    {
        public ObservableCollection<Game> Games { get; set; }
        public MyGamesViewModel ViewModel { get; }

        public MyGamesPage()
        {
            ViewModel = App.GetService<MyGamesViewModel>();
            Games = new ObservableCollection<Game>();
            DataContext = ViewModel;
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Ładowanie gier użytkownika z bazy danych
            ViewModel.LoadMyGamesFromDatabase();

            if (ViewModel.MyGamesList == null || !ViewModel.MyGamesList.Any())
                return;

            // Dodanie gier do lokalnej kolekcji
            foreach (var game in ViewModel.MyGamesList)
            {
                Games.Add(game);
            }
        }

        private void Action_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var game = button?.DataContext as Game;

            if (game != null)
                Frame.Navigate(typeof(EditGamePage), game.GameId, new DrillInNavigationTransitionInfo());
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var game = button?.DataContext as Game;

            if (game != null)
                Frame.Navigate(typeof(GameBasePage), game.GameSymbol, new DrillInNavigationTransitionInfo());
        }
    }
}
