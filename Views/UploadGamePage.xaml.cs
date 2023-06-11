using Microsoft.UI.Xaml.Controls;

using SGSClient.ViewModels;

namespace SGSClient.Views;

// To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/.
public sealed partial class UploadGamePage : Page
{
    public UploadGameViewModel ViewModel
    {
        get;
    }

    public UploadGamePage()
    {
        ViewModel = App.GetService<UploadGameViewModel>();
        InitializeComponent();

        ViewModel.WebViewService.Initialize(WebView);
    }
}
