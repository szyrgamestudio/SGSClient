using SGSClient.Core;
using System;

namespace SGSClient.MVVM.ViewModel
{
    class DoddaniViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public HomeViewModel HomeVM { get; set; }

        object CurrentView;
        public DoddaniViewModel()
        {
            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVM;
            });

        }
    }
}
