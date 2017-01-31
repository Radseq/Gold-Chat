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
        //This class represents a client connected to server
        /*public class Client
        {
            public bool KeepProcessing { get; set; }
            public Socket Socket { get; set; }
            public string strName;  //Name by which the user logged into the chat room
        }*/

        // public int Port { get; set; }
        public Socket ServerSocket { get; set; }
        public List<Client> clientList = new List<Client>();
        private bool runServer = true;

        byte[] byteData = new byte[1024];

        public const int max_users = 50;

        //Client client;
        static StreamWriter strWriter = new StreamWriter("serverLogger.txt", true);
        ServerLogger servLogg = new ServerLogger(ref strWriter);

        public server()
        {
            try
            {
                //Port = 2323;
                ServerSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                ServerSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
                ServerSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, 5000));
                ServerSocket.Listen(5);

                Console.WriteLine(" >> Server Started");
                servLogg.msgLog(" >> Server Started");

                //The collection of all clients logged into the room (an array of type ClientInfo)
                //ArrayList clientList = new ArrayList();

                while (runServer)
                {
                    if (clientList.Count <= max_users)
                    {
                        Socket clientSocket = ServerSocket.Accept();
                        if (clientSocket != null)
                        {
                            string acceptConnectrion = " >> Accept connection from client: " + ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString() + " on Port: " + ((IPEndPoint)clientSocket.RemoteEndPoint).Port.ToString() + " Users Connected: " + clientList.Count;
                            Console.WriteLine(acceptConnectrion);
                            servLogg.msgLog(acceptConnectrion);
                            //Client cl = new Client();
                            //cl.KeepProcessing = true;
                            // cl.Socket = clientSocket;
                            //Clients.Add(cl); //add to clients collection
                            clientSocket.Receive(byteData, byteData.Length, SocketFlags.None);
                            ParameterizedThreadStart pts = new ParameterizedThreadStart(ProcessClientThreadProc);
                            Thread thr = new Thread(pts);
                            thr.IsBackground = true;
                            thr.Start(clientSocket); //start client processing thread
                            clientSocket = null;
                            //cl = null;
                        }
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
            while (clientList.Count > 0)
            {
                clientList[0].KeepProcessing = false;
                clientList[0].cSocket.Close();
                clientList.RemoveAt(0);
            }
            strWriter.Close();
            strWriter.Dispose();
        }

        private void ProcessClientThreadProc(object obj)
        {
            //Client cl = (Client)obj;

            Socket clientSocket = (Socket)obj;

            //cl.Socket = clientSocket;
            bool KeepProcessing = true;

            ServerManager sm = new ServerManager();

            while (KeepProcessing)
            {
                try
                {
                    //Transform the array of bytes received from the user into an
                    //intelligent form of object Data
                    Data msgReceived = new Data(byteData);

                    //We will send this object in response the users request
                    Data msgToSend = new Data();

                    //byte[] message;

                    //If the message is to login, logout, or simple text message
                    //then when send to others the type of the message remains the same
                    msgToSend.cmdCommand = msgReceived.cmdCommand;
                    msgToSend.strName = msgReceived.strName;
                    msgToSend.friendName = msgReceived.friendName;

                    switch (msgReceived.cmdCommand)
                    {
                        case Command.Login:

                            //When a user logs in to the server then we add her to our
                            //list of clients

                            Client clientInfo = new Client();
                            clientInfo.cSocket = clientSocket;
                            clientInfo.strName = msgReceived.strName;

                            sm.clientLogin(ref clientInfo, msgReceived, msgToSend);

                            clientList.Add(clientInfo);
                            sm.ClientLogin -= OnClientLogin;
                            sm.ClientLogin += OnClientLogin;
                            sm.ClientLogin -= servLogg.OnClientLoginLogger;
                            sm.ClientLogin += servLogg.OnClientLoginLogger;
                            //Set the text of the message that we will broadcast to all users
                            //msgToSend.strMessage = "<<<" + client.strName + " has joined the room>>>";
                            //Console.WriteLine(client.strName + " has joined the room>>>");
                            break;

                        case Command.Logout:

                            sm.clientLogout(ref clientList, ref clientSocket, /*msgReceived,*/ msgToSend);
                            sm.ClientLogout -= OnClientLogout;
                            sm.ClientLogout += OnClientLogout;
                            sm.ClientLogout -= servLogg.OnClientLogoutLogger;
                            sm.ClientLogout += servLogg.OnClientLogoutLogger;

                            //When a user wants to log out of the server then we search for her 
                            //in the list of clients and close the corresponding connection
                            /*
                            int nIndex = 0;
                            foreach (Client c in clientList)
                            {
                                if (c.Socket == clientSocket)
                                {
                                    clientList.RemoveAt(nIndex);
                                    //user_list.Items.Remove(msgReceived.strName);
                                    break;
                                }
                                ++nIndex;
                            }

                            clientSocket.Close();

                            msgToSend.strMessage = "<<<" + msgReceived.strName + " has left the room>>>";
                            Console.WriteLine(msgReceived.strName + " has left the room>>>");*/
                            KeepProcessing = false;
                            break;

                        case Command.Message:
                            sm.clientMessage(msgReceived, msgToSend);
                            //Set the text of the message that we will broadcast to all users
                            //msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
                            sm.ClientMessage -= OnClientMessage;
                            sm.ClientMessage += OnClientMessage;
                            sm.ClientMessage -= servLogg.OnClientMessageLogger;
                            sm.ClientMessage += servLogg.OnClientMessageLogger;
                            break;

                        case Command.privMessage:
                            sm.clientMessage(msgReceived, msgToSend);
                            //Set the text of the message that we will broadcast to all users
                            // msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
                            //Console.WriteLine(msgReceived.strName + ": " + msgReceived.friendName + " : " + msgReceived.strMessage);
                            break;

                        case Command.List:
                            sm.clientList(clientList, msgToSend);
                            sm.ClientList -= OnClientList;
                            sm.ClientList += OnClientList;
                            /*client.clientList(this, msgToSend);

                            //Send the names of all users in the chat room to the new user
                            msgToSend.cmdCommand = Command.List;
                            msgToSend.strName = null;
                            msgToSend.strMessage = null;

                            //Collect the names of the user in the chat room
                            foreach (Client c in clientList)
                            {
                                //To keep things simple we use asterisk as the marker to separate the user names
                                msgToSend.strMessage += c.strName + "*";
                            }

                            message = msgToSend.ToByte();

                            //Send the name of the users in the chat room
                            clientSocket.Send(message, 0, message.Length, SocketFlags.None);*/
                            break;
                    }

                    if (msgToSend.cmdCommand != Command.List && msgToSend.cmdCommand != Command.privMessage)   //List messages are not broadcasted
                    {
                        sm.SendMessage(clientList, msgReceived, msgToSend);
                        sm.ClientSendMessage -= OnClientSendMessage;
                        sm.ClientSendMessage += OnClientSendMessage;
                        sm.ClientSendMessage -= servLogg.OnClientSendMessageLogger;
                        sm.ClientSendMessage += servLogg.OnClientSendMessageLogger;
                    }
                    else if (msgToSend.cmdCommand != Command.List && msgToSend.cmdCommand == Command.privMessage)
                    {
                        sm.SendMessage(clientList, msgReceived, msgToSend, true);
                        sm.ClientSendMessage -= OnClientSendMessage;
                        sm.ClientSendMessage += OnClientSendMessage;
                        sm.ClientSendMessage -= servLogg.OnClientSendMessageLogger;
                        sm.ClientSendMessage += servLogg.OnClientSendMessageLogger;
                    }

                    sm.ReceivedMessage(msgReceived, byteData);
                    sm.ClientReceiMessage -= OnClientReceiMessage;
                    sm.ClientReceiMessage += OnClientReceiMessage;
                    sm.ClientReceiMessage -= servLogg.OnClientReceiMessageLogger;
                    sm.ClientReceiMessage += servLogg.OnClientReceiMessageLogger;

                    //If the user is logging out then we need not listen from her
                    /*if (msgReceived.cmdCommand != Command.Logout)
                    {
                        //Start listening to the message send by the user
                        clientSocket.Receive(byteData, byteData.Length, SocketFlags.None);
                        //KeepProcessing = false;
                    }*/
                    //}));
                }
                catch (Exception ex)
                {
                    /*int nIndex = 0;
                    foreach (Client client in clientList)
                    {
                        if (client.cSocket == clientSocket)
                        {
                            clientList.RemoveAt(nIndex);
                            Console.WriteLine("client " + client.strName + " " + ex.Message);
                            break;
                        }
                        ++nIndex;
                    }

                    KeepProcessing = false;*/
                    //so we make sure that client with got crash or internet close, server will send log out message

                    Data msgToSend = new Data();
                    msgToSend.cmdCommand = Command.Logout;

                    string exMessage = ("client: " + sm.clientLogout(ref clientList, ref clientSocket/*, msgReceived*/, msgToSend, true) + " " + ex.Message);
                    Console.WriteLine(exMessage);
                    servLogg.msgLog(exMessage);

                    sm.ClientLogout -= OnClientLogout;
                    sm.ClientLogout += OnClientLogout;

                    sm.SendMessage(clientList, null, msgToSend);
                    sm.ClientSendMessage -= OnClientSendMessage;
                    sm.ClientSendMessage += OnClientSendMessage;
                    sm.ClientSendMessage -= servLogg.OnClientSendMessageLogger;
                    sm.ClientSendMessage += servLogg.OnClientSendMessageLogger;
                    break;
                }
            }
            clientSocket.Close();
            //clientList.Remove(clientInfo);
        }

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
    }

    class Program
    {
        static void Main(string[] args)
        {
            server serv = new server();

            Console.ReadLine();
            serv.Stop();
        }
    }
}