using Microsoft.UI.Xaml.Data;
using System;

namespace SGSClient.Helpers;
public class LConverterDynamic : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string key && !string.IsNullOrEmpty(key))
        {
            return LocalizedText.Instance[key];
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
