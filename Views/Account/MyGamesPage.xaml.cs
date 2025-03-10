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
        #region Properties
        public ObservableCollection<Game> Games { get; set; }
        public MyGamesViewModel ViewModel { get; }
        #endregion

        #region Constructor
        public MyGamesPage()
        {
            ViewModel = App.GetService<MyGamesViewModel>();
            Games = new ObservableCollection<Game>();
            DataContext = ViewModel;
            InitializeComponent();
        }
        #endregion

        #region Navigation
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.LoadMyGamesFromDatabase();

            if (ViewModel.MyGamesList == null || !ViewModel.MyGamesList.Any())
                return;

            foreach (var game in ViewModel.MyGamesList)
                Games.Add(game);
        }
        #endregion

        #region Event Handlers
        private void Action_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button?.DataContext is Game game)
                Frame.Navigate(typeof(EditGamePage), game.GameId, new DrillInNavigationTransitionInfo());
        }
        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button?.DataContext is Game game)
                Frame.Navigate(typeof(GameBasePage), game.GameSymbol, new DrillInNavigationTransitionInfo());
        }
        #endregion
    }
}
