using SGSClient.Core;
using System;

namespace SGSClient.MVVM.ViewModel
{
    class AtorthViewModel : ObservableObject
    {
        public static RelayCommand AtorthViewCommand { get; set; }

        object CurrentView;
        public AtorthViewModel()
        {
            AtorthViewCommand = new RelayCommand(o =>
            {
                MainViewModel.AtorthViewCommand.Execute(CurrentView);
            });
        }
    }
}
