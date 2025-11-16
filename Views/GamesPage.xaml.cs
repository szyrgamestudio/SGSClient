using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.ViewModels;

namespace SGSClient.Views
{
    public sealed partial class GamesPage : Page
    {
        #region Ctors and properties
        public GamesViewModel ViewModel { get; }
        public GamesPage()
        {
            ViewModel = App.GetService<GamesViewModel>();
            DataContext = ViewModel;
            InitializeComponent();
        }
        #endregion

        #region Private or protected methods
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.LoadGamesFromDatabase();
            _ = ViewModel.LoadFiltersAsync();
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

        #region Filters

        private void Input_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ApplyFilters();
            }
        }
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }
        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            SearchAuthorTextBox.Text = string.Empty;
            //CategoryComboBox.SelectedIndex = -1;
            GamesItemsControl.ItemsSource = ViewModel.GamesList;
        }
        private void ApplyFilters()
        {
            string searchTitleText = SearchTextBox.Text.ToLower();
            string searchAuthorText = SearchAuthorTextBox.Text.ToLower();
            int? selectedTypeId = ViewModel.SelectedGameType?.Id;
            int? selectedEngineId = ViewModel.SelectedGameEngine?.Id;

            var filteredGames = ViewModel.GamesList.Where(game =>
                (string.IsNullOrEmpty(searchTitleText) ||
                    (!string.IsNullOrEmpty(game.GameTitle) &&
                     game.GameTitle.Contains(searchTitleText, StringComparison.OrdinalIgnoreCase)))
                && (string.IsNullOrEmpty(searchAuthorText) ||
                    (!string.IsNullOrEmpty(game.GameDeveloper) &&
                     game.GameDeveloper.Contains(searchAuthorText, StringComparison.OrdinalIgnoreCase)))
                && (!selectedTypeId.HasValue || game.GameTypeId == selectedTypeId.Value)
                && (!selectedEngineId.HasValue || game.GameEngineId == selectedEngineId.Value)
            ).ToList();

            GamesItemsControl.ItemsSource = filteredGames;
        }
        #endregion

        #endregion
    }
}
