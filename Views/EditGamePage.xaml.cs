using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.Helpers;
using SGSClient.ViewModels;
using System.Net.Http.Headers;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace SGSClient.Views
{
    public sealed partial class EditGamePage : Page
    {
        private int gameId;
        public string GameId { get; private set; }
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
            if (e.Parameter is int parsedGameId)
            {
                gameId = parsedGameId;
                GameId = parsedGameId.ToString();

                await ViewModel.LoadGameTypes();
                await ViewModel.LoadGameEngines();
                await ViewModel.LoadGameData(gameId);
                gameDescriptionRichEditBox.SetHtml(ViewModel.GameDescription);
                hardwareRequirementsRichEditBox.SetHtml(ViewModel.HardwareRequirements);
            }
        }
        private void GameDescriptionRichEditBox_Loaded(object sender, RoutedEventArgs e)
        {
            gameDescriptionRichEditBox.SetHtml(ViewModel.GameDescription ?? string.Empty);
        }

        private void HardwareRequirementsRichEditBox_Loaded(object sender, RoutedEventArgs e)
        {
            hardwareRequirementsRichEditBox.SetHtml(ViewModel.HardwareRequirements ?? string.Empty);
        }

        private void GameDescriptionRichEditBox_TextChanged(object sender, RoutedEventArgs e)
        {
            gameDescriptionRichEditBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string plain);
            plain = plain?.Trim() ?? string.Empty;

            if (plain.Length == 0)
                ViewModel.GameDescription = string.Empty;
            else
                ViewModel.GameDescription = gameDescriptionRichEditBox.GetHtml();
        }

        private void HardwareRequirementsRichEditBox_TextChanged(object sender, RoutedEventArgs e)
        {
            hardwareRequirementsRichEditBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string plain);
            plain = plain?.Trim() ?? string.Empty;

            if (plain.Length == 0)
                ViewModel.HardwareRequirements = string.Empty;
            else
                ViewModel.HardwareRequirements = hardwareRequirementsRichEditBox.GetHtml();
        }




        //private void Menu_Opening(object sender, object e)
        //{
        //    CommandBarFlyout myFlyout = sender as CommandBarFlyout;
        //    if (myFlyout.Target == gameDescriptionRichEditBox)
        //    {
        //        AppBarButton myButton = new AppBarButton();
        //        myButton.Command = new StandardUICommand(StandardUICommandKind.Share);
        //        myFlyout.PrimaryCommands.Add(myButton);
        //    }
        //}

        //private void gameDescriptionRichEditBox_Loaded(object sender, RoutedEventArgs e)
        //{
        //    gameDescriptionRichEditBox.SelectionFlyout.Opening += Menu_Opening;
        //    gameDescriptionRichEditBox.ContextFlyout.Opening += Menu_Opening;
        //}



        #region Logo
        private async void AddLogoButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            // Inicjalizuj picker z uchwytem okna
            Helpers.WindowHelper.InitializeWithWindow(picker, App.MainWindow);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Load the selected image
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var bitmapImage = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);

                    var newGameImage = new GameImage(bitmapImage)
                    {
                        Url = file.Path
                    };

                    ViewModel.GameLogos.Add(newGameImage);
                    AddLogoBtn.IsEnabled = false;
                    //await OpenImagePreviewDialog(newGameImage);
                }
            }
        }
        private void RemoveLogoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is GameImage gameImage)
                ViewModel.GameLogos.Remove(gameImage);

            AddLogoBtn.IsEnabled = true;
        }
        #endregion

        #region Images gallery
        private async void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            // Inicjalizuj picker z uchwytem okna
            Helpers.WindowHelper.InitializeWithWindow(picker, App.MainWindow);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Load the selected image
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var bitmapImage = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);

                    // Add the selected image to the GameImages collection
                    var newGameImage = new GameImage(bitmapImage)
                    {
                        Url = file.Path // Store the image path (URL)
                    };

                    ViewModel.GameImages.Add(newGameImage);

                    // Open the preview dialog to show the image immediately
                    await OpenImagePreviewDialog(newGameImage);
                }
            }
            else
            {
                // If no file is chosen, add a placeholder image
                //var placeholderImage = new GameImage("ms-appx:///Assets/placeholder.png");
                //ViewModel.GameImages.Add(placeholderImage);
            }
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
        #endregion

        #region Buttons
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

        private async Task OpenImagePreviewDialog(GameImage gameImage)
        {
            var previewDialog = new ContentDialog
            {
                Title = "Podgląd zdjęcia",
                CloseButtonText = "Zamknij",
                PrimaryButtonText = "Zmień zdjęcie",
                XamlRoot = this.XamlRoot,
                Width = 800,
                Height = 450
            };

            // StackPanel for dialog layout
            StackPanel dialogStackPanel = new StackPanel();

            // Image for displaying the preview
            Image imagePreview = new Image
            {
                Width = 800,
                Height = 450,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5)
            };

            // Load initial image source asynchronously
            if (!string.IsNullOrEmpty(gameImage.Url))
            {
                try
                {
                    string username = "sgsclient";
                    string password = "yGnxE-Tykxe-SwjwW-NooLc-xSwPT";

                    var uri = new Uri(gameImage.Url, UriKind.RelativeOrAbsolute);
                    if (uri.Scheme == "http" || uri.Scheme == "https")
                    {
                        using (var client = new HttpClient())
                        {
                            var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

                            var response = await client.GetAsync(uri);
                            if (response.IsSuccessStatusCode)
                            {
                                var imageStream = await response.Content.ReadAsStreamAsync();

                                Microsoft.UI.Xaml.Media.Imaging.BitmapImage bitmapImage = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                                await bitmapImage.SetSourceAsync(imageStream.AsRandomAccessStream());

                                imagePreview.Source = bitmapImage;
                            }
                            else
                            {
                                var bitmapImage = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                                await bitmapImage.SetSourceAsync(await RandomAccessStreamReference.CreateFromUri(uri).OpenReadAsync());
                                imagePreview.Source = bitmapImage;
                            }
                        }
                    }

                    else if (uri.IsFile) // If it's a local file path, use StorageFile
                    {
                        var file = await StorageFile.GetFileFromPathAsync(gameImage.Url); // Use the local path
                        using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                        {
                            var bitmapImage = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                            await bitmapImage.SetSourceAsync(fileStream);
                            imagePreview.Source = bitmapImage;
                        }
                    }
                }
                catch
                {
                    imagePreview.Source = null; // Handle invalid URIs
                }
            }

            dialogStackPanel.Children.Add(imagePreview);

            previewDialog.Content = dialogStackPanel;

            // Define the action for the primary button click (Change Image)
            previewDialog.PrimaryButtonClick += async (s, e) =>
            {
                // Open file picker
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".bmp");

                // Ensure picker works in desktop apps
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    // Update image preview with the selected file
                    var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    var bitmapImage = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);

                    imagePreview.Source = bitmapImage;
                    gameImage.Url = file.Path; // Update the GameImage URL to reflect the local path
                }
            };

            // Show the dialog
            await previewDialog.ShowAsync();
        }
    }
}