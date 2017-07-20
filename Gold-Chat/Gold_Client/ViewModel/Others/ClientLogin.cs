using CommandClient;
using Gold_Client.Model;
using Gold_Client.View.Others;
using Gold_Client.ViewModel.Others;
using System;
using System.Security;
using System.Windows;

namespace Gold_Client.ViewModel
{
    class ClientLogin : IClient, IDisposable
    {
        bool loginNotyfi = false;

        public Configuration config = new Configuration();

        //ClientReceivedFromServer clientReceive = ClientReceivedFromServer.Instance;
        ProcessReceivedByte getMessageFromServer = ProcessReceivedByte.Instance;

        public Client User
        {
            get; set;
        }

        public ClientLogin()
        {
            User = App.Client;

            loginNotyfi = config.loginEmailNotyfication; //load config value
            getMessageFromServer.ClientSendActivCodeFromEmail += OnSendActivateCodeFromEmail;
            //clientReceive.ReceiveLogExcep += OnReceiveLogExcep;
            getMessageFromServer.ClientLogin += OnClientLogin;
        }

        private void OnSendActivateCodeFromEmail(object sender, ClientEventArgs e)
        {
            if (e.clientSendActivCodeFromEmail == "You must activate your account first.")
            {
                MessageBoxResult result = MessageBox.Show(e.clientSendActivCodeFromEmail, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Question);
                if (result == MessageBoxResult.OK)
                {
                    ActivationUserWindow activateWindow = new ActivationUserWindow();
                    activateWindow.Show();
                }
            }
            else
            {
                MessageBox.Show(e.clientSendActivCodeFromEmail, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnClientLogin(object sender, ClientEventArgs e)
        {
            if (e.clientLoginName == User.strName && e.clientLoginMessage != "<<<" + e.clientLoginName + " has joined the room>>>") // i dont want to see msgBox when other users log in
                MessageBox.Show(e.clientLoginMessage, "Login Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void SendLoginAndEncryptPass(string UserName, SecureString password)
        {
            try
            {
                App.Client.strName = UserName;
                ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
                clientSendToServer.SendToServer(Command.Login, clientSendToServer.CalculateChecksum(new System.Net.NetworkCredential(string.Empty, password).Password),
                    (loginNotyfi ? "1" : null));
                if (!ReceivePackageFromServer.IsClientStartReceive)
                {
                    ReceivePackageFromServer.BeginReceive();
                    //clientReceive.BeginReceive();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Unexpected error!{0}{1}", Environment.NewLine, ex.Message), "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnReceiveLogExcep(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.receiveLogExpceMessage, "Login Information", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnClientSuccesLogin(object sender, ClientEventArgs e)
        {

            //Thread newWindowThread = new Thread(new ThreadStart(() =>
            //{
            //    MainProgramWindow mainWindow = new MainProgramWindow();
            //    mainWindow.Show();
            //    System.Windows.Threading.Dispatcher.Run();
            //}));
            //newWindowThread.SetApartmentState(ApartmentState.STA);
            //newWindowThread.IsBackground = true;
            //newWindowThread.Start();

            //var uiContext = SynchronizationContext.Current;
            //uiContext.Send(x => App.loginWindow.DialogResult = true, null);
            //App.loginWindow.DialogResult = true;
            //Application.Current.Dispatcher.Invoke(delegate
            // {
            //loginWindow.DialogResult = true;
            //});

        }

        public void Dispose()
        {
            config.SaveConfig(config);
        }
    }
}
