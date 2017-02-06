﻿using CommandClient;
using System;
using System.Windows;

namespace Gold
{
    /// <summary>
    /// Interaction logic for register_code.xaml
    /// </summary>
    public partial class register_code : Window
    {

        ClientManager clientManager;
        private string userName;

        public register_code(ClientManager cm, string userNameP_log)
        {
            InitializeComponent();
            clientManager = cm;
            userName = userNameP_log;
            clientManager.ClientReSendEmail += OnClientReSendEmail;
        }

        private void OnClientReSendEmail(object sender, ClientEventArgs e)
        {
            if (e.clientReSendEmailMessage == "Now you can login in to application")
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    MessageBoxResult result = MessageBox.Show(e.clientReSendEmailMessage, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (result == MessageBoxResult.OK)
                    {
                        Close();
                    }
                }));
            }
        }

        private void endReg(object sender, RoutedEventArgs e)
        {
            Data msgToSend = new Data();

            msgToSend.strName = userName;
            msgToSend.strMessage = registerCode.Text;
            msgToSend.cmdCommand = Command.ReSendEmail;

            byte[] byteData = msgToSend.ToByte();

            clientManager.BeginSend(byteData);

            //Send it to the server
            //pLogin.clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(pLogin.OnSend), null);
        }

        private void resendCode(object sender, RoutedEventArgs e)
        {
            Data msgToSend = new Data();

            msgToSend.strName = userName;
            msgToSend.strMessage = "";
            msgToSend.cmdCommand = Command.ReSendEmail;

            byte[] byteData = msgToSend.ToByte();
            clientManager.BeginSend(byteData);
            //Send it to the server
            // pLogin.clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(pLogin.OnSend), null);
        }
    }
}