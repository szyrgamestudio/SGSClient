using SGSClient.Models;
using SGSClient.Views;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace SGSClient.Services;
public class DownloadManager
{
    #region Singleton
    private static readonly Lazy<DownloadManager> instance = new(() => new DownloadManager());
    public static DownloadManager Instance => instance.Value;
    #endregion

    #region Properties
    public ObservableCollection<DownloadItem> ActiveDownloads { get; } = new();
    #endregion

    #region Constructor
    private DownloadManager() { }
    #endregion

    #region Methods
    public static Task StartDownloadAsync(ShellPage shellPage, string gameName, string url, StorageFolder destinationPath, string gameLogo)
    {
        return shellPage?.AddDownload(gameName, url, destinationPath, gameLogo) ?? Task.CompletedTask;
    }
    #endregion
}
