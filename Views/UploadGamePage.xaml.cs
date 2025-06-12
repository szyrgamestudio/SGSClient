using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.Core.Authorization;
using SGSClient.ViewModels;
using System.Net.Http.Headers;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace SGSClient.Views;

public sealed partial class UploadGamePage : Page
{
    public UploadGameViewModel ViewModel
    {
        get;
    }
    public ShellViewModel ShellViewModel
    {
        get;
    }
    public UploadGamePage(IAppUser appUser)
    {
        ViewModel = App.GetService<UploadGameViewModel>();
        ShellViewModel = App.GetService<ShellViewModel>();
        InitializeComponent();

        DataContext = ViewModel;
    }
    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        await ViewModel.LoadGameTypes();
        await ViewModel.LoadGameEngines();
    }

    #region Buttons
    private void RemoveLogoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is GameImage gameImage)
            ViewModel.GameLogos.Remove(gameImage);

        AddLogoBtn.IsEnabled = true;
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

    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(MyGamesPage), new DrillInNavigationTransitionInfo());
    }
    private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        string userId = ShellViewModel.GetUserDisplayNameAsync();
        bool success = await ViewModel.AddGameData();

        if (success)
            Frame.Navigate(typeof(MyGamesPage), new DrillInNavigationTransitionInfo());
        else
            ShowMessageDialog("Błąd", "Nie udało się dodać gry.");
    }
    #endregion

    #region Dialogs
    private async void ShowMessageDialog(string title, string content)
    {
        ContentDialog messageDialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = "OK",
            //CloseButtonText = "Anuluj",
            XamlRoot = this.Content.XamlRoot
        };

        ContentDialogResult result = await messageDialog.ShowAsync();
    }
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
    #endregion

    #region File pickers
    private async void PickZIPFile_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.Desktop;
        picker.FileTypeFilter.Add(".zip");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        StorageFile file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            var viewModel = (UploadGameViewModel)DataContext;
            viewModel.ZipFile = file;
            viewModel.ZipLink = file.Name;
        }
    }
    private async void PickLogoImage_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        StorageFile file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            //gameLogoTextBox.Text = file.Path;
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
            }

        }
    }
    private async void PickScreenshotImage_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        StorageFile file = await picker.PickSingleFileAsync();
        if (file != null)
        {
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
    }
    #endregion
}