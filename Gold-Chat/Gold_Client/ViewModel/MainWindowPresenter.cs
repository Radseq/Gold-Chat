using Gold_Client.Model;
using Gold_Client.ViewModel.Others;
using System;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    public class MainWindowPresenter : ObservableObject
    {
        ProcessReceivedByte getMessageFromServer = ProcessReceivedByte.Instance;

        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private bool isUserLogged = false;

        public MainWindowPresenter()
        {
            getMessageFromServer.ClientSuccesLogin += OnClientSuccesLogin;

            LoginCommand = new DelegateCommand(OpenLogin);
            MainWindowCommand = new DelegateCommand(OpenMainWindow);

            dispatcherTimer.Tick += new EventHandler(SendToServerAsk);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            //dispatcherTimer.Start();

            LoginCommand.Execute(null); // Firt navigate to login window
        }

        // OnClientSuccesLogin running on x-th thread but, main window need to be running on main thread
        // so i create timer on main thread that stop when user log
        private void SendToServerAsk(object sender, EventArgs e)
        {
            if (isUserLogged == true)
            {
                dispatcherTimer.Stop();
                MainWindowCommand.Execute(null);
            }

        }

        private void OnClientSuccesLogin(object sender, ClientEventArgs e)
        {
            //var uiContext = SynchronizationContext.Current;
            //uiContext.Send(x => MainWindowCommand.Execute(null), null); // Now we navigate to main program
            //Thread newWindowThread = new Thread(new ThreadStart(() =>
            //{
            //    MainProgramWindow mainWindow = new MainProgramWindow();
            //    mainWindow.Show();
            //    System.Windows.Threading.Dispatcher.Run();
            //}));
            //newWindowThread.SetApartmentState(ApartmentState.STA);
            //newWindowThread.IsBackground = true;
            //newWindowThread.Start();
            MainWindowCommand.Execute(null);
            isUserLogged = true;
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
            SelectedViewModel = new MainContentPresenter();
        }
    }
}
