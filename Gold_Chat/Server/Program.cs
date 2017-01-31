using CommandClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
        ServerLogger servLogg = new ServerLogger(ref strWriter);

        public List<Client> clientList = new List<Client>();
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

                while (runServer)
                {
                    Socket clientSocket = ServerSocket.Accept();
                    if (clientSocket != null)
                    {
                        clientSocket.Receive(byteData, byteData.Length, SocketFlags.None);
                        Data msgReceived = new Data(byteData);
                        ParameterizedThreadStart pts = new ParameterizedThreadStart(ProcessClientThreadProc);
                        Thread thr = new Thread(pts);
                        thr.IsBackground = true;
                        thr.Start(clientSocket); //start client processing thread
                        clientSocket = null;
                    }
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

        private void ProcessClientThreadProc(object obj)
        {
            //Client cl = (Client)obj;

            Socket clientSocket = (Socket)obj;

            string connIP = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
            string connPort = ((IPEndPoint)clientSocket.RemoteEndPoint).Port.ToString();
            string acceptConnectrion = " >> Accept connection from client: " + connIP + " on Port: " + connPort;// + " Users Connected: " + clientList.Count;
            Console.WriteLine(acceptConnectrion);
            servLogg.msgLog(acceptConnectrion);

            //to check 
            //string connIP = (string)obj;
            //string connPort = (string)obj;
            //Client client;

            ServerManager sm = new ServerManager(servLogg);

            Client clientInfo = new Client();
            clientInfo.cSocket = clientSocket;
            clientInfo.addr = (IPEndPoint)clientSocket.RemoteEndPoint;

            //bool IsLogin = false;

            //logRegClient(ref clientInfo, ref IsLogin, ref sm);

            if (/*IsLogin && */clientList.Count <= max_users)
            {
                clientList.Add(clientInfo); //When a user logs in to the server then we add her to our list of clients
                //byteData = new byte[1024];
                manageClient(ref clientInfo, ref sm);
            }
            else
            {
                //TODO send to client that there is no space in server
            }

        }

        private void manageClient(ref Client clientInfo, ref ServerManager sm)
        {
            bool KeepProcessing = true;

            while (KeepProcessing)
            {
                try
                {
                    //Transform the array of bytes received from the user into an
                    //intelligent form of object Data
                    Data msgReceived = new Data(byteData);

                    //We will send this object in response the users request
                    Data msgToSend = new Data();

                    //If the message is to login, logout, or simple text message
                    //then when send to others the type of the message remains the same
                    msgToSend.cmdCommand = msgReceived.cmdCommand;
                    msgToSend.strName = msgReceived.strName;
                    msgToSend.strMessage = msgReceived.strMessage;

                    switch (msgReceived.cmdCommand)
                    {
                        /* case Command.Login: //this is no needed for client message manager i will use this in logRegClient
                             Client clientInfo = new Client();
                             clientInfo.cSocket = clientSocket;
                             clientInfo.strName = msgReceived.strName;

                             sm.clientLogin(ref clientInfo, msgReceived, msgToSend);

                             clientList.Add(clientInfo); //When a user logs in to the server then we add her to our list of clients
                             sm.ClientLogin -= OnClientLogin;
                             sm.ClientLogin += OnClientLogin;
                             sm.ClientLogin -= servLogg.OnClientLoginLogger;
                             sm.ClientLogin += servLogg.OnClientLoginLogger;
                             break;*/

                        case Command.Login:

                            sm.clientLogin(ref clientInfo, msgReceived, ref msgToSend);

                            sm.ClientLogin -= OnClientLogin;
                            sm.ClientLogin += OnClientLogin;
                            sm.ClientLogin -= servLogg.OnClientLoginLogger;
                            sm.ClientLogin += servLogg.OnClientLoginLogger;
                            break;

                        case Command.Reg:
                            sm.clientRegistration(msgReceived, ref msgToSend);
                            sm.ClientRegistration += OnClientRegistration;
                            // todo add subscribers
                            break;

                        case Command.changePassword:
                            sm.chnageUserPassword(clientInfo, msgReceived, ref msgToSend);
                            break;

                        case Command.ReSendEmail: //Text of the message that we will broadcast to all users
                            sm.clientReSendActivCode(msgReceived, ref msgToSend);
                            sm.ClientReSendAckCode += OnClientReSendAckCode;
                            // todo add subscribers
                            break;

                        case Command.Logout:
                            sm.clientLogout(ref clientList, ref clientInfo, /*msgReceived,*/msgToSend);
                            sm.ClientLogout -= OnClientLogout;
                            sm.ClientLogout += OnClientLogout;
                            sm.ClientLogout -= servLogg.OnClientLogoutLogger;
                            sm.ClientLogout += servLogg.OnClientLogoutLogger;
                            KeepProcessing = false;
                            break;

                        case Command.Message: //Text of the message that we will broadcast to all users
                            sm.clientMessage(msgReceived, msgToSend);
                            sm.ClientMessage -= OnClientMessage;
                            sm.ClientMessage += OnClientMessage;
                            sm.ClientMessage -= servLogg.OnClientMessageLogger;
                            sm.ClientMessage += servLogg.OnClientMessageLogger;
                            break;

                        case Command.privMessage: //to deleted Command.Message can manage priv
                            //sm.clientMessage(msgReceived, msgToSend);
                            sm.SendMessage(clientList, clientInfo, msgReceived, msgToSend, true);
                            sm.ClientSendMessage -= OnClientSendMessage;
                            sm.ClientSendMessage += OnClientSendMessage;
                            sm.ClientSendMessage -= servLogg.OnClientSendMessageLogger;
                            sm.ClientSendMessage += servLogg.OnClientSendMessageLogger;
                            break;

                        case Command.List:  //Send the names of all users in the chat room to the new user
                            sm.sendClientList(clientList, clientInfo, msgToSend);
                            sm.ClientList -= OnClientList;
                            sm.ClientList += OnClientList;
                            break;
                    }

                    if (msgToSend.cmdCommand != Command.List && msgToSend.cmdCommand != Command.privMessage)   //List messages are not broadcasted
                    {
                        sm.SendMessage(clientList, clientInfo, msgReceived, msgToSend);
                        sm.ClientSendMessage -= OnClientSendMessage;
                        sm.ClientSendMessage += OnClientSendMessage;
                        sm.ClientSendMessage -= servLogg.OnClientSendMessageLogger;
                        sm.ClientSendMessage += servLogg.OnClientSendMessageLogger;
                    }
                    /*else if (msgToSend.cmdCommand != Command.List && msgToSend.cmdCommand == Command.privMessage)
                    {
                        sm.SendMessage(clientList, clientInfo, msgReceived, msgToSend, true);
                        sm.ClientSendMessage -= OnClientSendMessage;
                        sm.ClientSendMessage += OnClientSendMessage;
                        sm.ClientSendMessage -= servLogg.OnClientSendMessageLogger;
                        sm.ClientSendMessage += servLogg.OnClientSendMessageLogger;
                    }*/

                    sm.ReceivedMessage(clientInfo, msgReceived, byteData);//only if user is login
                    sm.ClientReceiMessage -= OnClientReceiMessage;
                    sm.ClientReceiMessage += OnClientReceiMessage;
                    sm.ClientReceiMessage -= servLogg.OnClientReceiMessageLogger;
                    sm.ClientReceiMessage += servLogg.OnClientReceiMessageLogger;

                }
                catch (Exception ex)
                {
                    //so we make sure that client with got crash or internet close, server will send log out message

                    Data msgToSend = new Data();
                    msgToSend.cmdCommand = Command.Logout;

                    string exMessage = ("client: " + sm.clientLogout(ref clientList, ref clientInfo/*, msgReceived*/, msgToSend, true) + " " + ex.Message);
                    Console.WriteLine(exMessage);
                    servLogg.msgLog(exMessage);

                    sm.ClientLogout -= OnClientLogout;
                    sm.ClientLogout += OnClientLogout;

                    sm.SendMessage(clientList, clientInfo, null, msgToSend);
                    sm.ClientSendMessage -= OnClientSendMessage;
                    sm.ClientSendMessage += OnClientSendMessage;
                    sm.ClientSendMessage -= servLogg.OnClientSendMessageLogger;
                    sm.ClientSendMessage += servLogg.OnClientSendMessageLogger;
                    break;
                }
            }
            clientInfo.cSocket.Close();
        }
        /*
        private void logRegClient(ref Client clientInfo, ref bool isLogin, ref ServerManager sm)
        {
            bool KeepProcessing = true;

            while (KeepProcessing)
            {
                try
                {
                    //Transform the array of bytes received from the user into an
                    //intelligent form of object Data
                    DataLogin msgReceived = new DataLogin(byteData);

                    //We will send this object in response the users request
                    DataLogin msgToSend = new DataLogin();

                    //If the message is to login, logout, or simple text message
                    //then when send to others the type of the message remains the same
                    msgToSend.cmdCommand = msgReceived.cmdCommand;
                    msgToSend.loginName = msgReceived.loginName;
                    //msgToSend.strMessage = msgReceived.strMessage;

                    switch (msgReceived.cmdCommand)
                    {
                        case CommandLogin.Login:

                            sm.clientLogin(ref clientInfo, msgReceived, ref msgToSend);

                            sm.ClientLogin -= OnClientLogin;
                            sm.ClientLogin += OnClientLogin;
                            sm.ClientLogin -= servLogg.OnClientLoginLogger;
                            sm.ClientLogin += servLogg.OnClientLoginLogger;
                            break;

                        case CommandLogin.Reg:
                            sm.clientRegistration(msgReceived, ref msgToSend);
                            sm.ClientRegistration += OnClientRegistration;
                            // todo add subscribers
                            break;

                        case CommandLogin.ReSendEmail: //Text of the message that we will broadcast to all users
                            sm.clientReSendActivCode(msgReceived, ref msgToSend);
                            sm.ClientReSendAckCode += OnClientReSendAckCode;
                            // todo add subscribers
                            break;
                    }

                    //if (msgToSend.cmdCommand != Command.List && msgToSend.cmdCommand != Command.privMessage)   //List messages are not broadcasted
                    //{
                    //    sm.SendMessage(clientList, msgReceived, msgToSend);

                    //}
                    //else if (msgToSend.cmdCommand != Command.List && msgToSend.cmdCommand == Command.privMessage)
                    //{
                    //    sm.SendMessage(clientList, msgReceived, msgToSend, true);

                    //}

                    sm.SendMessage(clientInfo, msgReceived, msgToSend);
                    //Start listening to the message send by the user
                    sm.ReceivedMessage(clientInfo, msgReceived, byteData);
                    if (msgToSend.strMessage == "You are succesfully Log in")
                    {
                        //sm.ReceivedMessage(msgReceived, byteData);
                        isLogin = true;
                        KeepProcessing = false;
                    }
                    //else
                    //{
                    //    isLogin = true;
                    //    KeepProcessing = false;
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    isLogin = false;
                    break;
                }
            }
        }*/
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