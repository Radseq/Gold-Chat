using CommandClient;
using System;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Gold
{
    //using Client;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class p_log : Window //typ
    {

        public string userName;
        public Socket clientSocket;
        //public string strName;
        public const int PORT = 5000;
        public const string SERVER = "::1";

        private byte[] byteData = new byte[1024];

        SpeechLib.SpVoice voice = new SpeechLib.SpVoice();

        bool loginNotyfi;

        ClientManager clientManager;

        public p_log(ClientManager cm)
        {
            InitializeComponent();
            loginTextBox.Focus();
            clientManager = cm;

            loginNotyfi = clientManager.config.loginEmailNotyfication;//load config value

            clientManager.ClientLogin += (s, e) => MessageBox.Show(e.clientLoginMessage, "Login Information", MessageBoxButton.OK, MessageBoxImage.Information); //OnClientLogin;
            clientManager.ClientSuccesLogin += OnClientSuccesLogin;
            clientManager.ClientReSendEmail += OnClientReSendEmail;
            clientManager.ReceiveLogExcep += (s, e) => MessageBox.Show(e.receiveLogExpceMessage, "Login except Information", MessageBoxButton.OK, MessageBoxImage.Error);
            clientManager.ClientPing += OnClientPing;
        }

        private void OnClientPing(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                /*
                if (e.clientPingTime > 0)
                {
                    loginButton.IsEnabled = true;
                    serverStatusLabel.Content = e.clientPingMessage;
                    serverStatusLabel.Foreground = Brushes.Green;
                }
                else
                {
                    loginButton.IsEnabled = false;
                    serverStatusLabel.Content = e.clientPingMessage;
                    serverStatusLabel.Foreground = Brushes.Red;
                }*/
            }));
        }

        private void OnClientReSendEmail(object sender, ClientEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (e.clientReSendEmailMessage == "You must activate your account first.")
                {
                    MessageBoxResult result = MessageBox.Show(e.clientReSendEmailMessage, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Question);
                    register_code regCodeWindows;
                    if (result == MessageBoxResult.OK)
                    {
                        regCodeWindows = new register_code(clientManager, userName);
                        regCodeWindows.Show();
                    }
                }
                else
                {
                    MessageBox.Show(e.clientReSendEmailMessage, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }));
        }

        //private void OnClientLogin(object sender, ClientEventArgs e)
        //{
        //    if (e.clientLoginMessage == "Wrong login or password")
        //    {

        //    }
        //}

        private void move(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation myDoubleAnimation1 = new DoubleAnimation();
            myDoubleAnimation1.From = 0.0;
            myDoubleAnimation1.To = 1.0;
            myDoubleAnimation1.Duration = new Duration(TimeSpan.FromSeconds(2));
            BeginAnimation(OpacityProperty, myDoubleAnimation1);
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (loginTextBox.Text != string.Empty && passwordBox.Password != string.Empty)
            {
                try
                {
                    userName = loginTextBox.Text;

                    //clientSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

                    //IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(SERVER), PORT);

                    //Connect to the server
                    //clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);

                    //byteData = new byte[1024];
                    //Start listening to the data asynchronously
                    //clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(/*clientManager.*/OnReceive), null);

                    //DataLogin msgToSend = new DataLogin();

                    //msgToSend.loginName = userName;
                    //msgToSend.passChecksum = CalculateChecksum(passwordBox.Password);
                    // msgToSend.cmdCommand = CommandLogin.Login;

                    //byte[] byteData = msgToSend.ToByte();

                    clientManager.userName = userName;
                    //clientManager.clientPassword = CalculateChecksum(passwordBox.Password);

                    clientManager.BeginConnect();

                    Data msgToSend = new Data();

                    msgToSend.strName = userName;
                    msgToSend.strMessage = clientManager.CalculateChecksum(passwordBox.Password);
                    if (loginNotyfi)
                        msgToSend.strMessage2 = "1";
                    msgToSend.cmdCommand = Command.Login;

                    byte[] byteData = msgToSend.ToByte();

                    //Send it to the server
                    clientManager.BeginSend(byteData);

                    //clientManager.BeginConnect(userName, CalculateChecksum(passwordBox.Password));
                    //cm.BeginSend(byteData);

                    //byteData = new byte[1024];

                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Unexpected error!{0}{1}", Environment.NewLine, ex.Message), "Error validation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("Fill in the fields", "Error validation", MessageBoxButton.OK, MessageBoxImage.Information);
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
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                anim.Completed += (s, _) => DialogResult = true;
                BeginAnimation(OpacityProperty, anim);
                clientSocket = e.clientSocket;
            }));
        }

        /*

                public void OnSend(IAsyncResult ar)
                {
                    try
                    {
                        clientSocket.EndSend(ar);
                    }
                    catch (ObjectDisposedException)
                    { }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Gold Chat: " + userName, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                private void OnConnect(IAsyncResult ar)
                {
                    try
                    {
                        //We are connected so we login into the server
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            clientSocket.EndConnect(ar);

                            DataLogin msgToSend = new DataLogin();

                            msgToSend.loginName = userName;
                            msgToSend.passChecksum = CalculateChecksum(passwordBox.Password);
                            msgToSend.cmdCommand = CommandLogin.Login;

                            byte[] byteData = msgToSend.ToByte();

                            //Send it to the server
                            clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

                            /*total wasted time here 5h, cuse 

                            byteData = new byte[1024];
                                //Start listening to the data asynchronously
                                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);

                            MUST BE AFTER ON CONNECT, OnReceive wont get full message
                            *//*
                        }));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                public void OnReceive(IAsyncResult ar)
                {
                    try
                    {
                        clientSocket.EndReceive(ar);
                        // clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
                        //Dispatcher.BeginInvoke((Action)(() =>
                        //{
                        DataLogin msgReceived = new DataLogin(byteData);
                        //Accordingly process the message received
                        switch (msgReceived.cmdCommand)
                        {
                            case CommandLogin.Login:
                                MessageBox.Show(msgReceived.strMessage, "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Information);
                                break;

                            case CommandLogin.Reg:
                                if (msgReceived.strMessage == "Your login exists, try other one" || msgReceived.strMessage == "Your email exists, try other one"
                                || msgReceived.strMessage == "You have already register, go to login windows and paste register key"
                                || msgReceived.strMessage == "You has been registered")
                                {
                                    //p_reg panelRegistration = new p_reg(this);
                                    //panelRegistration.ShowDialog();
                                    MessageBox.Show(msgReceived.strMessage, "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                break;

                            case CommandLogin.ReSendEmail:
                                if (msgReceived.strMessage == "Activation code not match." || msgReceived.strMessage == "You must activate an account.")
                                {
                                    register_code regCodeWindow = new register_code(this);
                                    regCodeWindow.ShowDialog();
                                    MessageBox.Show(msgReceived.strMessage, "Gold Chat", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                break;

                            case CommandLogin.Message:
                                break;

                        }

                        byteData = new byte[1024];
                        if (msgReceived.strMessage != "You are succesfully Log in" && msgReceived.loginName != userName)
                            clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
                        else
                        {
                            Dispatcher.BeginInvoke((Action)(() =>
                            {
                                var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                                anim.Completed += (s, _) => DialogResult = true;
                                BeginAnimation(OpacityProperty, anim);
                            }));
                        }
                        //                }));
                    }
                    catch (ObjectDisposedException)
                    { }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Gold Chat: " + userName, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                */
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to close Aplication?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                //oznaczenie ze ktos zamknal okno = chce zakonczyc aplikacje
                var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                anim.Completed += (s, _) => DialogResult = false;
                BeginAnimation(OpacityProperty, anim);
            }
        }

        private void minimalizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            p_reg reg = new p_reg(clientManager);
            reg.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            /*
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
            anim.Completed += (s, _) => DialogResult = true;
            BeginAnimation(OpacityProperty, anim);*/
        }
    }
}