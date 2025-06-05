using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Helpers;
using SGSClient.Models;
using SGSClient.ViewModels;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Gaming.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace SGSClient.Views;

public sealed partial class UploadGamePage : Page
{
    private int selectedGameTypeId;
    private int selectedGameEngineId;
    private StorageFile? selectedZipFile;

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
            selectedZipFile = file;
            linkZIPTextBox.Text = file.Name;
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

    #region DDLs
    private void GetSelectedGameTypeKey()
    {
        if (comboBoxGameType.SelectedItem != null)
        {
            GameTypeItem selectedGameType = (GameTypeItem)comboBoxGameType.SelectedItem;
            selectedGameTypeId = selectedGameType.Id;
        }
    }
    private void GetSelectedGameEngineKey()
    {
        if (comboBoxGameEngine.SelectedItem != null)
        {
            GameEngineItem selectedGameEngine = (GameEngineItem)comboBoxGameEngine.SelectedItem;
            selectedGameEngineId = selectedGameEngine.Id;
        }
    }
    #endregion

    public UploadGameViewModel ViewModel
    {
        get;
    }
    public ShellViewModel ShellViewModel
    {
        get;
    }
    public UploadGamePage()
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
    private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        string userId = ShellViewModel.GetUserDisplayNameAsync().ToString();
        await ViewModel.AddGameData(userId);
        Frame.GoBack(new DrillInNavigationTransitionInfo());

        #region Do przeniesienia do ViewModel 
        if (string.IsNullOrEmpty(gameNameTextBox.Text) ||
            string.IsNullOrEmpty(versionTextBox.Text) ||
            string.IsNullOrEmpty(linkZIPTextBox.Text) ||
            string.IsNullOrEmpty(gameEXETextBox.Text) ||
            string.IsNullOrEmpty(gameDescriptionTextBox.Text))
        {
            ShowMessageDialog("Błąd", "Nie uzupełniono wszystkich wymaganych pól.");
            return;
        }

        string pattern = @"^\d+\.\d+\.\d+$";
        bool isMatch = Regex.IsMatch(versionTextBox.Text, pattern);
        if (!isMatch)
        {
            ShowMessageDialog("Błąd", "Wprowadzono wersję w niepoprawnym formacie.\nUpewnij się, że format wersji wygląda następująco: x.x.x");
            return;
        }

        GetSelectedGameTypeKey();
        GetSelectedGameEngineKey();

        // Pobranie wartości z formularza
        string gameName = gameNameTextBox.Text;      //Nazwa gry
        string symbol = symbolTextBox.Text;          //Symbol gry
        string currentVersion = versionTextBox.Text; //Aktualna wersja gry
        string exeName = gameEXETextBox.Text;        //Plik wykonawczy gry

        //Opis gry
        string gameDescription = gameDescriptionTextBox.Text;
        string[] gameDescriptionLines = gameDescription.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        string gameDescriptionParam = string.Join(Environment.NewLine, gameDescriptionLines);

        //Wymagania sprzętowe
        string hardwareRequirements = hardwareRequirementsTextBox.Text;
        string[] hardwareRequirementsLines = hardwareRequirements.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        string hardwareRequirementsParam = string.Join(Environment.NewLine, hardwareRequirementsLines);

        //Pozostałe informacje - pole tekstowe
        string otherInformations = otherInfoTextBox.Text;
        string[] otherInformationsLines = otherInformations.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
        string otherInformationsParam = string.Join(Environment.NewLine, otherInformationsLines);

        int? gameEngine = selectedGameEngineId == 0 ? null : selectedGameEngineId;
        int? gameType = selectedGameTypeId == 0 ? null : selectedGameTypeId;

        #region Upload to Nextcloud
        var uploader = new NextcloudUploader("https://cloud.m455yn.dev/", "sgsclient", "yGnxE-Tykxe-SwjwW-NooLc-xSwPT");
        string nextcloudGameFolder = gameName;

        List<string> uploadedImageUrls = new();

        if (selectedZipFile != null)
        {
            string zipName = Guid.NewGuid() + ".zip";
            linkZIPTextBox.Text = await uploader.UploadFileAsync(selectedZipFile.Path, nextcloudGameFolder, zipName);
        }
        else if (!string.IsNullOrWhiteSpace(linkZIPTextBox.Text) && !Path.IsPathRooted(linkZIPTextBox.Text))
        {
            linkZIPTextBox.Text = linkZIPTextBox.Text;
        }
        else
        {
            Debug.WriteLine("Nie wybrano pliku ZIP.");
            return;
        }

