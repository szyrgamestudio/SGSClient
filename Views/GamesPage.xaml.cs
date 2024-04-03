using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.Controllers;
using SGSClient.ViewModels;
using System.Collections.Generic;
using System.Windows.Documents.DocumentStructures;

namespace SGSClient.Views
{
    public sealed partial class GamesPage : Page
    {
        private readonly ConfigurationManagerSQL configManagerSQL;

        public GamesViewModel ViewModel { get; }
        //private const string ConnectionString = @"Data Source=(localdb)\localDB1;Initial Catalog=SGS_SGSCLIENT;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        string ConnectionString = @"Data Source=145.239.80.7;Initial Catalog=SGS_CLIENT;User ID=sa;Password=ajjKcZtam63c#;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public GamesPage()
        {
            this.InitializeComponent();

            configManagerSQL = new ConfigurationManagerSQL(ConnectionString);
            LoadGamesFromDatabase();
        }

        private void LoadGamesFromDatabase()
        {
            List<GamesViewModel> gamesList = configManagerSQL.LoadGamesFromDatabase();
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
