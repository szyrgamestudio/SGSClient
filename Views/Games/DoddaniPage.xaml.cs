using Microsoft.UI.Xaml.Controls;

using SGSClient.ViewModels;

namespace SGSClient.Views;

public sealed partial class DoddaniPage : Page
{
    public DoddaniViewModel ViewModel
    {
        get;
    }

    public DoddaniPage()
    {
        ViewModel = App.GetService<DoddaniViewModel>();
        InitializeComponent();
    }
}
