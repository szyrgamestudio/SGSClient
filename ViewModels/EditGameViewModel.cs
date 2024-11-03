using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using SGSClient.Core.Database;
using SGSClient.Models;
using SGSClient.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static SGSClient.Views.EditGamePage;

namespace SGSClient.ViewModels;

public partial class EditGameViewModel : ObservableRecipient
{
    private readonly DbContext _dbContext;
    public ObservableCollection<GameTypeItem> GameTypes { get; set; } = new ObservableCollection<GameTypeItem>();
    public ObservableCollection<GameEngineItem> GameEngines { get; set; } = new ObservableCollection<GameEngineItem>();

    [ObservableProperty]
    public string gameName;

    [ObservableProperty]
    public string symbol;

    [ObservableProperty]
    public string currentVersion;

    [ObservableProperty]
    public string zipLink;

    [ObservableProperty]
    public string gameLogo;

    [ObservableProperty]
    public string exeName;

    [ObservableProperty]
    public string gameDescription;

    [ObservableProperty]
    public string hardwareRequirements;

    [ObservableProperty]
    public string otherInfo;

    [ObservableProperty]
    private GameTypeItem _selectedGameType;

    [ObservableProperty]
    private GameEngineItem _selectedGameEngine;

    private ObservableCollection<GameImage> _gameImages;
    public ObservableCollection<GameImage> GameImages
    {
        get => _gameImages;
        set
        {
            _gameImages = value;
            OnPropertyChanged(nameof(GameImages));
        }
    }
    public ObservableCollection<string> GameImagePaths { get; set; } = new ObservableCollection<string>();
    public EditGameViewModel(DbContext dbContext)
    {
        _dbContext = dbContext;
        GameImages = new ObservableCollection<GameImage>();
    }
    public int SelectedGameTypeId => SelectedGameType?.Id ?? 0;
    public int SelectedGameEngineId => SelectedGameEngine?.Id ?? 0;

