using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace SGSClient.Controls
{
    public sealed partial class ChooseInstallLocationDialog : ContentDialog
    {
        public StorageFolder SelectedFolder { get; private set; }

        public ChooseInstallLocationDialog()
        {
            this.InitializeComponent();
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
                SelectedFolder = folder;

                string token = "GameInstallFolder";
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, folder);

                PathTextBox.Text = folder.Path;
            }
        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (SelectedFolder == null)
            {
                args.Cancel = true;
                SetError("Musisz wybraæ folder instalacji.");
            }
        }
        private void PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PathTextBox != null)
            {
                string path = PathTextBox.Text.Trim();

                if (string.IsNullOrEmpty(path))
                {
                    SetError("Œcie¿ka nie mo¿e byæ pusta.");
                }
                else
                {
                    ClearError();
                }
            }
        }

        private void SetError(string msg)
        {
            ToolTipService.SetToolTip(PathTextBox, msg);
        }

        private void ClearError()
        {
            ToolTipService.SetToolTip(PathTextBox, null);
        }
    }
}
