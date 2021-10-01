using GameLauncher;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SGS_Client
{
    /// <summary>
    /// Logika interakcji dla klasy Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Hide();
            GameLauncher.Atorth Ath = new Atorth();
            Ath.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Doddani Dod = new Doddani();
            Dod.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.Hide();
            StaffOfHell SOH = new StaffOfHell();
            SOH.ShowDialog();
        }

        private void Button_Click_Wyjscie(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
};
