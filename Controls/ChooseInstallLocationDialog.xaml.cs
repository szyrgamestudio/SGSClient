using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using SGSClient.Helpers; // Dodaj na górze!
namespace SGSClient.Controls
{
    public sealed partial class ChooseInstallLocationDialog : ContentDialog
    {
        public string SelectedPath { get; private set; } = string.Empty;
        public ChooseInstallLocationDialog()
        {
            InitializeComponent();
            var localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SGSClient", "Gry");
            PathTextBox.Text = localAppDataPath;
        }
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
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
        private void PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidatePath();
        }
        private void ValidatePath()
        {
            try
            {
                string path = PathTextBox.Text.Trim();
                if (string.IsNullOrEmpty(path))
                {
                    SetError(LocalizationHelper.GetString("Error_EmptyPath"));
                    return;
                }

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string testFilePath = Path.Combine(path, "sgs_testfile.tmp");
                File.WriteAllText(testFilePath, "test");
                File.Delete(testFilePath);

                ClearError();
            }
            catch (Exception)
            {
                SetError(LocalizationHelper.GetString("Error_InvalidOrUnauthorized"));
            }
        }
        private void SetError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
            IsPrimaryButtonEnabled = false;
        }
        private void ClearError()
        {
            ErrorTextBlock.Text = "";
            ErrorTextBlock.Visibility = Visibility.Collapsed;
            IsPrimaryButtonEnabled = true;
        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SelectedPath = PathTextBox.Text;
        }
    }
}
