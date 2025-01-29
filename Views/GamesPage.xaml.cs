using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.Controllers;
using SGSClient.Core.Database;
using SGSClient.ViewModels;

namespace SGSClient.Views
{
    public sealed partial class GamesPage : Page
    {
        private readonly ConfigurationManagerSQL configManagerSQL;
        private readonly DbContext _dbContext;

        private List<GamesViewModel> gamesList;
        private List<GamesViewModel> gamesFeaturedList;

        public GamesViewModel ViewModel { get; }
        public GamesPage()
        {
            ViewModel = App.GetService<GamesViewModel>();
            DataContext = ViewModel;
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadGamesFromDatabaseAsync();
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
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }
        private void SearchTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ApplyFilters();
            }
        }
        private void SearchAuthorTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ApplyFilters();
            }
        }
        private void CategoryComboBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ApplyFilters();
            }
        }
        private void ApplyFilters()
        {
            string searchTitleText = SearchTextBox.Text.ToLower();
            string searchAuthorText = SearchAuthorTextBox.Text.ToLower();
            string? selectedCategory = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            try
            {
                var filteredGames = ViewModel.GamesList.Where(game =>
    (string.IsNullOrEmpty(searchTitleText) || game.GameTitle.ToLower().Contains(searchTitleText))
    && (string.IsNullOrEmpty(searchAuthorText) || game.GameDeveloper.ToLower().Contains(searchAuthorText))
    /*&& (selectedCategory == "Wszystkie" || string.IsNullOrEmpty(selectedCategory) || game.GameType == selectedCategory)*/).ToList();

                GamesItemsControl.ItemsSource = filteredGames;

            }
            catch (Exception ex) { }
        }
        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            SearchAuthorTextBox.Text = string.Empty;
            CategoryComboBox.SelectedIndex = -1;
            GamesItemsControl.ItemsSource = gamesList;
        }
    }
}
