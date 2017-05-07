using CommandClient;
using Gold_Client.Model;
using Gold_Client.View;
using System;
using System.Security;
using System.Threading;
using System.Windows;

namespace Gold_Client.ViewModel
{
    class ClientLogin : IClient
    {

        //SpeechLib.SpVoice voice = new SpeechLib.SpVoice();

        bool loginNotyfi;

        ClientReceivedFromServer clientReceive = ClientReceivedFromServer.Instance;

        public Client User
        {
            get; set;
        }

        public ClientLogin(Client client)
        {
            User = client;

            loginNotyfi = clientReceive.config.loginEmailNotyfication; //load config value

            //clientReceive.ClientLogin += OnClientLogin;
            clientReceive.ClientSuccesLogin += OnClientSuccesLogin;
            clientReceive.ClientReSendEmail += OnClientReSendEmail;
            clientReceive.ReceiveLogExcep += (s, e) => MessageBox.Show(e.receiveLogExpceMessage, "Login except Information", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnClientReSendEmail(object sender, ClientEventArgs e)
        {
            if (e.clientReSendEmailMessage == "You must activate your account first.")
            {
                MessageBoxResult result = MessageBox.Show(e.clientReSendEmailMessage, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Question);
                //register_code regCodeWindows;
                if (result == MessageBoxResult.OK)
                {
                    //regCodeWindows = new register_code(clientManager/*, Client.strName*/);
                    //regCodeWindows.Show();
                }
            }
            else
            {
                MessageBox.Show(e.clientReSendEmailMessage, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnClientLogin(object sender, ClientEventArgs e)
        {
            if (e.clientLoginName == User.strName && e.clientLoginMessage != "<<<" + e.clientLoginName + " has joined the room>>>")//i dont want to see msgBox when other users log in
            {
                MessageBox.Show(e.clientLoginMessage, "Login Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void SendLoginAndEncryptPass(string UserName, SecureString password)
        {
            try
            {
                App.Client.strName = UserName;
                ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
                clientSendToServer.SendToServer(Command.Login, clientSendToServer.CalculateChecksum(new System.Net.NetworkCredential(string.Empty, password).Password),
                    (loginNotyfi ? "1" : null));
                clientReceive.BeginReceive();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Unexpected error!{0}{1}", Environment.NewLine, ex.Message), "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /*
        private void OnReceiveLogExcep(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.receiveLogExpceMessage, "Login Information", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnClientLogin(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientLoginMessage, "Login Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }*/

        private void OnClientSuccesLogin(object sender, ClientEventArgs e)
        {
            //var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
            //anim.Completed += (s, _) => DialogResult = true;
            //BeginAnimation(OpacityProperty, anim);
            //clientSocket = e.clientSocket;

            Thread newWindowThread = new Thread(new ThreadStart(() =>
            {
                MainProgramWindow mainWindow = new MainProgramWindow();
                mainWindow.Show();
                System.Windows.Threading.Dispatcher.Run();
            }));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
        }

        //private void closeButton_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBoxResult result = MessageBox.Show("Do you want to close Aplication?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
        //    if (result == MessageBoxResult.Yes)
        //    {
        //        var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
        //        anim.Completed += (s, _) => DialogResult = false;
        //        BeginAnimation(OpacityProperty, anim);
        //    }
        //}
    }
}
