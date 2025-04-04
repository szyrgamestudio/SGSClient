using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SGSClient.Controls
{
    public sealed partial class ChooseInstallLocationDialog : ContentDialog
    {
        public string SelectedPath { get; private set; } = string.Empty;

        public ChooseInstallLocationDialog()
        {
            this.InitializeComponent();
            PathTextBox.Text = @"C:\Gry\";
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
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
                PathTextBox.Text = folder.Path;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SelectedPath = PathTextBox.Text;
        }
    }
}
