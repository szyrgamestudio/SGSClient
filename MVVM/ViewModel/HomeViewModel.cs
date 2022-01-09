using SGSClient.Core;
using System;

namespace SGSClient.MVVM.ViewModel
{
    class HomeViewModel : ObservableObject
    {
        //GRY SGS
        public static RelayCommand AtorthViewCommand { get; set; }
        public static RelayCommand DoddaniViewCommand { get; set; }

        //POZOSTAŁE GRY
        public static RelayCommand CastlelineEvilViewCommand { get; set; }
        public static RelayCommand SciezkaBohateraViewCommand { get; set; }
        public static RelayCommand ZacmienieViewCommand { get; set; }

        object CurrentView; //IMPORT FROM MainViewModel.cs
        public HomeViewModel()
        {
            AtorthViewCommand = new RelayCommand(o =>
            {
                MainViewModel.AtorthViewCommand.Execute(CurrentView);
            });

            DoddaniViewCommand = new RelayCommand(o =>
            {
                MainViewModel.DoddaniViewCommand.Execute(CurrentView);
            });

            CastlelineEvilViewCommand = new RelayCommand(o =>
            {
                MainViewModel.CastlelineEvilViewCommand.Execute(CurrentView);
            });

            SciezkaBohateraViewCommand = new RelayCommand(o =>
            {
                MainViewModel.SciezkaBohateraViewCommand.Execute(CurrentView);
            });

            ZacmienieViewCommand = new RelayCommand(o =>
            {
                MainViewModel.ZacmienieViewCommand.Execute(CurrentView);
            });
        }
    }
}
