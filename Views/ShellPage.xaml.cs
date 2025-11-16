using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SevenZipExtractor;
using SGSClient.Contracts.Services;
using SGSClient.Core.Utilities.LogUtility;
using SGSClient.Helpers;
using SGSClient.Models;
using SGSClient.ViewModels;
using Windows.Storage;
using Windows.System;
using SGSClient.Helpers;

namespace SGSClient.Views;

// TODO: Update NavigationViewItem titles and icons in ShellPage.xaml.
public sealed partial class ShellPage : Page
{
    public DownloadViewModel DownloadViewModel { get; } = new();
    public ShellViewModel ViewModel
    {
        get;
    }

    #region Ctor
    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
        this.PointerPressed += ShellPage_PointerPressed;
        DownloadBar.DataContext = DownloadViewModel.Instance;
    }
    #endregion
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }
    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        App.AppTitlebar = AppTitleBarText as UIElement;
    }
    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }
    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }
    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }
    private void ShellPage_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;

        if (properties.IsXButton1Pressed)
        {
            ViewModel.NavigationService.GoBack();
        }
        else if (properties.IsXButton2Pressed)
        {
            if (ViewModel.NavigationService.CanGoForward)
            {
                ViewModel.NavigationService.GoForward();
            }
        }
    }
    private void UserAccountItem_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var flyout = new MenuFlyout();

        if (ViewModel.IsUserLoggedIn)
        {
            flyout.Items.Add(new MenuFlyoutItem { Text = ViewModel.UserMenuText, IsEnabled = false });
            flyout.Items.Add(new MenuFlyoutSeparator());
            flyout.Items.Add(new MenuFlyoutItem { Text = L.p("Add game"), Command = ViewModel.AddGameCommand });
            flyout.Items.Add(new MenuFlyoutItem { Text = L.p("My profile"), Command = ViewModel.MyGamesCommand });
        }
        else
        {
            flyout.Items.Add(new MenuFlyoutItem { Text = L.p("Log in"), Command = ViewModel.LoginCommand });
        }

        flyout.ShowAt((FrameworkElement)sender);
    }
    public async Task AddDownload(string gameName, string gameIdentifier, string url, StorageFolder folder, string gameLogo)
    {
        if (folder == null)
        {
            throw new ArgumentNullException(nameof(folder), "Destination folder cannot be null.");
        }

        var downloadItem = new DownloadItem(gameName, gameIdentifier, url, folder, gameLogo);
        DownloadViewModel.Instance.ActiveDownloads.Add(downloadItem);

        DownloadBar.Visibility = Visibility.Visible;

        using var httpClient = new HttpClient();
        await downloadItem.StartDownloadAsync(httpClient);
        await ExtractAndCleanup(downloadItem);
    }
    private async Task ExtractAndCleanup(DownloadItem item)
    {
        try
        {
            string fileName = item.GameName.Replace(" ", "");
            fileName = string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));

            await Task.Run(async () =>
            {
                StorageFile zipFile = await item.DestinationFolder.GetFileAsync($"{fileName}.zip");
                StorageFolder extractFolder = await item.DestinationFolder.CreateFolderAsync(fileName, CreationCollisionOption.ReplaceExisting);

                using (var archive = new ArchiveFile(zipFile.Path))
                {
                    archive.Extract(extractFolder.Path);
                }

                await zipFile.DeleteAsync();
            });

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                DownloadViewModel.Instance.ActiveDownloads.Remove(item);
                if (DownloadViewModel.Instance.ActiveDownloads.Count == 0)
                    DownloadBar.Visibility = Visibility.Collapsed;
            });
        }
        catch (Exception ex)
        {
            await Log.ErrorAsync("Exception (ExtractAndCleanup)", ex);
        }
    }
    public void RemoveDownload(string gameName)
    {
        var itemToRemove = DownloadViewModel.Instance.ActiveDownloads
            .FirstOrDefault(d => d.GameName == gameName);

        if (itemToRemove != null)
        {
            DownloadViewModel.Instance.ActiveDownloads.Remove(itemToRemove);
        }

        if (DownloadViewModel.Instance.ActiveDownloads.Count == 0)
        {
            DownloadBar.Visibility = Visibility.Collapsed;
        }
    }
}
