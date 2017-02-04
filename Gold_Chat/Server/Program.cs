﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class server
    {
        public Socket ServerSocket { get; set; }

        private bool runServer = true;

        byte[] byteData = new byte[1024];

        public const int max_users = 50;

        //Client client;
        static StreamWriter strWriter = new StreamWriter("serverLogger.txt", true);
        static ServerLogger servLogg = new ServerLogger(ref strWriter);
        ServerManager sm = new ServerManager(servLogg);

        // ServerManager sm;

        public server()
        {
            try
            {
                //sm = new ServerManager(servLogg);
                ServerSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                ServerSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
                ServerSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, 5000));
                ServerSocket.Listen(5);

                Console.WriteLine(" >> Server Started");
                servLogg.msgLog(" >> Server Started"); /*on Adress:" + ((IPEndPoint)ServerSocket.RemoteEndPoint).Address.ToString() + " Port:" + ((IPEndPoint)ServerSocket.RemoteEndPoint).Port.ToString());*/

                //The collection of all clients logged into the room (an array of type ClientInfo)
                //ArrayList clientList = new ArrayList();


                sm.ClientLogin += OnClientLogin;
                sm.ClientLogin += servLogg.OnClientLoginLogger;
                sm.ClientRegistration += OnClientRegistration;
                sm.ClientReSendAckCode += OnClientReSendAckCode;
                sm.ClientLogout += OnClientLogout;
                sm.ClientLogout += servLogg.OnClientLogoutLogger;
                sm.ClientMessage += OnClientMessage;
                sm.ClientMessage += servLogg.OnClientMessageLogger;
                sm.ClientSendMessage += OnClientSendMessage;
                sm.ClientList += OnClientList;
                sm.ClientSendMessage += servLogg.OnClientSendMessageLogger;
                sm.ClientReceiMessage += OnClientReceiMessage;
                sm.ClientReceiMessage += servLogg.OnClientReceiMessageLogger;

                while (runServer)
                {
                    sm.getConnection(ServerSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                servLogg.RunServerLogger(ex);
            }
        }

        public void Stop()
        {
            runServer = false;
            ServerSocket.Close();
            /*while (clientList.Count > 0)
            {
                clientList[0].KeepProcessing = false;
                clientList[0].cSocket.Close();
                clientList.RemoveAt(0);
            }*/
            strWriter.Close();
            strWriter.Dispose();
        }

        #region event messages
        private void OnClientReceiMessage(object sender, ClientEventArgs e)
        {
            Console.WriteLine("ReceivedMessage " + e.clientMessageReciv);
        }

        private void OnClientSendMessage(object sender, ClientEventArgs e)
        {
            Console.WriteLine("SendMessage " + e.clientMessageToSend);
        }

        private void OnClientList(object sender, ClientEventArgs e)
        {
            Console.WriteLine(e.clientMessageToSend);
        }

        private void OnClientMessage(object sender, ClientEventArgs e)
        {
            Console.WriteLine("clientMessage " + e.clientMessageReciv);
        }

        private void OnClientLogout(object sender, ClientEventArgs e)
        {
            Console.WriteLine(e.clientName + " has left the room>>>");
        }

        private void OnClientLogin(object sender, ClientEventArgs e)
        {
            Console.WriteLine(e.clientName + " has joined the room>>>");
        }

        private void OnClientReSendAckCode(object sender, ClientEventArgs e)
        {
            Console.WriteLine(e.clientName + " resend thier activation code to " + e.clientEmail);
        }

        private void OnClientRegistration(object sender, ClientEventArgs e)
        {
            Console.WriteLine(e.clientName + " has registered by " + e.clientEmail);
        }
        #endregion
    }

    class Program
    {
        static void Main(string[] args)
        {
            //todo when server start check for there is mysql connection to server
            //and port is in use

            server serv = new server();

            Console.ReadLine();
            serv.Stop();
        }
    }
}