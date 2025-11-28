using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Graph.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Helpers;
using SGSClient.Core.Utilities.AppInfoUtility.Interfaces;
using SGSClient.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage;

namespace SGSClient.ViewModels;

public partial class EditGameViewModel : ObservableRecipient
{
    private readonly IAppUser _appUser;
    private readonly IAppInfo _appInfo;

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

    public EditGameViewModel(IAppUser appUser, IAppInfo appInfo)
    {
        _appUser = appUser;
        _appInfo = appInfo;
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
    public async Task LoadGameData(int gameId)
    {
        string nextcloudLogin = _appInfo.GetAppSetting("NextcloudLogin").Value;
        string nextcloudPassword = _appInfo.GetAppSetting("NextcloudPassword").Value;

        DataSet ds = db.con.select(@"
select
  g.Title
, g.Symbol
, g.CurrentVersion
, g.ZipLink
, g.ExeName
, g.Description
, g.HardwareRequirements
, g.OtherInformation
, g.TypeId
, g.EngineId
from Games g
where g.Id = @p0
", gameId);
        if (ds.Tables[0].Rows.Count > 0)
        {
            DataRow dr = ds.Tables[0].Rows[0];

            GameName = dr.TryGetValue("Title");
            Symbol = dr.TryGetValue("Symbol");
            CurrentVersion = dr.TryGetValue("CurrentVersion");
            ZipLink = dr.TryGetValue("ZipLink");
            ExeName = dr.TryGetValue("ExeName");
            GameDescription = dr.TryGetValue("Description");
            HardwareRequirements = dr.TryGetValue("HardwareRequirements");
            OtherInfo = dr.TryGetValue("OtherInformation") ?? "";

            int gameTypeId = Convert.ToInt32(dr.TryGetValue("TypeId"));
            int gameEngineId = Convert.ToInt32(dr.TryGetValue("EngineId"));

            SelectedGameType = GameTypes.FirstOrDefault(g => g.Id == gameTypeId);
            SelectedGameEngine = GameEngines.FirstOrDefault(g => g.Id == gameEngineId);

            DataSet logoData = db.con.select(@"
select
  gi.Url
from GameImages gi
where gi.GameId = @p0 and gi.LogoP = 1
", gameId);
            DataSet imagesData = db.con.select(@"
select
  gi.Url
from GameImages gi
where gi.GameId = @p0 and gi.LogoP = 0
", gameId);

            GameLogos.Clear();
            foreach (DataRow dr0 in logoData.Tables[0].Rows)
            {
                string imageUrl = dr0.TryGetValue("Url").ToString();

                await LoadLogoFromNextcloud(dr0, nextcloudLogin, nextcloudPassword);
            }

            GameImages.Clear();
            foreach (DataRow dr1 in imagesData.Tables[0].Rows)
            {
                string imageUrl = dr1.TryGetValue("Url").ToString();

                await LoadImageFromNextcloud(dr1, nextcloudLogin, nextcloudPassword);
            }

        }
    }

    public async Task<bool> SaveGameData(int gameId)
    {
        string nextcloudLogin = _appInfo.GetAppSetting("NextcloudLogin").Value;
        string nextcloudPassword = _appInfo.GetAppSetting("NextcloudPassword").Value;

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

        var uploader = new NextcloudUploader("https://cloud.m455yn.dev/", nextcloudLogin, nextcloudPassword);
        string nextcloudFolder = gameName;

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

        db.con.select(@"
update g set
  g.Title = @p0
, g.Symbol = @p1
, g.CurrentVersion = @p2
, g.ZipLink = @p3
, g.ExeName = @p4
, g.Description = @p5
, g.HardwareRequirements = @p6
, g.OtherInformation = @p7
, g.TypeId = @p8
, g.EngineId = @p9
from Games g
where g.Id = @p10", GameName, Symbol, CurrentVersion, ZipLink, ExeName, gameDescriptionParam, hardwareRequirementsParam, otherInfoParam, SelectedGameTypeId, SelectedGameEngineId, gameId);

        // utworzenie tabeli tymczasowej
        db.con.exec(@"
create table #gmi
(
    gameid int
  , url nvarchar(max)
  , logop bit
)
");

        // wrzucenie galerii
        foreach (var url in uploadedGalleryUrls)
        {
            db.con.exec(@"
insert #gmi (gameid, url, logop)
select @p0, @p1, 0
", gameId.ToSqlParameter(), url.ToSqlParameter());
        }

        // wrzucenie logo
        if (!string.IsNullOrWhiteSpace(GameLogoUrl))
        {
            db.con.exec(@"
insert #gmi (gameid, url, logop)
select @p0, @p1, 1
", gameId.ToSqlParameter(), GameLogoUrl.ToSqlParameter());
        }

        // MERGE (dodaje nowe, usuwa brakujące, zostawia stare)
        db.con.exec(@"
delete from GameImages where GameId = @p0

insert GameImages(GameId, Url, LogoP)
select
  g.gameid
, g.url
, g.logop
from #gmi g

drop table #gmi
", gameId.ToSqlParameter());

        return gameId > 0;
    }

    #region Helper Methods
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
    public async Task LoadLogoFromNextcloud(DataRow imageRow, string username, string password)
    {
        string imageUrl = imageRow["Url"].ToString();

        using (var client = new HttpClient())
        {
            var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var response = await client.GetAsync(imageUrl);
            if (response.IsSuccessStatusCode)
            {
                var imageStream = await response.Content.ReadAsStreamAsync();
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(imageStream.AsRandomAccessStream());

                GameLogos.Add(new GameImage(imageUrl, bitmapImage));
            }
            else
            {
                GameLogos.Add(new GameImage(imageUrl));
            }
        }

        IsAddLogoBtnEnabled = !GameLogos.Any();

    }
    public async Task LoadImageFromNextcloud(DataRow imageRow, string username, string password)
    {
        string imageUrl = imageRow["Url"].ToString();

        using (var client = new HttpClient())
        {
            var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var response = await client.GetAsync(imageUrl);
            if (response.IsSuccessStatusCode)
            {
                var imageStream = await response.Content.ReadAsStreamAsync();
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(imageStream.AsRandomAccessStream());

                GameImages.Add(new GameImage(imageUrl, bitmapImage));
            }
            else
            {
                GameImages.Add(new GameImage(imageUrl));
            }
        }
    }
    #endregion
}
