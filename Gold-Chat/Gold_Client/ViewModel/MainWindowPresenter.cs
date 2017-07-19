using Gold_Client.Model;
using Gold_Client.ViewModel.Others;
using System;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    public class MainWindowPresenter : ObservableObject
    {
        ProcessReceivedByte getMessageFromServer = ProcessReceivedByte.Instance;
        public Action CloseAction { get; set; }

        public MainWindowPresenter()
        {
            getMessageFromServer.ClientSuccesLogin += OnClientSuccesLogin;

            LoginCommand = new DelegateCommand(OpenLogin);
            MainWindowCommand = new DelegateCommand(OpenMainWindow);

            LoginCommand.Execute(null); // Firt navigate to login window
        }

        private void OnClientSuccesLogin(object sender, ClientEventArgs e)
        {
            MainWindowCommand.Execute(null);
        }

        public ICommand LoginCommand { get; set; }
        public ICommand MainWindowCommand { get; set; }

        private object selectedViewModel;

        public object SelectedViewModel
        {
            get { return selectedViewModel; }
            set
            {
                selectedViewModel = value;
                RaisePropertyChangedEvent(nameof(SelectedViewModel));
            }
        }

        private void OpenLogin(object obj)
        {
            SelectedViewModel = new LoginPresenter();
        }
        private void OpenMainWindow(object obj)
        {
            SelectedViewModel = new MainContentPresenter(CloseAction);
        }
    }
}
