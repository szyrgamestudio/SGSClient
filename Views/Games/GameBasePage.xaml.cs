using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.Controls;
using SGSClient.Core.Extensions;
using SGSClient.Helpers;
using SGSClient.Models;
using SGSClient.ViewModels;
using System.Data;
using System.Diagnostics;
using Windows.Storage;

namespace SGSClient.Views;
public sealed partial class GameBasePage : Page
{
    private LauncherStatus _status;
    private GameRating? _gameRating;
    private readonly string? gameZip = "";

    public GameBaseViewModel ViewModel { get; }

    internal LauncherStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            LauncherStatusHelper.UpdateStatus(PlayButton, CheckUpdateButton, UninstallButton, _status, gameZip ?? "");
        }
    }

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = ViewModel;

        if (e.Parameter is string gameSymbol && !string.IsNullOrWhiteSpace(gameSymbol))
        {
            (bool installedP, bool updateP) = ViewModel.CheckForUpdate(gameSymbol);

            if (installedP)
                Status = LauncherStatus.ready;
            else
                Status = LauncherStatus.readyNoGame;

            CheckUpdateButton.Visibility = updateP? Visibility.Visible : Visibility.Collapsed;

            await ViewModel.LoadGameData(gameSymbol);
            GameDescriptionSmallRichText.SetHtml(ViewModel.GameDescription ?? string.Empty);
            GameDescriptionRichText.SetHtml(ViewModel.GameDescription ?? string.Empty);
            HardwareRichText.SetHtml(ViewModel.HardwareRequirements ?? string.Empty);
        }
    }

    public GameBasePage()
    {
        ViewModel = App.GetService<GameBaseViewModel>();
        InitializeComponent();
    }

    #region Rating
    private async void AddRatingButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        bool hasUserRated = ViewModel.UserRatingP();
        string title = string.Empty;
        int rating = 5;
        string review = string.Empty;

        if (hasUserRated && !string.IsNullOrEmpty(ViewModel.GameId.ToString()))
        {
            DataSet ds = ViewModel.ReturnUserRating(ViewModel.GameId ?? 0);
            if (ds.Tables[0].Rows.Count > 0)
            {
                var dr = ds.Tables[0].Rows[0];
                title = dr.TryGetValue("Title");
                rating = dr.TryGetValue("Rating");
                review = dr.TryGetValue("Review");
                _gameRating = new GameRating { RatingId = dr.TryGetValue("Id") };
            }
        }
        else
        {
            _gameRating = null;
        }

        // Tworzymy kontrolki dynamicznie
        TextBox titleTextBox = new TextBox { Text = title, PlaceholderText = "Tytuł oceny..." };
        RatingControl ratingControl = new RatingControl { Value = rating, MaxRating = 5 };
        TextBox reviewTextBox = new TextBox { Text = review, PlaceholderText = "Opinia...", AcceptsReturn = true, Height = 100 };

        StackPanel panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(titleTextBox);
        panel.Children.Add(ratingControl);
        panel.Children.Add(reviewTextBox);

        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "Oceń grę",
            PrimaryButtonText = "Zapisz",
            CloseButtonText = "Anuluj",
            DefaultButton = ContentDialogButton.Primary,
            Content = panel
        };

        dialog.PrimaryButtonClick += (s, eArgs) =>
        {
            if (string.IsNullOrWhiteSpace(titleTextBox.Text))
            {
                eArgs.Cancel = true;
                ToolTipService.SetToolTip(titleTextBox, "Tytuł oceny nie może być pusty.");
            }
            else
            {
                // Zapisujemy dane
                _gameRating ??= new GameRating();
                _gameRating.Title = titleTextBox.Text;
                _gameRating.Rating = (int)ratingControl.Value;
                _gameRating.Review = reviewTextBox.Text;
                ViewModel.SaveGameRating(ViewModel.GameId ?? 0, _gameRating);
            }
        };

        await dialog.ShowAsync();
    }



    /*
    private void RatingRatingControl_ValueChanged(RatingControl sender, object args)
    {
        ArgumentNullException.ThrowIfNull(sender);
    }
    private void AddRatingButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (!String.IsNullOrEmpty(gameIdentifier))
        {
            GameRating gameRating = new();
            _gameRating = gameRating;
            var ds = ViewModel.ReturnUserRating(gameIdentifier);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    _gameRating.RatingId = dr.Field<int>("Id");
                }
            }
            _gameRating.Title = RatingTitleTextBox.Text;
            _gameRating.Rating = (int)RatingRatingControl.Value;
            _gameRating.Review = RatingReviewTextBox.Text;
            ViewModel.AddRating(gameIdentifier, _gameRating);
            ViewModel.LoadRatings(gameIdentifier);

            AddRatingDetailsDialog.Hide();

        }
    }
    */
    private void ShowAllReviewsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }
    #endregion

    #region Buttons
    private async void PlayButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (App.MainWindow.Content is not ShellPage shellPage)
        {
            Debug.WriteLine("Nie udało się uzyskać ShellPage.");
            return;
        }

        switch (Status)
        {
            case LauncherStatus.readyNoGame:
                var folder = await ShowInstallLocationDialogAsync(ViewModel.GameIdentifier);
                if (folder != null)
                {
                    await ViewModel.DownloadGameAsync(shellPage);
                }
                break;

            case LauncherStatus.ready:
                ViewModel.PlayGame();
                break;

            default:
                Debug.WriteLine($"Nieobsługiwany status: {Status}");
                break;
        }
    }

    private async Task<StorageFolder> ShowInstallLocationDialogAsync(string gameIdentifier)
    {
        TextBox pathTextBox = new TextBox { PlaceholderText = "Wybierz folder instalacji..." };
        Button browseButton = new Button { Content = "Przeglądaj" };
        StackPanel panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(pathTextBox);
        panel.Children.Add(browseButton);

        StorageFolder selectedFolder = default!;

        browseButton.Click += async (s, e) =>
        {
            var picker = new Windows.Storage.Pickers.FolderPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add("*");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                selectedFolder = folder;
                pathTextBox.Text = folder.Path;

                string token = $"GameInstallFolder_{gameIdentifier}";
                Windows.Storage.AccessCache.StorageApplicationPermissions
                    .FutureAccessList
                    .AddOrReplace(token, folder);
            }
        };

        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Title = "Wybierz folder instalacji",
            PrimaryButtonText = "OK",
            CloseButtonText = "Anuluj",
            DefaultButton = ContentDialogButton.Primary,
            Content = panel
        };

        dialog.PrimaryButtonClick += (s, e) =>
        {
            if (selectedFolder == null)
            {
                e.Cancel = true;
                ToolTipService.SetToolTip(pathTextBox, "Musisz wybrać folder instalacji.");
            }
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary || selectedFolder is null)
            throw new InvalidOperationException("Nie wybrano folderu.");

        return selectedFolder;
    }

    private async void UpdateButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (App.MainWindow.Content is not ShellPage shellPage)
        {
            Debug.WriteLine("Nie udało się uzyskać ShellPage.");
            return;
        }

        await ViewModel.DownloadGameAsync(shellPage);
    }
    private void UninstallButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.UninstallGame();
    }
    #endregion
}