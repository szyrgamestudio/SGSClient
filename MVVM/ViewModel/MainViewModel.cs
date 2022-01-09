using SGSClient.Core;
using System;

namespace SGSClient.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }

        public static RelayCommand AtorthViewCommand { get; set; }
        public static RelayCommand DoddaniViewCommand { get; set; }
        public static RelayCommand CastlelineEvilViewCommand { get; set; }
        public static RelayCommand SciezkaBohateraViewCommand { get; set; }
        public static RelayCommand ZacmienieViewCommand { get; set; }
        public RelayCommand ExitLauncherCommand { get; set; }


        public HomeViewModel HomeVM { get; set; }
        public AtorthViewModel AtorthVM { get; set; }
        public DoddaniViewModel DoddaniVM { get; set; }
        public CastlelineEvilViewModel CastlelineEvilVM { get; set; }
        public SciezkaBohateraViewModel SciezkaBohateraVM { get; set; }
        public ZacmienieViewModel ZacmienieVM { get; set; }


        public object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set 
            {
                _currentView = value; 
                OnPropertyChanged();
            }
        }


        public MainViewModel()
        {
            HomeVM = new HomeViewModel();

            //Zakładki poszczególnych gier
            AtorthVM = new AtorthViewModel();
            DoddaniVM = new DoddaniViewModel();
            CastlelineEvilVM = new CastlelineEvilViewModel();
            SciezkaBohateraVM = new SciezkaBohateraViewModel();
            ZacmienieVM = new ZacmienieViewModel();

            //początkowe okno
            CurrentView = HomeVM;

            //przyciski początkowego okna
            HomeViewCommand = new RelayCommand(o =>
            { 
                CurrentView = HomeVM;
            });

            AtorthViewCommand = new RelayCommand(o =>
            {
                CurrentView = AtorthVM;
            });

            DoddaniViewCommand = new RelayCommand(o =>
            {
                CurrentView = DoddaniVM;
            });

            CastlelineEvilViewCommand = new RelayCommand(o =>
            {
                CurrentView = CastlelineEvilVM;
            });

            SciezkaBohateraViewCommand = new RelayCommand(o =>
            {
                CurrentView = SciezkaBohateraVM;
            });

            ZacmienieViewCommand = new RelayCommand(o =>
            {
                CurrentView = ZacmienieVM;
            });

            ExitLauncherCommand = new RelayCommand(o =>
            {
                App.Current.Shutdown();
            });
        }
    }
}
