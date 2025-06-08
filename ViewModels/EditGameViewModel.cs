using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Helpers;
using SGSClient.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage;
using static SGSClient.Views.EditGamePage;
using static System.Net.Mime.MediaTypeNames;

namespace SGSClient.ViewModels;

public partial class EditGameViewModel : ObservableRecipient
{
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

    public EditGameViewModel()
    {
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

        return Task.CompletedTask;
    }
    public async Task LoadGameData(int gameId)
    {
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
from sgsGames g
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
            OtherInfo = dr.TryGetValue("OtherInformation");

            int gameTypeId = Convert.ToInt32(dr.TryGetValue("TypeId"));
            int gameEngineId = Convert.ToInt32(dr.TryGetValue("EngineId"));

            SelectedGameType = GameTypes.FirstOrDefault(g => g.Id == gameTypeId);
            SelectedGameEngine = GameEngines.FirstOrDefault(g => g.Id == gameEngineId);

            DataSet logoData = db.con.select(@"
select
  l.LogoPath
from sgsGameLogo l
where l.GameId = @p0
", gameId);
            DataSet imagesData = db.con.select(SqlQueries.gameImagesByIdSQL, gameId);

            GameLogos.Clear();
            foreach (DataRow dr0 in logoData.Tables[0].Rows)
            {
                string imageUrl = dr0.TryGetValue("LogoPath").ToString();

                await LoadLogoFromNextcloud(dr0, NextcloudUsername, NextcloudPassword);
            }

            GameImages.Clear();
            foreach (DataRow dr1 in imagesData.Tables[0].Rows)
            {
                string imageUrl = dr1.TryGetValue("ImagePath").ToString();

                await LoadImageFromNextcloud(dr1, NextcloudUsername, NextcloudPassword);
            }

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
            string gameDescriptionParam = string.Join(Environment.NewLine, GameDescription.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None));
            string hardwareRequirementsParam = string.Join(Environment.NewLine, HardwareRequirements.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None));
            string otherInfoParam = string.Join(Environment.NewLine, OtherInfo.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None));

            db.con.select(SqlQueries.updateGameDetailsSQL, GameName, Symbol, CurrentVersion, ZipLink, ExeName, gameDescriptionParam, hardwareRequirementsParam, otherInfoParam, SelectedGameTypeId, SelectedGameEngineId, gameId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        await UpdateGameLogo(gameId);
        await UpdateGameImages(gameId);
    }
    private async Task UpdateGameLogo(int gameId)
    {
        db.con.select(SqlQueries.deleteLogoSQL, gameId);
        var uploader = new NextcloudUploader("https://cloud.m455yn.dev/", "sgsclient", "yGnxE-Tykxe-SwjwW-NooLc-xSwPT");

        foreach (var logo in GameLogos)
        {
            string imageUrl = logo.Url;

            if (Path.IsPathRooted(imageUrl))
            {
                var file = await StorageFile.GetFileFromPathAsync(imageUrl);
                string uploadedUrl = await uploader.UploadFileAsync(file.Path);

                if (uploadedUrl != null)
                {
                    imageUrl = uploadedUrl;
                }
            }
            db.con.select(SqlQueries.insertLogoSQL, gameId, imageUrl);
        }
    }
    private async Task UpdateGameImages(int gameId)
    {
        db.con.select(SqlQueries.deleteImagesSQL, gameId);
        var uploader = new NextcloudUploader("https://cloud.m455yn.dev/", "sgsclient", "yGnxE-Tykxe-SwjwW-NooLc-xSwPT");

        foreach (var image in GameImages)
        {
            string imageUrl = image.Url;

            // Check if the image is a local file
            if (Path.IsPathRooted(imageUrl))
            {
                // It's a local file, so upload it to Nextcloud
                var file = await StorageFile.GetFileFromPathAsync(imageUrl);
                string uploadedUrl = await uploader.UploadFileAsync(file.Path); // Upload file and get URL

                if (uploadedUrl != null)
                {
                    imageUrl = uploadedUrl; // Update URL with the Nextcloud URL
                }
            }

            // Insert the URL (which may now be from Nextcloud) into the database
            db.con.select(SqlQueries.insertImageSQL, gameId, imageUrl);
        }
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
        string imageUrl = imageRow["LogoPath"].ToString();

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
        string imageUrl = imageRow["ImagePath"].ToString();

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