        //string logoPath = gameLogoTextBox.Text;
        //if (!string.IsNullOrWhiteSpace(logoPath) && Path.IsPathRooted(logoPath))
        //{
        //    string ext = Path.GetExtension(logoPath);
        //    gameLogoTextBox.Text = await uploader.UploadFileAsync(logoPath, nextcloudGameFolder, $"logo{ext}", userId);
        //}
        //else
        //{
        //    gameLogoTextBox.Text = logoPath;
        //}

        int imageIndex = 1;
        foreach (var child in gameGalleryStackPanel.Children)
        {
            if (child is StackPanel stackPanel)
            {
                TextBox textBox = stackPanel.Children.OfType<TextBox>().FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(textBox?.Text))
                {
                    string imgPath = textBox.Text;
                    if (Path.IsPathRooted(imgPath))
                    {
                        string ext = Path.GetExtension(imgPath);
                        string uploadedUrl = await uploader.UploadFileAsync(imgPath, nextcloudGameFolder, $"zrzutEkranu_{imageIndex++}{ext}", userId);
                        if (uploadedUrl != null)
                            uploadedImageUrls.Add(uploadedUrl);
                    }
                    else
                    {
                        uploadedImageUrls.Add(imgPath); // gotowy URL
                    }
                }
            }
        }

        #endregion

        try
        {
            SqlCommand cmd = new(@"
declare @developerId int = (select r.DeveloperId from Registration r where r.UserId = @userId)

insert sgsGames (UserId, Title, PayloadName, ExeName, ZipLink, VersionLink, CurrentVersion, Description, HardwareRequirements, OtherInformation, Symbol, EngineId, TypeId, DraftP)
select 
  @userId
, @gameName
, null
, @ExeName
, @ZipLink
, @currentVersion
, @currentVersion
, @gameDescriptionParam
, @hardwareRequirementsParam
, @otherInformationsParam
, @Symbol
, @GameEngine
, @GameType
, 1

select
  SCOPE_IDENTITY()
", db.con);

            cmd.Parameters.AddWithValue("gameName", gameName.ToSqlParameter());
            cmd.Parameters.AddWithValue("userId", userId.ToSqlParameter());
            cmd.Parameters.AddWithValue("exeName", exeName.ToSqlParameter());
            cmd.Parameters.AddWithValue("zipLink", linkZIPTextBox.Text.ToSqlParameter());
            cmd.Parameters.AddWithValue("currentVersion", currentVersion.ToSqlParameter());
            cmd.Parameters.AddWithValue("gameDescriptionParam", gameDescriptionParam.ToSqlParameter());
            cmd.Parameters.AddWithValue("hardwareRequirementsParam", hardwareRequirementsParam.ToSqlParameter());
            cmd.Parameters.AddWithValue("otherInformationsParam", otherInformationsParam.ToSqlParameter());
            cmd.Parameters.AddWithValue("symbol", symbol.ToSqlParameter());
            cmd.Parameters.AddWithValue("gameEngine", gameEngine.ToSqlParameter());
            cmd.Parameters.AddWithValue("gameType", gameType.ToSqlParameter());

            int gameId = Convert.ToInt32(db.scalarSQL(cmd));

            //-----------------------------------
            // Zrzuty ekranu
            if (uploadedImageUrls.Count > 0)
            {
                foreach (var url in uploadedImageUrls)
                {
                    db.con.exec(@"
insert sgsGameImages (GameId, ImagePath)
select
  @p0
, @p1
", gameId.ToSqlParameter(), url.ToSqlParameter());
                }
            }
            // Logo
            //            if (!string.IsNullOrWhiteSpace(gameLogoTextBox.Text))
            //            {
            //                db.con.exec(@"
            //insert sgsGameLogo (GameId, LogoPath)
            //select
            //  @p0
            //, @p1
            //", gameId.ToSqlParameter(), gameLogoTextBox.Text.ToSqlParameter());

            //            }

            if (gameId > 0)
                Frame.Navigate(typeof(MyGamesPage), new DrillInNavigationTransitionInfo());
            else
                System.Windows.MessageBox.Show("Błąd podczas dodawania gry do bazy danych.");
            #endregion
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show("Błąd: " + ex.Message);
        }

    }

}