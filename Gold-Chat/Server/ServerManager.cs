using Server.Interfaces;
using Server.Interfaces.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{

    //This class represents a client connection to server
    public class ServerManager : IServerGetConnection, IClient
    {
        private static ManualResetEvent allDone = new ManualResetEvent(false);

        //The collection of all clients logged into the room
        private List<Client> clientList = new List<Client>();
        //list of all channels
        private List<Channel> channels = new List<Channel>();

        LoggerToFile servLogger = LoggerToFile.Instance;
        public Client Client { get; set; }

        LoggerToConsole consoleLog = new LoggerToConsole();


        private readonly IGetListOfChannels GetListOfChannels;

        public ServerManager(IGetListOfChannels getListOfChannels)
        {
            GetListOfChannels = getListOfChannels;
            channels = GetListOfChannels.Get();
        }


        /*public ServerManager()
        {
            GetListOfChannelsFromDataBase getListOfChannlsFromDataBase = new GetListOfChannelsFromDataBase();
            channels = getListOfChannlsFromDataBase.Get();
        }*/

        public void acceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;

            Client = new Client();
            Client.cSocket = listener.EndAccept(ar);
            Client.addr = (IPEndPoint)Client.cSocket.RemoteEndPoint;
            // Sent of implementation i think i must do builder, because rest of inicialize client is in login command

            string acceptConnectrion = $" >> Accept connection from client: {Client.addr.Address} on Port: {Client.addr.Port}";// + " Users Connected: " + clientList.Count;
            consoleLog.Log(acceptConnectrion);
            servLogger.Log(acceptConnectrion);

            clientList.Add(Client); // When a user logs in to the server then we add her to our list of clients

            ServerReceive serverReceive = new ServerReceive(Client, clientList, channels);
            serverReceive.startReceiver();
        }

        public void getConnection(Socket sock)
        {
            allDone.Reset();
            sock.BeginAccept(new AsyncCallback(acceptCallback), sock);
            allDone.WaitOne();
        }
    }
}