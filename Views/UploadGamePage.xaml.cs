using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Helpers;
using SGSClient.ViewModels;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SGSClient.Views;

public sealed partial class UploadGamePage : Page
{
    private int selectedGameTypeId;
    private int selectedGameEngineId;
    private StorageFile? selectedZipFile;
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
            return Pair.Value; // Zwraca nazwę jako wartość do wyświetlenia w ComboBox
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
            return Pair.Value; // Zwraca nazwę jako wartość do wyświetlenia w ComboBox
        }
    }

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
            gameLogoTextBox.Text = file.Path;
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
            ImageTextBox1.Text = file.Path;
        }
    }
    #endregion

    #region DDLs
    private void LoadGameTypes()
    {
        List<GameTypeItem> gameTypeList = [];

        try
        {
            DataSet ds = db.con.select(@"
select
  gt.Id
, gt.Name
from sgsGameTypes gt
");

            if (ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int typeId = dr.TryGetValue("Id").ToInt32();
                    string typeName = dr.TryGetValue("Name").ToString();
                    KeyValuePair<int, string> pair = new(typeId, typeName);
                    gameTypeList.Add(new GameTypeItem(typeId, pair));
                }

                comboBoxGameType.Items.Clear();
                foreach (var item in gameTypeList)
                {
                    comboBoxGameType.Items.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
    }
    private void LoadGameEngines()
    {
        List<GameEngineItem> enginesList = [];

        try
        {
            DataSet ds = db.con.select(@"
select
  ge.Id
, ge.Name
from sgsGameEngines ge");

            if (ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int engineId = dr.TryGetValue("Id").ToInt32();
                    string engineName = dr.TryGetValue("Name").ToString();

                    KeyValuePair<int, string> pair = new(engineId, engineName);
                    enginesList.Add(new GameEngineItem(engineId, pair));
                }

                comboBoxGameEngine.Items.Clear();
                foreach (var item in enginesList)
                {
                    comboBoxGameEngine.Items.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
    }
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
        LoadGameTypes();
        LoadGameEngines();
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

        string logoPath = gameLogoTextBox.Text;
        if (!string.IsNullOrWhiteSpace(logoPath) && Path.IsPathRooted(logoPath))
        {
            string ext = Path.GetExtension(logoPath);
            gameLogoTextBox.Text = await uploader.UploadFileAsync(logoPath, nextcloudGameFolder, $"logo{ext}", userId);
        }
        else
        {
            gameLogoTextBox.Text = logoPath;
        }

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

insert sgsGames (Title, DeveloperId, PayloadName, ExeName, ZipLink, VersionLink, CurrentVersion, Description, HardwareRequirements, OtherInformation, Symbol, EngineId, TypeId, DraftP)
select 
  @gameName
, @developerId
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
            if (!string.IsNullOrWhiteSpace(gameLogoTextBox.Text))
            {
                db.con.exec(@"
insert sgsGameLogo (GameId, LogoPath)
select
  @p0
, @p1
", gameId.ToSqlParameter(), gameLogoTextBox.Text.ToSqlParameter());

            }

            if (gameId > 0)
                Frame.Navigate(typeof(MyGamesPage), new DrillInNavigationTransitionInfo());
            else
                System.Windows.MessageBox.Show("Błąd podczas dodawania gry do bazy danych.");
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show("Błąd: " + ex.Message);
        }

    }

    private int additionalImageCount = 1;
    private async void AddImageButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".bmp");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        //WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        StorageFile file = await picker.PickSingleFileAsync();

        if (file == null)
            return;

        StackPanel imageTextBoxPanel = new StackPanel();
        imageTextBoxPanel.Orientation = Orientation.Horizontal;

        TextBox newImageTextBox = new TextBox();
        newImageTextBox.Name = "ImageTextBox" + (additionalImageCount + 1);
        newImageTextBox.Margin = new Thickness(5);
        newImageTextBox.Width = 400;
        newImageTextBox.TextWrapping = TextWrapping.Wrap;
        newImageTextBox.Text = file.Path;

        #region Przycisk "Usuń"
        Button removeButton = new Button();
        removeButton.Margin = new Thickness(5);
        removeButton.Click += RemoveImageButton_Click;

        var removeButtonFontIcon = new FontIcon();
        removeButtonFontIcon.Glyph = "\xE74D";
        removeButton.Content = removeButtonFontIcon;

        ToolTip removeToolTip = new ToolTip();
        removeToolTip.Content = "Usuń";
        ToolTipService.SetToolTip(removeButton, removeToolTip);
        #endregion

        imageTextBoxPanel.Children.Add(newImageTextBox);
        imageTextBoxPanel.Children.Add(removeButton);

        gameGalleryStackPanel.Children.Insert(gameGalleryStackPanel.Children.Count - 1, imageTextBoxPanel);

        additionalImageCount++;

        if (additionalImageCount >= 10)
        {
            AddImageButton.Visibility = Visibility.Collapsed;
        }
    }
    private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
    {
        Button removeButton = sender as Button;
        StackPanel parentPanel = removeButton.Parent as StackPanel;

        gameGalleryStackPanel.Children.Remove(parentPanel);
    }
    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(MyGamesPage), new DrillInNavigationTransitionInfo());
    }
}