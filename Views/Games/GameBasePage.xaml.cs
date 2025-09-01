using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.Controls;
using SGSClient.Helpers;
using SGSClient.Models;
using SGSClient.ViewModels;
using System.Data;
using System.Diagnostics;

namespace SGSClient.Views;
public sealed partial class GameBasePage : Page
{
    private LauncherStatus _status;
    private readonly string? gameZip = "";
    private readonly string? gameIdentifier = "";

    private GameRating? _gameRating;
    public GameBaseViewModel ViewModel { get; }

    internal LauncherStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            LauncherStatusHelper.UpdateStatus(PlayButton, CheckUpdateButton, UninstallButton, DownloadProgressBorder, _status, gameZip ?? "");
        }
    }

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = ViewModel;

        if (e.Parameter is string gameSymbol && !string.IsNullOrWhiteSpace(gameSymbol))
        {
            await ViewModel.LoadGameData(gameSymbol);
            (bool installedP, bool updateP) = ViewModel.CheckForUpdate(gameSymbol); 

            if (installedP)
                Status = LauncherStatus.ready;
            else
                Status = LauncherStatus.readyNoGame;

            if (updateP)
                CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
    }

    public GameBasePage()
    {
        ViewModel = App.GetService<GameBaseViewModel>();
        InitializeComponent();
    }

    #region Rating
    /*
    private void RatingRatingControl_ValueChanged(RatingControl sender, object args)
    {
        ArgumentNullException.ThrowIfNull(sender);
    }
    private async void AddRatingButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        bool hasUserRated = ViewModel.UserRatingP();

        if (hasUserRated && !String.IsNullOrEmpty(gameIdentifier))
        {
            DataSet ds = ViewModel.ReturnUserRating(gameIdentifier);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    _gameRating = new GameRating
                    {
                        RatingId = dr.Field<int>("Id")
                    };
                    RatingTitleTextBox.Text = dr.Field<string>("Title");
                    RatingRatingControl.Value = dr.Field<int>("Rating");
                    RatingReviewTextBox.Text = dr.Field<string>("Review");
                    AddRatingDetailsDialog.Title = "Oceń";
                }
            }
            AddRatingDetailsDialog.Title = "Oceń";
            await AddRatingDetailsDialog.ShowAsync();
        }
        else
        {
            _gameRating = null;
            RatingTitleTextBox.Text = string.Empty;
            RatingRatingControl.Value = 5;
            RatingReviewTextBox.Text = string.Empty;
            AddRatingDetailsDialog.Title = "Oceń";
            await AddRatingDetailsDialog.ShowAsync();
        }
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
                var dialog = new ChooseInstallLocationDialog
                {
                    XamlRoot = XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary && dialog.SelectedFolder != null)
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
    private async void UpdateButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new ChooseInstallLocationDialog
        {
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary || dialog.SelectedFolder is null)
            return;

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