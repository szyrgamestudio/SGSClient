using Microsoft.UI.Xaml.Controls;

using SGSClient.ViewModels;

namespace SGSClient.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel
    {
        get;
    }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();
    }
}
