using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.ComponentModel;

namespace SGSClient.Helpers;

public class LocalizedText : INotifyPropertyChanged
{
    private static readonly ResourceLoader loader = new();
    private static LocalizedText? instance;

    public static LocalizedText Instance => instance ??= new();

    private string languageOverride = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string this[string key]
    {
        get
        {
            try { return loader.GetString(key); }
            catch { return $"[{key}]"; }
        }
    }

    public void UpdateLanguage(string newLang)
    {
        if (languageOverride != newLang)
        {
            languageOverride = newLang;
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = newLang;
            OnPropertyChanged(string.Empty); // powiadom wszystkich bindingi
        }
    }

    private void OnPropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
