using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.Controllers;
using SGSClient.Core.Database;
using SGSClient.ViewModels;
using System;
using System.Collections.ObjectModel;
using Windows.Gaming.Input;

namespace SGSClient.Views
{
    public sealed partial class MyGamesPage : Page
    {
        public ObservableCollection<Game> Games
        {
            get; set;
        }
        private readonly ConfigurationManagerSQL configManagerSQL;
        string ConnectionString = db.con;

        public MyGamesPage()
        {
            this.InitializeComponent();
            Games = new ObservableCollection<Game>();
            configManagerSQL = new ConfigurationManagerSQL(ConnectionString);
            LoadGamesFromDatabase();
        }

        private void LoadGamesFromDatabase()
        {
            List<GamesViewModel> gamesList = configManagerSQL.LoadMyGamesFromDatabase();

            foreach (var gameViewModel in gamesList)
            {
                Games.Add(new Game { GameId = gameViewModel.GameId, Title = gameViewModel.GameTitle, Genre = !string.IsNullOrEmpty(gameViewModel.GameType) ? gameViewModel.GameType : "-", DraftP = gameViewModel.GameType == "0" ? "Oczekuje na wydanie" : "Tak" });
            }
        }

        private void Action_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var game = button?.DataContext as Game;

            if (game != null)
                Frame.Navigate(typeof(EditGamePage), game.GameId, new DrillInNavigationTransitionInfo());
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
        // Dodaj inne właściwości gry, jeśli są potrzebne
    }
}
