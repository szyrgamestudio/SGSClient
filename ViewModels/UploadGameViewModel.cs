using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml.Controls;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Helpers;
using SGSClient.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using Windows.Storage;
namespace SGSClient.ViewModels;

public partial class UploadGameViewModel : ObservableRecipient
{
    private readonly IAppUser _appUser;
    private StorageFile _zipFile;
    public StorageFile ZipFile
    {
        get => _zipFile;
        set
        {
            _zipFile = value;
            OnPropertyChanged();
        }
    }
    public string NextcloudUsername { get; set; } = "sgsclient";
    public string NextcloudPassword { get; set; } = "yGnxE-Tykxe-SwjwW-NooLc-xSwPT";
    public ObservableCollection<GameTypeItem> GameTypes { get; set; } = [];
    public ObservableCollection<GameEngineItem> GameEngines { get; set; } = [];

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
    public string GameLogoUrl { get; set; }

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

    public UploadGameViewModel(IAppUser appUser)
    {
        _appUser = appUser;
        GameLogos = [];
        GameImages = [];
    }

    public Task LoadGameTypes()
    {
        try
        {
            DataSet ds = db.con.select(@"
select
  gt.Id
, gt.Name
from GameTypes gt
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

        return Task.CompletedTask;
    }
    public Task LoadGameEngines()
    {
        try
        {
            DataSet ds = db.con.select(@"
select
  ge.Id
, ge.Name
from GameEngines ge
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

        return Task.CompletedTask;
    }
    public async Task<bool> AddGameData()
    {
        if (string.IsNullOrEmpty(GameName) ||
            string.IsNullOrEmpty(CurrentVersion) ||
            (ZipFile == null && string.IsNullOrEmpty(ZipLink)) ||
            string.IsNullOrEmpty(ExeName) ||
            string.IsNullOrEmpty(GameDescription))
        {
            return false;
        }

        string pattern = @"^\d+\.\d+\.\d+$";
        bool isMatch = Regex.IsMatch(CurrentVersion, pattern);
        if (!isMatch)
        {
            ShowMessageDialog("Błąd", "Wprowadzono wersję w niepoprawnym formacie.\nUpewnij się, że format wersji wygląda następująco: x.x.x");
            return false;
        }

        string gameDescriptionParam = string.Join(Environment.NewLine, GameDescription.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None));
        string hardwareRequirementsParam = string.Join(Environment.NewLine, HardwareRequirements.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None));
        string otherInfoParam = string.Join(Environment.NewLine, OtherInfo.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None));

        var uploader = new NextcloudUploader("https://cloud.m455yn.dev/", NextcloudUsername, NextcloudPassword);
        string nextcloudFolder = GameName;

        #region ZIP File Upload
        if (ZipFile != null)
        {
            string zipName = Guid.NewGuid() + ".zip";
            ZipLink = await uploader.UploadFileAsync(ZipFile.Path, nextcloudFolder, zipName);
        }
        else if (Path.IsPathRooted(ZipLink))
        {
            ZipLink = await uploader.UploadFileAsync(ZipLink, nextcloudFolder, Guid.NewGuid() + ".zip");
        }
        #endregion

        #region GameLogo File Upload
        if (GameLogos.FirstOrDefault() is GameImage logoImage && Path.IsPathRooted(logoImage.Url))
        {
            string ext = Path.GetExtension(logoImage.Url);
            GameLogoUrl = await uploader.UploadFileAsync(logoImage.Url, nextcloudFolder, $"logo{ext}", _appUser.GetCurrentUser().UserId);
        }
        #endregion

        #region GameScreenshots Files Upload
        List<string> uploadedGalleryUrls = new();
        int index = 1;
        foreach (var path in GameImages)
        {
            if (string.IsNullOrWhiteSpace(path.Url)) continue;

            if (Path.IsPathRooted(path.Url))
            {
                string ext = Path.GetExtension(path.Url);
                string uploadedUrl = await uploader.UploadFileAsync(path.Url, nextcloudFolder, $"zrzutEkranu_{index++}{ext}", _appUser.GetCurrentUser().UserId);
                if (uploadedUrl != null)
                    uploadedGalleryUrls.Add(uploadedUrl);
            }
            else
            {
                uploadedGalleryUrls.Add(path.Url);
            }
        }
        #endregion

        SqlCommand cmd = new(@"
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
        cmd.Parameters.AddWithValue("userId", _appUser.GetCurrentUser().UserId.ToSqlParameter());
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

        foreach (var url in uploadedGalleryUrls)
        {
            db.con.exec(@"
insert GameImages (GameId, Url, LogoP)
select @p0, @p1, 0
", gameId.ToSqlParameter(), url.ToSqlParameter());
        }

        if (!string.IsNullOrWhiteSpace(GameLogoUrl))
        {
            db.con.exec(@"
insert GameImages (GameId, Url, LogoP)
select @p0, @p1, 1
", gameId.ToSqlParameter(), GameLogoUrl.ToSqlParameter());
        }

        return gameId > 0;
    }
    private static async void ShowMessageDialog(string title, string content)
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