    public async Task LoadGameData(int gameId)
    {
        var gameData = await _dbContext.ExecuteQueryAsync(SqlQueries.gameInfoSQL, gameId);
        if (gameData.Tables[0].Rows.Count > 0)
        {
            var row = gameData.Tables[0].Rows[0];

            GameName = row.IsNull("Title") ? string.Empty : row["Title"].ToString();
            Symbol = row.IsNull("Symbol") ? string.Empty : row["Symbol"].ToString();
            CurrentVersion = row.IsNull("CurrentVersion") ? string.Empty : row["CurrentVersion"].ToString();
            ZipLink = row.IsNull("ZipLink") ? string.Empty : row["ZipLink"].ToString();
            ExeName = row.IsNull("ExeName") ? string.Empty : row["ExeName"].ToString();
            GameDescription = row.IsNull("Description") ? string.Empty : row["Description"].ToString();
            HardwareRequirements = row.IsNull("HardwareRequirements") ? string.Empty : row["HardwareRequirements"].ToString();
            OtherInfo = row.IsNull("OtherInformation") ? string.Empty : row["OtherInformation"].ToString();

            int gameTypeId = Convert.ToInt32(row["TypeId"]);
            int gameEngineId = row["EngineId"] != DBNull.Value ? Convert.ToInt32(row["EngineId"]) : -1;

            SelectedGameType = GameTypes.FirstOrDefault(g => g.Id == gameTypeId);
            SelectedGameEngine = GameEngines.FirstOrDefault(g => g.Id == gameEngineId);

            var logoData = await _dbContext.ExecuteQueryAsync(SqlQueries.gameLogoSQL, gameId);
            GameLogo = (logoData.Tables[0].Rows.Count > 0 && !logoData.Tables[0].Rows[0].IsNull("LogoPath"))
                ? logoData.Tables[0].Rows[0]["LogoPath"].ToString()
                : string.Empty;

            var imagesData = await _dbContext.ExecuteQueryAsync(SqlQueries.gameImagesByIdSQL, gameId);
            GameImages.Clear();
            foreach (DataRow imageRow in imagesData.Tables[0].Rows)
            {
                string imageUrl = imageRow["ImagePath"].ToString();
                BitmapImage bitmapImage = new BitmapImage(new Uri(imageUrl));

                GameImages.Add(new GameImage(imageUrl));
            }

        }
    }
    public async Task LoadGameTypes()
    {
        try
        {
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.gameTypesSQL);
            if (dataSet.Tables.Count > 0)
            {
                GameTypes.Clear(); // Wyczyść istniejące elementy

                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    int typeId = Convert.ToInt32(row["Id"]);
                    string typeName = row["Name"].ToString();
                    var pair = new KeyValuePair<int, string>(typeId, typeName);
                    GameTypes.Add(new GameTypeItem(typeId, pair));
                }
                OnPropertyChanged(nameof(GameTypes));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    public async Task LoadGameEngines()
    {
        try
        {
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.gameEnginesSQL);
            if (dataSet.Tables.Count > 0)
            {
                GameEngines.Clear();

                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    int engineId = Convert.ToInt32(row["Id"]);
                    string engineName = row["Name"].ToString();
                    var pair = new KeyValuePair<int, string>(engineId, engineName);
                    GameEngines.Add(new GameEngineItem(engineId, pair));
                }
                OnPropertyChanged(nameof(GameEngines));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    public async Task SaveGameData(int gameId)
    {
        if (string.IsNullOrEmpty(GameName) ||
            string.IsNullOrEmpty(CurrentVersion) ||
            string.IsNullOrEmpty(ZipLink) ||
            string.IsNullOrEmpty(ExeName) ||
            string.IsNullOrEmpty(GameDescription))
        {
            return;
        }

        string pattern = @"^\d+\.\d+\.\d+$";
        bool isMatch = Regex.IsMatch(CurrentVersion, pattern);
        if (!isMatch)
        {
            ShowMessageDialog("Błąd", "Wprowadzono wersję w niepoprawnym formacie.\nUpewnij się, że format wersji wygląda następująco: x.x.x");
            return;
        }

        try
        {
            SqlParameter gameDescriptionParam = new SqlParameter("@gameDescriptionParam", SqlDbType.NVarChar)
            {
                Value = string.Join(Environment.NewLine, GameDescription.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
            };

            SqlParameter hardwareRequirementsParam = new SqlParameter("@hardwareRequirementsParam", SqlDbType.NVarChar)
            {
                Value = string.Join(Environment.NewLine, HardwareRequirements.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
            };

            SqlParameter otherInfoParam = new SqlParameter("@otherInfoParam", SqlDbType.NVarChar)
            {
                Value = string.Join(Environment.NewLine, OtherInfo.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
            };

            var parameters = new List<object>
            {
                GameName,
                Symbol,
                CurrentVersion,
                ZipLink,
                ExeName,
                gameDescriptionParam,
                hardwareRequirementsParam,
                otherInfoParam,
                SelectedGameTypeId,
                SelectedGameEngineId,
                gameId
            };
            await _dbContext.ExecuteQueryAsync(SqlQueries.updateGameDetailsSQL, parameters);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        await UpdateGameLogo(gameId, GameLogo);
        await UpdateGameImages(gameId);
    }
    private async Task UpdateGameLogo(int gameId, string logoPath)
    {
        var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.gameLogoSQL);
        if (dataSet.Tables.Count > 0)
        {
           await _dbContext.ExecuteQueryAsync(SqlQueries.updateLogoSQL, GameLogo, gameId);
        }
        else
        {
           await _dbContext.ExecuteQueryAsync(SqlQueries.insertLogoSQL, gameId, GameLogo);
        }
    }
    private async Task UpdateGameImages(int gameId)
    {
        string deleteImagesQuery = "DELETE FROM sgsGameImages WHERE GameId = @GameId";
        string insertImageQuery = "INSERT INTO sgsGameImages (GameId, ImagePath) VALUES (@GameId, @ImagePath)";

        await _dbContext.ExecuteQueryAsync(SqlQueries.deleteImagesSQL, gameId);
        foreach (var imagePath in GameImagePaths)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                await _dbContext.ExecuteQueryAsync(SqlQueries.insertImageSQL, gameId, imagePath);
            }
        }
    }

    private async void ShowMessageDialog(string title, string content)
    {
        ContentDialog messageDialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = "OK",
            //CloseButtonText = "Anuluj",
            //XamlRoot = this.Content.XamlRoot //FIXME
        };

        ContentDialogResult result = await messageDialog.ShowAsync();
    }
}
