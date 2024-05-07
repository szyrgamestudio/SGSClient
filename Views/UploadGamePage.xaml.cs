using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.ViewModels;
using Windows.UI;
using Windows.UI.Text;
using static System.Net.Mime.MediaTypeNames;

namespace SGSClient.Views;

public sealed partial class UploadGamePage : Microsoft.UI.Xaml.Controls.Page
{
    private int selectedGameTypeId; //zmienna przechowująca wybrany gatunek gry
    private int selectedGameEngineId; //zmienna przechowująca wybrany silnik gry

    public class GameTypeItem
    {
        public int Id { get; set; }
        public KeyValuePair<int, string> Pair { get; set; }

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
        public int Id { get; set; }
        public KeyValuePair<int, string> Pair { get; set; }

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
        string connectionString = db.con;
        string query = "select sgsgt.Id, sgsgt.Name from sgsGameTypes sgsgt";

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                List<GameTypeItem> gameTypeList = new List<GameTypeItem>();

                while (reader.Read())
                {
                    int typeId = Convert.ToInt32(reader["Id"]);
                    string typeName = reader["Name"].ToString();
                    var pair = new KeyValuePair<int, string>(typeId, typeName);
                    gameTypeList.Add(new GameTypeItem(typeId, pair));
                }

                reader.Close();

                foreach (var item in gameTypeList)
                {
                    comboBoxGameType.Items.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            // Obsługa błędów
        }
    }

    /// <summary>
    /// Wczytaj wszystkie silniki gier i zapisz je do comboboxa
    /// </summary>
    private void LoadGameEngines()
    {
        string connectionString = db.con;
        string query = "select sgseg.Id, sgseg.Name from sgsGameEngines sgseg";

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                List<GameEngineItem> enginesList = new List<GameEngineItem>();

                while (reader.Read())
                {
                    int engineId = Convert.ToInt32(reader["Id"]);
                    string engineName = reader["Name"].ToString();
                    var pair = new KeyValuePair<int, string>(engineId, engineName);
                    enginesList.Add(new GameEngineItem(engineId, pair));
                }

                reader.Close();
                foreach (var item in enginesList)
                {
                    comboBoxGameEngine.Items.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            //MessageBox.Show("Błąd podczas ładowania rodzajów gier: " + ex.Message);
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
        else
        {
            // Obsługa przypadku, gdy nic nie jest wybrane w comboBoxGameType
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
        else
        {
            // Obsługa przypadku, gdy nic nie jest wybrane w comboBoxGameType
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
    private void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        if (gameNameTextBox.Text.Length == 0 || gameDescriptionTextBox.Text.Length == 0 || hardwareRequirementsTextBox.Text.Length == 0)
        {
            return;
        }

        GetSelectedGameTypeKey();
        GetSelectedGameEngineKey();
        // Pobranie wartości z formularza
        string gameName = gameNameTextBox.Text;
        string payloadName = "";
        string exeName = "";
        string zipLink = "";
        string versionLink = "";

        /*Opis gry*/
        string gameDescription = gameDescriptionTextBox.Text;
        string[] gameDescriptionLines = gameDescription.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        SqlParameter gameDescriptionParam = new SqlParameter("@gameDescriptionParam", SqlDbType.NVarChar);
        gameDescriptionParam.Value = string.Join(Environment.NewLine, gameDescriptionLines);

        /*Wymagania sprzętowe*/
        string hardwareRequirements = hardwareRequirementsTextBox.Text;
        string[] hardwareRequirementsLines = hardwareRequirements.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        SqlParameter hardwareRequirementsParam = new SqlParameter("@hardwareRequirementsParam", SqlDbType.NVarChar);
        hardwareRequirementsParam.Value = string.Join(Environment.NewLine, hardwareRequirementsLines);

        string otherInformation = "";
        string symbol = "";
        string ?gameEngine = comboBoxGameEngine.Text;

        var gameTypePair = (KeyValuePair<int, string>)comboBoxGameType.SelectedItem;
        int selectedKey = gameTypePair.Key;
        string ?gameType = comboBoxGameType.SelectedValue.ToString();

        if (gameEngine.Length == 0 || gameType.Length == 0)
        {
            return; //jawale syf
        }

        string connectionString = db.con;
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            // Zapytanie SQL do wstawienia danych
            string query = @"
        insert into sgsGames (Title, DeveloperId, PayloadName, ExeName, ZipLink, VersionLink, Description, HardwareRequirements, OtherInformation, Symbol, EngineId, TypeId, DraftP)
        select 
          @Name
        , @DeveloperId
        , @PayloadName
        , @ExeName
        , @ZipLink
        , @VersionLink
        , @gameDescriptionParam
        , @hardwareRequirementsParam
        , @OtherInformation
        , @Symbol
        , @GameEngine
        , @GameType
        , 1
        ";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", gameName);
            command.Parameters.AddWithValue("@DeveloperId", AppSession.CurrentUserSession.UserId);
            command.Parameters.AddWithValue("@PayloadName", payloadName);
            command.Parameters.AddWithValue("@ExeName", exeName);
            command.Parameters.AddWithValue("@ZipLink", zipLink);
            command.Parameters.AddWithValue("@VersionLink", versionLink);
            command.Parameters.Add(gameDescriptionParam);
            command.Parameters.Add(hardwareRequirementsParam);
            command.Parameters.AddWithValue("@OtherInformation", otherInformation);
            command.Parameters.AddWithValue("@Symbol", symbol);
            command.Parameters.AddWithValue("@GameEngine", gameEngine);
            command.Parameters.AddWithValue("@GameType", gameType);


            try
            {
                // Otwarcie połączenia i wykonanie zapytania
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                //int rowsAffected = 1;

                // Sprawdzenie czy dane zostały poprawnie dodane
                if (rowsAffected > 0)
                {
                    //MessageBox.Show("Gra została pomyślnie dodana do bazy danych.");
                    // Tutaj możesz dodać dodatkowe czynności po dodaniu gry
                }
                else
                {
                    //MessageBox.Show("Błąd podczas dodawania gry do bazy danych.");
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Błąd: " + ex.Message);
            }
        }

    }
    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        AppSession.CurrentUserSession.IsLoggedIn = false;
        AppSession.CurrentUserSession.UserId = null;
        AppSession.CurrentUserSession.UserName = null;

        Frame.Navigate(typeof(LoginPage), new DrillInNavigationTransitionInfo());
    }

}
