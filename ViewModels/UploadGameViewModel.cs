using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml.Controls;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Helpers;
using SGSClient.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.Gaming.Input;
using Windows.Storage;
namespace SGSClient.ViewModels;

public partial class UploadGameViewModel : ObservableRecipient
{
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

    public int SelectedGameTypeId => SelectedGameType?.Id ?? 0;
    public int SelectedGameEngineId => SelectedGameEngine?.Id ?? 0;

    #region Logo
    private ObservableCollection<GameImage> _gameLogos;
    public ObservableCollection<GameImage> GameLogos
    {
        get => _gameLogos;
        set
        {
            _gameLogos = value;
            OnPropertyChanged(nameof(GameLogos));
        }
    }
    public ObservableCollection<string> GameLogoPaths { get; set; } = new ObservableCollection<string>();

    private bool _isAddLogoBtnEnabled;
    public bool IsAddLogoBtnEnabled
    {
        get => _isAddLogoBtnEnabled;
        set
        {
            if (_isAddLogoBtnEnabled != value)
            {
                _isAddLogoBtnEnabled = value;
                OnPropertyChanged(nameof(IsAddLogoBtnEnabled));
            }
        }
    }

    #endregion

    #region Images gallery
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
    #endregion

    public UploadGameViewModel()
    {
        GameLogos = new ObservableCollection<GameImage>();
        GameImages = new ObservableCollection<GameImage>();
    }

    public async Task LoadGameTypes()
    {
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
                GameTypes.Clear();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int typeId = dr.TryGetValue("Id");
                    string typeName = dr.TryGetValue("Name");
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
            DataSet ds = db.con.select(@"
select
  ge.Id
, ge.Name
from sgsGameEngines ge
");
            if (ds.Tables.Count > 0)
            {
                GameEngines.Clear();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int engineId = dr.TryGetValue("Id");
                    string engineName = dr.TryGetValue("Name");

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
    public async Task AddGameData(string userId)
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

        string gameDescriptionParam = string.Join(Environment.NewLine, GameDescription.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None));
        string hardwareRequirementsParam = string.Join(Environment.NewLine, HardwareRequirements.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None));
        string otherInfoParam = string.Join(Environment.NewLine, OtherInfo.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None));

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

        cmd.Parameters.AddWithValue("gameName", GameName.ToSqlParameter());
        cmd.Parameters.AddWithValue("userId", userId.ToSqlParameter());
        cmd.Parameters.AddWithValue("exeName", ExeName.ToSqlParameter());
        cmd.Parameters.AddWithValue("zipLink", ZipLink.ToSqlParameter());
        cmd.Parameters.AddWithValue("currentVersion", CurrentVersion.ToSqlParameter());
        cmd.Parameters.AddWithValue("gameDescriptionParam", gameDescriptionParam.ToSqlParameter());
        cmd.Parameters.AddWithValue("hardwareRequirementsParam", hardwareRequirementsParam.ToSqlParameter());
        cmd.Parameters.AddWithValue("otherInformationsParam", otherInfoParam.ToSqlParameter());
        cmd.Parameters.AddWithValue("symbol", Symbol.ToSqlParameter());
        cmd.Parameters.AddWithValue("gameEngine", SelectedGameEngineId.ToSqlParameter());
        cmd.Parameters.AddWithValue("gameType", SelectedGameTypeId.ToSqlParameter());

        int gameId = Convert.ToInt32(db.scalarSQL(cmd));

        await UpdateGameLogo(gameId);
        await UpdateGameImages(gameId);
    }
    private async Task UpdateGameLogo(int gameId)
    {

    }
    private async Task UpdateGameImages(int gameId)
    {

    }
    private async void ShowMessageDialog(string title, string content)
    {
        ContentDialog messageDialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = "OK",
        };

        ContentDialogResult result = await messageDialog.ShowAsync();
    }
}