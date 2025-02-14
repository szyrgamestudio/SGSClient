using System.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

public class GameImage : INotifyPropertyChanged
{
    private string _url;
    private BitmapImage _imageSource;

    public string Url
    {
        get => _url;
        set
        {
            if (_url != value)
            {
                _url = value;
                OnPropertyChanged(nameof(Url));
                UpdateImageSource();
            }
        }
    }

    public BitmapImage ImageSource
    {
        get => _imageSource;
        private set
        {
            if (_imageSource != value)
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }
    }

    public GameImage(string url)
    {
        Url = url;
        UpdateImageSource();
    }

    public GameImage(BitmapImage imageSource)
    {
        ImageSource = imageSource;
    }

    public GameImage(string url, BitmapImage imageSource)
    {
        Url = url;
        UpdateImageSource();
        ImageSource = imageSource;
    }

    private void UpdateImageSource()
    {
        if (!string.IsNullOrEmpty(Url))
        {
            // Sprawdź, czy ścieżka to ścieżka lokalna
            if (Uri.IsWellFormedUriString(Url, UriKind.Absolute) && Url.StartsWith("file://"))
            {
                ImageSource = new BitmapImage(new Uri(Url));
            }
            else if (Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            {
                ImageSource = new BitmapImage(new Uri(Url));
            }
            else
            {
                // Obsługa ścieżek lokalnych jako 'file://'
                ImageSource = new BitmapImage(new Uri("file://" + Url));
            }
        }
        else
        {
            ImageSource = null; // Clear if the URL is empty
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
