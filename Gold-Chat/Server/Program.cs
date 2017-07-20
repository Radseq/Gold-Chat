using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class server
    {
        public Socket ServerSocket { get; set; }

        DataBaseManager db = DataBaseManager.Instance;

        private bool runServer = true;

        byte[] byteData = new byte[1024];

        public const int max_users = 50;
        EmailSender emailSender = EmailSender.Instance;

        static string dateFile = DateTime.Now.ToString("dd_MM_yyyy");

        static LoggerToFile servLogg = LoggerToFile.Instance;
        ServerManager sm = new ServerManager();

        public server()
        {
            try
            {
                ServerSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                ServerSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
                ServerSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, Settings.SERVER_PORT));
                ServerSocket.Listen(5);

                Console.WriteLine(" >> Server Started");
                servLogg.Log(" >> Server Started"); /*on Adress:" + ((IPEndPoint)ServerSocket.RemoteEndPoint).Address.ToString() + " Port:" + ((IPEndPoint)ServerSocket.RemoteEndPoint).Port.ToString());*/

                //sm.ClientLogin += OnClientLogin;
                //sm.ClientLogin += servLogg.OnClientLoginLogger;
                //sm.ClientRegistration += OnClientRegistration;
                //sm.ClientReSendAckCode += OnClientReSendAckCode;
                emailSender.EmailSended += servLogg.OnEmaiSended;
                emailSender.EmailSended += servLogg.OnEmaiNotyficationLoginSended;
                //sm.ClientMessage += OnClientMessage;
                //sm.ClientMessage += servLogg.OnClientMessageLogger;
                //sm.ClientSendMessage += OnClientSendMessage;
                //sm.ListOfClientsOnline += OnClientList;
                //sm.ClientSendMessage += servLogg.OnClientSendMessageLogger;
                //sm.ClientReceiMessage += OnClientReceiMessage;
                //sm.ClientReceiMessage += servLogg.OnClientReceiMessageLogger;
                // DataBase
                db.ConnectToDB += servLogg.OnConnectToDB;
                db.ExecuteNonQuery += servLogg.OnExecuteNonQuery;
                db.ExecuteReader += servLogg.OnExecuteReader;

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
            //strWriter.Close();
            //strWriter.Dispose();
            db.closeConnection();
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
            Console.WriteLine("SendList " + e.clientMessageToSend + " " + e.clientMessageTwoToSend);
        }

        private void OnClientMessage(object sender, ClientEventArgs e)
        {
            Console.WriteLine("clientMessage " + e.clientMessageReciv);
        }

        private void OnClientLogout(object sender, ClientEventArgs e)
        {
            Console.WriteLine(e.clientName + " has left the room>>>");
        }
        #endregion
    }

    class Program
    {
        static void Main(string[] args)
        {
            server serv = new server(); //after changes on this and ServerManager i think this class can be deleted

            Console.ReadLine();
            serv.Stop();
        }
    }
}