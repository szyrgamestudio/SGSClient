using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.ViewModels;

namespace SGSClient.Views;

public sealed partial class UploadGamePage : Microsoft.UI.Xaml.Controls.Page
{
    private int selectedGameTypeId; //zmienna przechowująca wybrany gatunek gry
    private int selectedGameEngineId; //zmienna przechowująca wybrany silnik gry
    private readonly IAppUser _appUser;  // Declare _appUser
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
    public UploadGamePage()
    {
        ViewModel = App.GetService<UploadGameViewModel>();
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

        //if (result == ContentDialogResult.Primary)
        //{
        //    // Użytkownik kliknął OK
        //}
        //else
        //{
        //    // Użytkownik kliknął Anuluj lub zamknął okno dialogowe
        //}
    }
    private void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
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
        SqlParameter gameDescriptionParam = new SqlParameter("@gameDescriptionParam", SqlDbType.NVarChar);
        gameDescriptionParam.Value = string.Join(Environment.NewLine, gameDescriptionLines);

        //Wymagania sprzętowe
        string hardwareRequirements = hardwareRequirementsTextBox.Text;
        string[] hardwareRequirementsLines = hardwareRequirements.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        SqlParameter hardwareRequirementsParam = new SqlParameter("@hardwareRequirementsParam", SqlDbType.NVarChar);
        hardwareRequirementsParam.Value = string.Join(Environment.NewLine, hardwareRequirementsLines);

        //Pozostałe informacje - pole tekstowe
        string otherInformations = otherInfoTextBox.Text;
        string[] otherInformationsLines = otherInformations.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        SqlParameter otherInformationsParam = new SqlParameter("@otherInformationsParam", SqlDbType.NVarChar);
        otherInformationsParam.Value = string.Join(Environment.NewLine, otherInformationsLines);

        //Galeria zdjęć
        List<string> galleryImageUrls = new List<string>();
        foreach (var child in gameGalleryStackPanel.Children)
        {
            if (child is StackPanel stackPanel)
            {
                TextBox textBox = stackPanel.Children.OfType<TextBox>().FirstOrDefault();
                if (textBox != null && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    galleryImageUrls.Add(textBox.Text);
                }
            }
        }


        var gameEngine = selectedGameEngineId == 0 ? (int?)null : selectedGameEngineId;
        var gameType = selectedGameTypeId == 0 ? (int?)null : selectedGameTypeId;

        try
        {
            SqlCommand cmd = new SqlCommand(@"
declare @developerId int = (select r.DeveloperId from Registration r where r.Id = @userId)

insert sgsGames (Title, DeveloperId, PayloadName, ExeName, ZipLink, VersionLink, CurrentVersion, Description, HardwareRequirements, OtherInformation, Symbol, EngineId, TypeId, DraftP)
select 
  @Name
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
            cmd.Parameters.AddWithValue("userId", _appUser.UserId.ToSqlParameter());
            cmd.Parameters.AddWithValue("exeName", exeName.ToSqlParameter());
            cmd.Parameters.AddWithValue("zipLink", zipLink.ToSqlParameter());
            cmd.Parameters.AddWithValue("currentVersion", currentVersion.ToSqlParameter());
            cmd.Parameters.AddWithValue("gameDescriptionParam", gameDescriptionParam.ToSqlParameter());
            cmd.Parameters.AddWithValue("hardwareRequirementsParam", hardwareRequirementsParam.ToSqlParameter());
            cmd.Parameters.AddWithValue("otherInformationsParam", otherInformationsParam.ToSqlParameter());
            cmd.Parameters.AddWithValue("symbol", symbol.ToSqlParameter());
            cmd.Parameters.AddWithValue("gameEngine", gameEngine.ToSqlParameter());
            cmd.Parameters.AddWithValue("gameType", gameType.ToSqlParameter());

            int gameId = (int)db.scalarSQL(cmd);

            //-----------------------------------
            cmd.Parameters.Clear();
            cmd = new SqlCommand(@"
insert sgsGameImages (GameId, ImagePath)
select
  @GameId
, @ImageUrl
", db.con);

            cmd.Parameters.AddWithValue("gameId", gameId.ToSqlParameter());
            foreach (string imageUrl in galleryImageUrls)
            {
                cmd.Parameters.AddWithValue("imageUrl", imageUrl.ToSqlParameter());
                db.execSQL(cmd);

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("gameId", gameId.ToSqlParameter());
            }
            //-----------------------------------
            cmd.Parameters.Clear();
            cmd = new SqlCommand(@"
insert sgsGameLogo (GameId, LogoPath)
select
  @GameId
, @ImageUrl
", db.con);

            cmd.Parameters.AddWithValue("gameId", gameId.ToSqlParameter());
            cmd.Parameters.AddWithValue("gameId", gameLogo.ToSqlParameter());
            db.execSQL(cmd);

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
    private void AddImageButton_Click(object sender, RoutedEventArgs e)
    {
        // Tworzenie nowego TextBoxa dla kolejnego zdjęcia
        StackPanel imageTextBoxPanel = new StackPanel();
        imageTextBoxPanel.Orientation = Orientation.Horizontal;

        TextBox newImageTextBox = new TextBox();
        newImageTextBox.Name = "ImageTextBox" + (additionalImageCount + 1);
        newImageTextBox.Margin = new Thickness(5);
        newImageTextBox.PlaceholderText = "Wstaw link do zdjęcia";
        newImageTextBox.Width = 400;
        newImageTextBox.TextWrapping = TextWrapping.Wrap;

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

        // Dodanie nowego TextBoxa do StackPanelu
        imageTextBoxPanel.Children.Add(newImageTextBox);
        imageTextBoxPanel.Children.Add(removeButton);
        imageTextBoxPanel.Children.Add(previewButton);

        //StackPanel.SetMargin(imageTextBoxPanel, new Thickness(5));
        gameGalleryStackPanel.Children.Insert(gameGalleryStackPanel.Children.Count - 1, imageTextBoxPanel); // Dodaj na przedostatniej pozycji

        // Zwiększanie licznika dodatkowych zdjęć
        additionalImageCount++;

        // Ukrycie przycisku dodawania zdjęcia, jeśli osiągnięto limit
        if (additionalImageCount >= 10) // Dla przykładu, limit 10 zdjęć
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
    private void gotoSGSClientWWW_Click(object sender, RoutedEventArgs e)
    {
        var URL = "https://sgsclient.massyn.dev/upload";
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
    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(MyGamesPage), new DrillInNavigationTransitionInfo());
    }

    //[Obsolete]
    //private void LogoutButton_Click(object sender, RoutedEventArgs e)
    //{
    //    _appUser.IsLoggedIn = false;
    //    _appUser.UserId = null;
    //    _appUser.UserName = null;

    //    Frame.Navigate(typeof(LoginPage), new DrillInNavigationTransitionInfo());
    //}

}