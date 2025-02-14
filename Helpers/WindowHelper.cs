using Microsoft.UI.Xaml;
using WinRT.Interop; // Namespace for WindowNative

namespace SGSClient.Helpers
{
    public static class WindowHelper
    {
        public static void InitializeWithWindow<T>(T dialog, Window window) where T : class
        {
            IntPtr hwnd = WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(dialog, hwnd);
        }
    }
}
