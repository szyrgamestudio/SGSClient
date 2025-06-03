using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.Core.Authorization;
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

public sealed partial class UploadGamePage : Microsoft.UI.Xaml.Controls.Page
{
    private int selectedGameTypeId; //zmienna przechowująca wybrany gatunek gry
    private int selectedGameEngineId; //zmienna przechowująca wybrany silnik gry
    private readonly IAppUser _appUser;  // Declare _appUser
    private StorageFile selectedZipFile;
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


    /// <summary>
    /// Wczytaj wszystkie gatunki gier i zapisz je do comboboxa
    /// </summary>
    private void LoadGameTypes()
    {
        try
        {
            DataSet ds = db.con.select("select sgsgt.Id, sgsgt.Name from sgsGameTypes sgsgt");

            List<GameTypeItem> gameTypeList = new List<GameTypeItem>();

            // Sprawdź, czy są jakiekolwiek tabele w DataSet
            if (ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int typeId = Convert.ToInt32(row["Id"]);
                    string typeName = row["Name"].ToString();
                    var pair = new KeyValuePair<int, string>(typeId, typeName);
                    gameTypeList.Add(new GameTypeItem(typeId, pair));
                }

                // Wyczyść istniejące elementy ComboBox przed dodaniem nowych
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

    /// <summary>
    /// Wczytaj wszystkie silniki gier i zapisz je do comboboxa
    /// </summary>
    private void LoadGameEngines()
    {
        string query = "SELECT sgseg.Id, sgseg.Name FROM sgsGameEngines sgseg";

        try
        {
            DataSet ds = db.con.select("SELECT sgseg.Id, sgseg.Name FROM sgsGameEngines sgseg");
            List<GameEngineItem> enginesList = new List<GameEngineItem>();

            // Sprawdź, czy są jakiekolwiek tabele w DataSet
            if (ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int engineId = Convert.ToInt32(row["Id"]);
                    string engineName = row["Name"].ToString();
                    var pair = new KeyValuePair<int, string>(engineId, engineName);
                    enginesList.Add(new GameEngineItem(engineId, pair));
                }

                // Wyczyść istniejące elementy ComboBox przed dodaniem nowych
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
            throw; // Możesz obsłużyć wyjątek lub logować go, jeśli to konieczne
        }
    }
    private void GetSelectedGameTypeKey()
    {
        if (comboBoxGameType.SelectedItem != null)
        {
            var selectedGameType = (GameTypeItem)comboBoxGameType.SelectedItem;
            selectedGameTypeId = selectedGameType.Id; // Przypisanie id do zmiennej na zewnątrz metody
            string gameType = selectedGameType.Pair.Value;
        }
    }
    private void GetSelectedGameEngineKey()
    {
        if (comboBoxGameEngine.SelectedItem != null)
        {
            var selectedGameEngine = (GameEngineItem)comboBoxGameEngine.SelectedItem;
            selectedGameEngineId = selectedGameEngine.Id; // Przypisanie id do zmiennej na zewnątrz metody
            string engineName = selectedGameEngine.Pair.Value;
        }
    }

    public UploadGameViewModel ViewModel
    {
        get;
    }
    public ShellViewModel shViewModel
    {
        get;
    }
    public UploadGamePage()
    {
        ViewModel = App.GetService<UploadGameViewModel>();
        shViewModel = App.GetService<ShellViewModel>();
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
        string userId = shViewModel.GetUserDisplayNameAsync().ToString();

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
        var gameName = gameNameTextBox.Text;      //Nazwa gry
        var symbol = symbolTextBox.Text;          //Symbol gry
        var currentVersion = versionTextBox.Text; //Aktualna wersja gry
        var zipLink = linkZIPTextBox.Text;        //Link do pliku ZIP z grą
        var gameLogo = gameLogoTextBox.Text;      //Link do loga gry
        var exeName = gameEXETextBox.Text;        //Plik wykonawczy gry

        //Opis gry
        string gameDescription = gameDescriptionTextBox.Text;
        string[] gameDescriptionLines = gameDescription.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        string gameDescriptionParam = string.Join(Environment.NewLine, gameDescriptionLines);

        //Wymagania sprzętowe
        string hardwareRequirements = hardwareRequirementsTextBox.Text;
        string[] hardwareRequirementsLines = hardwareRequirements.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        string hardwareRequirementsParam = string.Join(Environment.NewLine, hardwareRequirementsLines);

        //Pozostałe informacje - pole tekstowe
        string otherInformations = otherInfoTextBox.Text;
        string[] otherInformationsLines = otherInformations.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        string otherInformationsParam = string.Join(Environment.NewLine, otherInformationsLines);

        var gameEngine = selectedGameEngineId == 0 ? (int?)null : selectedGameEngineId;
        var gameType = selectedGameTypeId == 0 ? (int?)null : selectedGameTypeId;

        #region Upload to Nextcloud
        var uploader = new NextcloudUploader("https://cloud.m455yn.dev/", "sgsclient", "yGnxE-Tykxe-SwjwW-NooLc-xSwPT");
        string nextcloudGameFolder = gameName;

        string uploadedLogoUrl = null;
        List<string> uploadedImageUrls = new();

        string uploadedZipUrl = null;
        if (selectedZipFile != null)
        {
            string zipName = Guid.NewGuid() + ".zip";
            zipLink = await uploader.UploadFileAsync(selectedZipFile.Path, nextcloudGameFolder, zipName);
        }
        else if (!string.IsNullOrWhiteSpace(linkZIPTextBox.Text) && !Path.IsPathRooted(linkZIPTextBox.Text))
        {
            zipLink = linkZIPTextBox.Text;
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
            uploadedLogoUrl = await uploader.UploadFileAsync(logoPath, nextcloudGameFolder, $"logo{ext}", userId);
        }
        else
        {
            uploadedLogoUrl = logoPath;
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
            SqlCommand cmd = new SqlCommand(@"
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
            cmd.Parameters.AddWithValue("zipLink", zipLink.ToSqlParameter());
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
            if (!string.IsNullOrWhiteSpace(uploadedLogoUrl))
            {
                db.con.exec(@"
insert sgsGameLogo (GameId, LogoPath)
select
  @p0
, @p1
", gameId.ToSqlParameter(), uploadedLogoUrl.ToSqlParameter());

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

        #region Przycisk "Podgląd"
        Button previewButton = new Button();
        previewButton.Margin = new Thickness(5);
        previewButton.Click += PreviewImageButton_Click;

        var previewButtonFontIcon = new FontIcon();
        previewButtonFontIcon.Glyph = "\xE71E";
        previewButton.Content = previewButtonFontIcon;

        ToolTip previewToolTip = new ToolTip();
        previewToolTip.Content = "Podgląd";
        ToolTipService.SetToolTip(previewButton, previewToolTip);
        #endregion

        imageTextBoxPanel.Children.Add(newImageTextBox);
        imageTextBoxPanel.Children.Add(removeButton);
        //imageTextBoxPanel.Children.Add(previewButton);

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
    private void PreviewImageButton_Click(object sender, RoutedEventArgs e)
    {
        Button previewButton = sender as Button;
        StackPanel parentPanel = previewButton.Parent as StackPanel;
        TextBox imageTextBox = parentPanel.Children.OfType<TextBox>().FirstOrDefault();

        string imageURL = imageTextBox.Text;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = imageURL,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Wystąpił błąd podczas otwierania linku do logo gry: " + ex.Message);
        }
    }
    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(MyGamesPage), new DrillInNavigationTransitionInfo());
    }
}