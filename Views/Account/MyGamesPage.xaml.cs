using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await ViewModel.LoadMyGamesFromDatabaseAsync();

            if (ViewModel.GamesList == null)
                return;

            foreach (var gameViewModel in ViewModel.GamesList)
            {
                if (gameViewModel != null)
                {
                    Games.Add(new Game
                    {
                        GameId = gameViewModel.GameId,
                        Title = gameViewModel.GameTitle,
                        Genre = !string.IsNullOrEmpty(gameViewModel.GameType) ? gameViewModel.GameType : "-",
                        DraftP = Convert.ToBoolean(gameViewModel.DraftP) ? "Oczekuje na wydanie" : "Tak",
                        gameSymbol = gameViewModel.GameSymbol
                    });
                }
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
                Frame.Navigate(typeof(GameBasePage), game.gameSymbol, new DrillInNavigationTransitionInfo());
        }
    }

    public class Game
    {
        public string GameId
        {
            get; set;
        }
        public string Title
        {
            get; set;
        }
        public string Genre
        {
            get; set;
        }
        public string DraftP
        {
            get; set;
        }
        public string gameSymbol
        {
            get; set;
        }
        // Dodaj inne właściwości gry, jeśli są potrzebne
    }
}
