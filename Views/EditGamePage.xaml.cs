using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.ViewModels;

namespace SGSClient.Views
{
    public sealed partial class EditGamePage : Page
    {
        private int selectedGameTypeId;
        private int selectedGameEngineId;
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string gameIdParam && int.TryParse(gameIdParam, out int parsedGameId))
            {
                gameId = parsedGameId;
                GameId = gameIdParam;

                // Load game data after gameId has been set
                LoadGameData();
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
            LoadGameTypes();
            LoadGameEngines();
            //LoadGameData();
        }

        private void LoadGameTypes()
        {
            string query = "SELECT Id, Name FROM sgsGameTypes";

            try
            {
                using (SqlConnection connection = db.Connect()) // Używamy metody Connect z klasy db
                {
                    // Używamy SelectSQL z klasy db, aby wykonać zapytanie i otrzymać DataSet
                    DataSet dataSet = db.SelectSQL(connection, query);
                    List<GameTypeItem> gameTypeList = new List<GameTypeItem>();

                    // Sprawdzamy, czy są jakiekolwiek tabele w DataSet
                    if (dataSet.Tables.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[0].Rows)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void LoadGameEngines()
        {
            string query = "SELECT Id, Name FROM sgsGameEngines";

            try
            {
                using (SqlConnection connection = db.Connect()) // Używamy metody Connect z klasy db
                {
                    // Używamy SelectSQL z klasy db, aby wykonać zapytanie i otrzymać DataSet
                    DataSet dataSet = db.SelectSQL(connection, query);
                    List<GameEngineItem> enginesList = new List<GameEngineItem>();

                    // Sprawdzamy, czy są jakiekolwiek tabele w DataSet
                    if (dataSet.Tables.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[0].Rows)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void GetSelectedGameTypeKey()
        {
            if (comboBoxGameType.SelectedItem != null)
            {
                var selectedGameType = (GameTypeItem)comboBoxGameType.SelectedItem;
                selectedGameTypeId = selectedGameType.Id;
            }
        }

        private void GetSelectedGameEngineKey()
        {
            if (comboBoxGameEngine.SelectedItem != null)
            {
                var selectedGameEngine = (GameEngineItem)comboBoxGameEngine.SelectedItem;
                selectedGameEngineId = selectedGameEngine.Id;
            }
        }

        private void LoadGameData()
        {
            string query = "SELECT * FROM sgsGames WHERE Id = @GameId";

            try
            {
                using (SqlConnection connection = db.Connect()) // Używamy metody Connect z klasy db
                {
                    SqlCommand command = db.CommandSQL(connection, query, gameId);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        gameNameTextBox.Text = reader["Title"].ToString();
                        symbolTextBox.Text = reader["Symbol"].ToString();
                        versionTextBox.Text = reader["CurrentVersion"].ToString();
                        linkZIPTextBox.Text = reader["ZipLink"].ToString();
                        gameEXETextBox.Text = reader["ExeName"].ToString();
                        gameDescriptionTextBox.Text = reader["Description"].ToString();
                        hardwareRequirementsTextBox.Text = reader["HardwareRequirements"].ToString();
                        otherInfoTextBox.Text = reader["OtherInformation"].ToString();

                        selectedGameTypeId = reader["TypeId"] != DBNull.Value ? Convert.ToInt32(reader["TypeId"]) : 0;
                        selectedGameEngineId = reader["EngineId"] != DBNull.Value ? Convert.ToInt32(reader["EngineId"]) : 0;

                        reader.Close();

                        // Load Game Logo
                        string logoQuery = "SELECT LogoPath FROM sgsGameLogo WHERE GameId = @GameId";
                        SqlCommand logoCommand = db.CommandSQL(connection, logoQuery, gameId);
                        SqlDataReader logoReader = logoCommand.ExecuteReader();

                        if (logoReader.Read())
                        {
                            gameLogoTextBox.Text = logoReader["LogoPath"].ToString();
                        }
                        logoReader.Close();

                        // Load Game Images
                        string imagesQuery = "SELECT ImagePath FROM sgsGameImages WHERE GameId = @GameId";
                        SqlCommand imagesCommand = db.CommandSQL(connection, imagesQuery, gameId);
                        SqlDataReader imagesReader = imagesCommand.ExecuteReader();

                        while (imagesReader.Read())
                        {
                            AddImageTextBox(imagesReader["ImagePath"].ToString());
                        }
                        imagesReader.Close();

                        // Set ComboBox selections
                        SetComboBoxSelections();
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void SetComboBoxSelections()
        {
            foreach (GameTypeItem item in comboBoxGameType.Items)
            {
                if (item.Id == selectedGameTypeId)
                {
                    comboBoxGameType.SelectedItem = item;
                    break;
                }
            }

            foreach (GameEngineItem item in comboBoxGameEngine.Items)
            {
                if (item.Id == selectedGameEngineId)
                {
                    comboBoxGameEngine.SelectedItem = item;
                    break;
                }
            }
        }

        private int additionalImageCount = 0;
        private void AddImageTextBox(string imageUrl)
        {
            StackPanel imageTextBoxPanel = new StackPanel { Orientation = Orientation.Horizontal };

            TextBox newImageTextBox = new TextBox
            {
                Name = "ImageTextBox" + (additionalImageCount + 1),
                Margin = new Thickness(5),
                PlaceholderText = "Wstaw link do zdjęcia",
                Width = 400,
                Text = imageUrl
            };

            Button removeButton = new Button
            {
                Margin = new Thickness(5),
                Content = new FontIcon { Glyph = "\xE74D" }
            };
            removeButton.Click += RemoveImageButton_Click;
            ToolTipService.SetToolTip(removeButton, new ToolTip { Content = "Usuń" });

            Button previewButton = new Button
            {
                Margin = new Thickness(5),
                Content = new FontIcon { Glyph = "\xE71E" }
            };
            previewButton.Click += PreviewImageButton_Click;
            ToolTipService.SetToolTip(previewButton, new ToolTip { Content = "Podgląd" });

            imageTextBoxPanel.Children.Add(newImageTextBox);
            imageTextBoxPanel.Children.Add(removeButton);
            imageTextBoxPanel.Children.Add(previewButton);

            gameGalleryStackPanel.Children.Insert(gameGalleryStackPanel.Children.Count - 1, imageTextBoxPanel);

            additionalImageCount++;
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

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(gameNameTextBox.Text) ||
                string.IsNullOrEmpty(versionTextBox.Text) ||
                string.IsNullOrEmpty(linkZIPTextBox.Text) ||
                string.IsNullOrEmpty(gameEXETextBox.Text) ||
                string.IsNullOrEmpty(gameDescriptionTextBox.Text))
            {
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

            var gameName = gameNameTextBox.Text;
            var symbol = symbolTextBox.Text;
            var currentVersion = versionTextBox.Text;
            var zipLink = linkZIPTextBox.Text;
            var gameLogo = gameLogoTextBox.Text;
            var exeName = gameEXETextBox.Text;

            string gameDescription = gameDescriptionTextBox.Text;
            SqlParameter gameDescriptionParam = new SqlParameter("@gameDescriptionParam", SqlDbType.NVarChar)
            {
                Value = string.Join(Environment.NewLine, gameDescription.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
            };

            string hardwareRequirements = hardwareRequirementsTextBox.Text;
            SqlParameter hardwareRequirementsParam = new SqlParameter("@hardwareRequirementsParam", SqlDbType.NVarChar)
            {
                Value = string.Join(Environment.NewLine, hardwareRequirements.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
            };

            string otherInfo = otherInfoTextBox.Text;
            SqlParameter otherInfoParam = new SqlParameter("@otherInfoParam", SqlDbType.NVarChar)
            {
                Value = string.Join(Environment.NewLine, otherInfo.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
            };

            string updateGameQuery = @"
    UPDATE g SET
        g.Title = @GameName,
        g.Symbol = @Symbol,
        g.CurrentVersion = @Version,
        g.ZipLink = @ZipLink,
        g.ExeName = @ExeName,
        g.Description = @gameDescriptionParam,
        g.HardwareRequirements = @hardwareRequirementsParam,
        g.OtherInformation = @otherInfoParam,
        g.TypeId = @GameTypeId,
        g.EngineId = @GameEngineId
    FROM sgsGames g
    WHERE g.Id = @GameId";

            try
            {
                using (SqlConnection connection = db.Connect()) // Używamy metody Connect z klasy db
                {
                    SqlCommand updateCommand = db.CommandSQL(connection, updateGameQuery,
                        gameName,
                        symbol,
                        currentVersion,
                        zipLink,
                        exeName,
                        gameDescriptionParam,
                        hardwareRequirementsParam,
                        otherInfoParam,
                        selectedGameTypeId,
                        selectedGameEngineId,
                        gameId);

                    connection.Open();
                    updateCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // Update Game Logo
            UpdateGameLogo(gameId, gameLogo);

            // Update Game Images
            UpdateGameImages(gameId);

            // Navigate back to the previous page
            Frame.GoBack(new DrillInNavigationTransitionInfo());
        }

        #region Actions
        private void UpdateGameLogo(int gameId, string gameLogo)
        {
            try
            {
                using (SqlConnection connection = db.Connect())
                {
                    try
                    {
                        // Sprawdzenie, czy logo dla danej gry już istnieje
                        string selectLogoQuery = "SELECT COUNT(*) FROM GameLogos WHERE GameId = @GameId";
                        using (SqlCommand selectCommand = db.CommandSQL(connection, selectLogoQuery))
                        {
                            selectCommand.Parameters.AddWithValue("@GameId", gameId);
                            connection.Open();

                            int count = (int)selectCommand.ExecuteScalar();

                            // Jeśli logo istnieje, aktualizujemy je
                            if (count > 0)
                            {
                                string updateLogoQuery = "UPDATE GameLogos SET LogoPath = @LogoPath WHERE GameId = @GameId";
                                using (SqlCommand updateCommand = db.CommandSQL(connection, updateLogoQuery))
                                {
                                    updateCommand.Parameters.AddWithValue("@GameId", gameId);
                                    updateCommand.Parameters.AddWithValue("@LogoPath", gameLogo);
                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                            else // Jeśli logo nie istnieje, dodajemy nowe
                            {
                                string insertLogoQuery = "INSERT INTO GameLogos (GameId, LogoPath) VALUES (@GameId, @LogoPath)";
                                using (SqlCommand insertCommand = db.CommandSQL(connection, insertLogoQuery))
                                {
                                    insertCommand.Parameters.AddWithValue("@GameId", gameId);
                                    insertCommand.Parameters.AddWithValue("@LogoPath", gameLogo);
                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Wystąpił błąd podczas aktualizacji logo: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private void UpdateGameImages(int gameId)
        {
            string deleteImagesQuery = "DELETE FROM sgsGameImages WHERE GameId = @GameId";
            string insertImageQuery = "INSERT INTO sgsGameImages (GameId, ImagePath) VALUES (@GameId, @ImagePath)";

            try
            {
                using (SqlConnection connection = db.Connect())
                {
                    try
                    {
                        // Usuwanie obrazów związanych z grą
                        using (SqlCommand deleteCommand = db.CommandSQL(connection, deleteImagesQuery))
                        {
                            deleteCommand.Parameters.AddWithValue("@GameId", gameId);
                            connection.Open();
                            deleteCommand.ExecuteNonQuery();
                        }

                        // Dodawanie nowych obrazów
                        foreach (var child in gameGalleryStackPanel.Children)
                        {
                            if (child is StackPanel panel && panel.Children[0] is TextBox textBox)
                            {
                                string imagePath = textBox.Text;
                                if (!string.IsNullOrEmpty(imagePath))
                                {
                                    using (SqlCommand insertCommand = db.CommandSQL(connection, insertImageQuery))
                                    {
                                        insertCommand.Parameters.AddWithValue("@GameId", gameId);
                                        insertCommand.Parameters.AddWithValue("@ImagePath", imagePath);
                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Wystąpił błąd podczas aktualizacji obrazów: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        #endregion

        #region Buttons
        private void RemoveImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Parent is StackPanel panel)
            {
                gameGalleryStackPanel.Children.Remove(panel);
            }
        }
        private void PreviewImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Parent is StackPanel panel && panel.Children[0] is TextBox textBox)
            {
                string imageUrl = textBox.Text;
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var imagePreviewDialog = new ContentDialog
                    {
                        Title = "Podgląd zdjęcia",
                        Content = new Image { Source = new BitmapImage(new Uri(imageUrl)), Stretch = Stretch.Uniform },
                        CloseButtonText = "Zamknij"
                    };

                    _ = imagePreviewDialog.ShowAsync();
                }
            }
        }
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
        #endregion
    }
}
