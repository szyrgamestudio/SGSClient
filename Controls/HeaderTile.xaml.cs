using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SGSClient.Controls
{
    public sealed partial class HeaderTile : UserControl
    {
        Compositor _compositor = Microsoft.UI.Xaml.Media.CompositionTarget.GetCompositorForCurrentThread();
        private SpringVector3NaturalMotionAnimation? _springAnimation;

        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(HeaderTile), new PropertyMetadata(null));

        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }
            set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(HeaderTile), new PropertyMetadata(null));

        public object Source
        {
            get
            {
                return (object)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(HeaderTile), new PropertyMetadata(null));

        public string Link
        {
            get
            {
                return (string)GetValue(LinkProperty);
            }
            set
            {
                SetValue(LinkProperty, value);
            }
        }

        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.Register("Link", typeof(string), typeof(HeaderTile), new PropertyMetadata(null));

        public HeaderTile()
        {
            this.InitializeComponent();
        }
        private void Element_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            CreateOrUpdateSpringAnimation(1.1f);
            ((UIElement)sender).CenterPoint = new Vector3(70, 40, 1f);
            ((UIElement)sender).StartAnimation(_springAnimation);
        }

        private void Element_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            CreateOrUpdateSpringAnimation(1.0f);
            ((UIElement)sender).CenterPoint = new Vector3(70, 40, 1f);
            ((UIElement)sender).StartAnimation(_springAnimation);
        }

        private void CreateOrUpdateSpringAnimation(float finalValue)
        {
            if (_springAnimation == null)
            {
                if (_compositor != null)
                {
                    _springAnimation = _compositor.CreateSpringVector3Animation();
                    _springAnimation.Target = "Scale";
                }
            }

            if (_springAnimation != null) // Check if _springAnimation is not null
            {
                _springAnimation.FinalValue = new Vector3(finalValue);
            }
        }
    }
}

