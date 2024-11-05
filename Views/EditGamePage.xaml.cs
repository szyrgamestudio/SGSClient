using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.ViewModels;

namespace SGSClient.Views
{
    public sealed partial class EditGamePage : Page
    {
        private int gameId;
        public string GameId { get; private set; }
        public class GameTypeItem
        {
            public int Id
            {
                get; set;
            }
            public KeyValuePair<int, string> Pair
            {
                get; set;
            }

            public GameTypeItem(int id, KeyValuePair<int, string> pair)
            {
                Id = id;
                Pair = pair;
            }

            public override string ToString()
            {
                return Pair.Value;
            }
        }
        public class GameEngineItem
        {
            public int Id
            {
                get; set;
            }
            public KeyValuePair<int, string> Pair
            {
                get; set;
            }

            public GameEngineItem(int id, KeyValuePair<int, string> pair)
            {
                Id = id;
                Pair = pair;
            }

            public override string ToString()
            {
                return Pair.Value;
            }
        }
        public EditGameViewModel ViewModel { get; }
        public EditGamePage()
        {
            ViewModel = App.GetService<EditGameViewModel>();
            InitializeComponent();
            DataContext = ViewModel;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string gameIdParam && int.TryParse(gameIdParam, out int parsedGameId))
            {
                gameId = parsedGameId;
                GameId = gameIdParam;

                await ViewModel.LoadGameTypes();
                await ViewModel.LoadGameEngines();
                await ViewModel.LoadGameData(gameId);
            }
        }

        #region Buttons
        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            var newGameImage = new GameImage("ms-appx:///Assets/placeholder.png");
            ViewModel.GameImages.Add(newGameImage);
        }
        private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is GameImage gameImage)
            {
                ViewModel.GameImages.Remove(gameImage);
            }
        }
        private void PreviewImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is GameImage gameImage)
            {
                _ = OpenImagePreviewDialog(gameImage);
            }
        }
        private void gotoSGSClientWWW_Click(object sender, RoutedEventArgs e)
        {
            var URL = "https://sgsclient.m455yn.dev/upload";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = URL,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wystąpił błąd podczas otwierania linku do logo gry: " + ex.Message);
            }
        }
        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.SaveGameData(gameId);
            Frame.GoBack(new DrillInNavigationTransitionInfo());
        }
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MyGamesPage), new DrillInNavigationTransitionInfo());
        }
        #endregion

        #region Tasks
        private async Task OpenImagePreviewDialog(GameImage gameImage)
        {
            var previewDialog = new ContentDialog
            {
                Title = "Podgląd zdjęcia",
                CloseButtonText = "Zamknij",
                PrimaryButtonText = "Zatwierdź",
                XamlRoot = this.XamlRoot
            };

            // StackPanel for dialog layout
            StackPanel dialogStackPanel = new StackPanel();

            // TextBox for entering image URL
            TextBox urlTextBox = new TextBox
            {
                Margin = new Thickness(5),
                PlaceholderText = "Wstaw link do zdjęcia",
                Text = gameImage.Url
            };
            dialogStackPanel.Children.Add(urlTextBox);

            // Image for displaying the preview
            Image imagePreview = new Image
            {
                Width = 200,
                Height = 150,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5),
                Source = new BitmapImage(new Uri(gameImage.Url)) // Preview initial URL
            };
            dialogStackPanel.Children.Add(imagePreview);

            // Update image preview as the URL changes
            urlTextBox.TextChanged += (s, e) =>
            {
                try
                {
                    imagePreview.Source = new BitmapImage(new Uri(urlTextBox.Text));
                }
                catch
                {
                    // Handle invalid URI
                    imagePreview.Source = null;
                }
            };

            previewDialog.Content = dialogStackPanel;

            // Show the dialog and confirm changes
            var result = await previewDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Update the game image URL
                gameImage.Url = urlTextBox.Text;
            }
        }
        #endregion
    }
}