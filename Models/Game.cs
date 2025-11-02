using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace SGSClient.Models;
public partial class Game : ObservableObject
{
    public int GameId { get; set; }
    public string? GameSymbol { get; set; }
    public string? GameName { get; set; }
    public string? GameDeveloper { get; set; }
    public string? GameTitle { get; set; }
    public Uri? ImageSource { get; set; }
    public string? GameVersion { get; set; }
    public string? GamePayloadName { get; set; }
    public string? GameExeName { get; set; }
    public string? GameZipLink { get; set; }
    public string? GameVersionLink { get; set; }
    public string? GameDescription { get; set; }
    public string? HardwareRequirements { get; set; }
    public string? OtherInformations { get; set; }

    // oryginalny URL do logo (np. z Nextcloud)
    public string? LogoPath { get; set; }

    public string? GameType { get; set; }
    public bool DraftP { get; set; }

    // nowa właściwość na załadowany obraz (dla XAML)
    [ObservableProperty]
    private BitmapImage? logoImage;
}
