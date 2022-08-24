using System;
using System.Windows;
using Wpf.Ui.Common.Interfaces;
using SGSClient.Services.Contracts;
using Wpf.Ui.Mvvm.Contracts;

namespace SGSClient.Views.Pages
{
    /// <summary>
    /// Logika interakcji dla klasy Dashboard.xaml
    /// </summary>
    public partial class Dashboard : INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ITestWindowService _testWindowService;
        public Dashboard(INavigationService navigationService, ITestWindowService testWindowService)
        {
            _navigationService = navigationService;
            _testWindowService = testWindowService;

            InitializeComponent();
        }

        public void OnNavigatedTo()
        {
            System.Diagnostics.Debug.WriteLine($"INFO | {typeof(Dashboard)} navigated", "SGSClient");
        }

        public void OnNavigatedFrom()
        {
            System.Diagnostics.Debug.WriteLine($"INFO | {typeof(Dashboard)} navigated out", "SGSClient");
        }

        private void ButtonControls_OnClick(object sender, RoutedEventArgs e)
        {
            _navigationService.Navigate(typeof(Views.Pages.Dashboard));
        }
    }
}
