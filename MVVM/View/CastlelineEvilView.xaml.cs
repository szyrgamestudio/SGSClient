using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SGSClient.MVVM.View
{
    /// <summary>
    /// Logika interakcji dla klasy CastlelineEvilView.xaml
    /// </summary>
    public partial class CastlelineEvilView : UserControl
    {
        public CastlelineEvilView()
        {
            InitializeComponent();
        }

        private void updateClickButton(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/DEVMASSYN/Castleline-Evil/releases",
                UseShellExecute = true
            });
        }

    }

}
