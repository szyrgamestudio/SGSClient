using Microsoft.Windows.ApplicationModel.Resources;


namespace SGSClient.Helpers
{
    public static class LocalizationHelper
    {
        private static readonly ResourceLoader _loader = new();

        public static string GetString(string key)
        {
            return _loader.GetString(key);
        }
    }
}
