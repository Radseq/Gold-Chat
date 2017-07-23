using CommandClient;
using Server.ClientService;
using Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Server
{
    class ServerReceive : IServerReceive, IChannelList, IClientList, IClient
    {
        public Data Received { get; set; }
        public List<Channel> ChannelsList { get; set; }
        public List<Client> ListOfClientsOnline { get; set; }
        public Client Client { get; set; }

        public ServerReceive(Client client, List<Client> clientList, List<Channel> channelList)
        {
            Client = client;
            ListOfClientsOnline = clientList;
            ChannelsList = channelList;
        }

        // I want to make strategy pattern but i must add void respind function into ipreparerespond
        DataContext dataContext = null;

        ClientLogin clientLogin = new ClientLogin();
        ClientRegistration clientRegistration = new ClientRegistration();
        ClientChangePassword clientChangePassword = new ClientChangePassword();
        ClientLostPassword clientLostPassword = new ClientLostPassword();
        ClientSendActiveCode clientReSendActivCode = new ClientSendActiveCode();
        ClientLogout clientLogout = new ClientLogout();
        ClientMessages clientMessage = new ClientMessages();

        ClientListManager clientListManager = new ClientListManager();

        ClientPrivateMessage clientPrivateMessage = new ClientPrivateMessage();
        ClientCreateChannel clientCreateChannel = new ClientCreateChannel();
        ClientJoinChannel clientJoinChannel = new ClientJoinChannel();
        ClientExitChannel clientExitChannel = new ClientExitChannel();
        ClientDeleteChannel clientDeleteChannel = new ClientDeleteChannel();
        ClientEnterChannel clientEnterChannel = new ClientEnterChannel();
        ClientLeaveChannel clientLeaveChannel = new ClientLeaveChannel();
        ManageClientFriend manageClientFriend = new ManageClientFriend();
        ClientIgnoreUser clientIgnoreUser = new ClientIgnoreUser();
        ClientKick clientKick = new ClientKick();
        ClientBan clientBan = new ClientBan();
        ClientKickFromChannel clientKickFromChannel = new ClientKickFromChannel();
        ClientBanFromChannel clientBanFromChannel = new ClientBanFromChannel();
        ClientSendFile clientSendFile = new ClientSendFile();
        ClientSendFileInfo clientSendFileInfo = new ClientSendFileInfo();

        byte[] byteData = new byte[1024];

        public event EventHandler<ClientEventArgs> ClientReceiMessage;
        LoggerToFile servLogger = LoggerToFile.Instance;

        public void startReceiver()
        {
            Client.cSocket.BeginReceive(byteData, 0, byteData.Length, 0, new AsyncCallback(OnReceive), Client);
        }

        private void OnReceive(IAsyncResult ar)
        {
            // Retrieve the client and the handler socket  
            // from the asynchronous client.  
            Client client = (Client)ar.AsyncState;

            //Transform the array of bytes received from the user into an
            //intelligent form of object Data
            Received = new Data(byteData);
            try
            {
                switch (Received.cmdCommand)
                {
                    case Command.Login:
                        dataContext = new DataContext(clientLogin);
                        break;

                    case Command.Registration:
                        dataContext = new DataContext(clientRegistration);
                        break;

                    case Command.changePassword:
                        dataContext = new DataContext(clientChangePassword);
                        break;

                    case Command.lostPassword:
                        dataContext = new DataContext(clientLostPassword);
                        break;

                    case Command.SendActivationCode:
                        dataContext = new DataContext(clientReSendActivCode);
                        break;

                    case Command.Logout:
                        dataContext = new DataContext(clientLogout);
                        break;

                    case Command.Message:
                        dataContext = new DataContext(clientMessage);
                        break;

                    case Command.privMessage:
                        dataContext = new DataContext(clientPrivateMessage);
                        break;

                    case Command.createChannel:
                        dataContext = new DataContext(clientCreateChannel);
                        break;

                    case Command.joinChannel:
                        dataContext = new DataContext(clientJoinChannel);
                        break;

                    case Command.exitChannel:
                        dataContext = new DataContext(clientExitChannel);
                        break;

                    case Command.editChannel:
                        // TODO ALL
                        break;

                    case Command.deleteChannel:
                        dataContext = new DataContext(clientDeleteChannel);
                        break;

                    case Command.enterChannel:
                        dataContext = new DataContext(clientEnterChannel);
                        break;

                    case Command.leaveChannel:
                        dataContext = new DataContext(clientLeaveChannel);
                        break;

                    case Command.List:
                        dataContext = new DataContext(clientListManager);
                        break;

                    case Command.manageFriend:
                        dataContext = new DataContext(manageClientFriend);
                        break;

                    case Command.ignoreUser:
                        dataContext = new DataContext(clientIgnoreUser);
                        break;

                    case Command.kick:
                        dataContext = new DataContext(clientKick);
                        break;

                    case Command.ban:
                        dataContext = new DataContext(clientBan);
                        break;

                    case Command.kickUserChannel:
                        dataContext = new DataContext(clientKickFromChannel);
                        break;

                    case Command.banUserChannel:
                        dataContext = new DataContext(clientBanFromChannel);
                        break;

                    case Command.sendFile:
                        if (Received.strFileMsg != null)
                            dataContext = new DataContext(clientSendFile);
                        else
                            dataContext = new DataContext(clientSendFileInfo);
                        break;
                    default:
                        throw new ArgumentException("Wrong Package command from client");
                }
                dataContext.Load(client, Received, ListOfClientsOnline, ChannelsList);
                dataContext.Execute();
                dataContext.Response();

                ReceivedMessage(client, Received, byteData);
            }
            catch (Exception ex)
            {
                // So we make sure that client which got crash or internet close, server will send log out message
                clientLogout.Send.cmdCommand = Command.Logout;
                clientLogout.Send.strName = Client.strName;
                clientLogout.Load(client, Received, ListOfClientsOnline, ChannelsList);
                clientLogout.Execute();
                string exMessage = ("client: " + client.strName + " " + ex.Message);
                Console.WriteLine(exMessage);
                //Console.WriteLine(ex.StackTrace);
                servLogger.Log(exMessage);

                clientLogout.Response();
            }
        }

        private void ReceivedMessage(Client conClient, Data msgReceived, byte[] byteData)
        {
            if (msgReceived.cmdCommand != Command.Logout)
                conClient.cSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), conClient);
            else if (msgReceived.strMessage != null) // I want to see messages, messages will be null on login/logout
                OnClientReceiMessage((int)msgReceived.cmdCommand, msgReceived.strName, msgReceived.strMessage, msgReceived.strMessage);
        }
        protected virtual void OnClientReceiMessage(int command, string cName, string cMessage, string cFriendName)
        {
            ClientReceiMessage?.Invoke(this, new ClientEventArgs() { clientCommand = command, clientName = cName, clientMessageReciv = cMessage, clientFriendName = cFriendName });// tu jeszcze nie wiem
        }
    }
}
