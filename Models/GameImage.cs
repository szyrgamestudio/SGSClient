using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel;

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
        UpdateImageSource(); // Initialize the BitmapImage based on the URL
    }

    private void UpdateImageSource()
    {
        if (!string.IsNullOrEmpty(Url))
        {
            ImageSource = new BitmapImage(new Uri(Url));
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
