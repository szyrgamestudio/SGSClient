using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.Controllers;
using SGSClient.Core.Database;
using SGSClient.ViewModels;
using System.Collections.Generic;
using System.Windows.Documents.DocumentStructures;

namespace SGSClient.Views
{
    public sealed partial class GamesPage : Page
    {
        private readonly ConfigurationManagerSQL configManagerSQL;

        public GamesViewModel ViewModel { get; }
        string ConnectionString = db.con;

        public GamesPage()
        {
            this.InitializeComponent();

            configManagerSQL = new ConfigurationManagerSQL(ConnectionString);
            LoadGamesFromDatabase();
        }

        private void LoadGamesFromDatabase()
        {
            List<GamesViewModel> gamesList = configManagerSQL.LoadGamesFromDatabase(false);
            GamesItemsControl.ItemsSource = gamesList;
        }

        private void ButtonGame_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                string gameName = clickedButton.Tag?.ToString();
                if (!string.IsNullOrEmpty(gameName))
                {
                    Frame.Navigate(typeof(GameBasePage), gameName, new DrillInNavigationTransitionInfo());
                }
            }
        }

        private void ButtonAtorth_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GameBasePage), "AtorthTalesOfUlkimond", new DrillInNavigationTransitionInfo());
        }

        private void ButtonDoddani_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GameBasePage), "Doddani", new DrillInNavigationTransitionInfo());
        }
    }
}
