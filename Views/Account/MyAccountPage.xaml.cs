using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
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

namespace SGSClient.Views;

public sealed partial class MyAccountPage : Microsoft.UI.Xaml.Controls.Page
{
    public class GameType
    {
        public int Id
        {
            get; set;
        }
        public string TypeName
        {
            get; set;
        }
    }
    public class GameEngine
    {
        public int Id
        {
            get; set;
        }
        public string EngineName
        {
            get; set;
        }
    }

    private void LoadGameTypes()
    {
        string connectionString = db.con;
        string query = "SELECT Id, Name FROM sgsGameTypes"; // Zakładając, że masz tabelę GameTypes z kolumnami Id i TypeName

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                List<string> gameTypeNames = new List<string>();

                while (reader.Read())
                {
                    string typeName = reader["Name"].ToString();
                    comboBoxGameType.Items.Add(typeName);
                }

                reader.Close();
            }
        }
        catch (Exception ex)
        {
            //MessageBox.Show("Błąd podczas ładowania rodzajów gier: " + ex.Message);
        }
    }
    private void LoadGameEngines()
    {
        string connectionString = db.con;
        string query = "SELECT Id, Name FROM sgsGameEngines"; // Zakładając, że masz tabelę GameTypes z kolumnami Id i TypeName

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                List<string> gameEnginesNames = new List<string>();

                while (reader.Read())
                {
                    string typeName = reader["Name"].ToString();
                    comboBoxGameEngine.Items.Add(typeName);
                }

                reader.Close();
            }
        }
        catch (Exception ex)
        {
            //MessageBox.Show("Błąd podczas ładowania rodzajów gier: " + ex.Message);
        }
    }
    public MyAccountViewModel ViewModel
    {
        get;
    }

    public MyAccountPage()
    {
        ViewModel = App.GetService<MyAccountViewModel>();
        InitializeComponent();
        LoadGameTypes();
        LoadGameEngines();
    }
    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        AppSession.CurrentUserSession.IsLoggedIn = false;
        AppSession.CurrentUserSession.UserId = null;
        AppSession.CurrentUserSession.UserName = null;

        Frame.Navigate(typeof(LoginPage), new DrillInNavigationTransitionInfo());
    }
    private void ButtonAdd_Click(object sender, RoutedEventArgs e)
    {
        // Pobranie wartości z formularza
        string gameName = gameNameTextBox.Text;
        string payloadName = "";
        string exeName = "";
        string zipLink = "";
        string versionLink = "";
        string gameDescription = gameDescriptionTextBox.Text;
        string hardwareRequirements = hardwareRequirementsTextBox.Text;
        string otherInformation = "";
        string symbol = "";
        string gameEngine = comboBoxGameEngine.Text;
        string gameType = comboBoxGameType.Text;

        string connectionString = db.con;
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            // Zapytanie SQL do wstawienia danych
            string query = @"
        insert into sgsGames (Title, DeveloperId, PayloadName, ExeName, ZipLink, VersionLink, Description, HardwareRequirements, OtherInformation, Symbol)
        select 
          @Name
        , @DeveloperId
        , @PayloadName
        , @ExeName
        , @ZipLink
        , @VersionLink
        , @Description
        , @HardwareRequirements
        , @OtherInformation
        , @Symbol
        /*
        , @GameEngine
        , @GameType
        */

        --declare @gameId int = SCOPE_IDENTITY()
        ";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", gameName);
            command.Parameters.AddWithValue("@DeveloperId", AppSession.CurrentUserSession.UserId);
            command.Parameters.AddWithValue("@PayloadName", payloadName);
            command.Parameters.AddWithValue("@ExeName", exeName);
            command.Parameters.AddWithValue("@ZipLink", zipLink);
            command.Parameters.AddWithValue("@VersionLink", versionLink);
            command.Parameters.AddWithValue("@Description", gameDescription);
            command.Parameters.AddWithValue("@HardwareRequirements", hardwareRequirements);
            command.Parameters.AddWithValue("@OtherInformation", otherInformation);
            command.Parameters.AddWithValue("@Symbol", symbol);
            command.Parameters.AddWithValue("@GameEngine", gameEngine);
            command.Parameters.AddWithValue("@GameType", gameType);

            try
            {
                // Otwarcie połączenia i wykonanie zapytania
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

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
}
