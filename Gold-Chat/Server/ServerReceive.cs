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

        DataContext dataContext = new DataContext();

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
                        dataContext.SetContext(clientLogin);
                        break;

                    case Command.Registration:
                        dataContext.SetContext(clientRegistration);
                        break;

                    case Command.changePassword:
                        dataContext.SetContext(clientChangePassword);
                        break;

                    case Command.lostPassword:
                        dataContext.SetContext(clientLostPassword);
                        break;

                    case Command.SendActivationCode:
                        dataContext.SetContext(clientReSendActivCode);
                        break;

                    case Command.Logout:
                        dataContext.SetContext(clientLogout);
                        break;

                    case Command.Message:
                        dataContext.SetContext(clientMessage);
                        break;

                    case Command.privMessage:
                        dataContext.SetContext(clientPrivateMessage);
                        break;

                    case Command.createChannel:
                        dataContext.SetContext(clientCreateChannel);
                        break;

                    case Command.joinChannel:
                        dataContext.SetContext(clientJoinChannel);
                        break;

                    case Command.exitChannel:
                        dataContext.SetContext(clientExitChannel);
                        break;

                    case Command.editChannel:
                        // TODO ALL
                        break;

                    case Command.deleteChannel:
                        dataContext.SetContext(clientDeleteChannel);
                        break;

                    case Command.enterChannel:
                        dataContext.SetContext(clientEnterChannel);
                        break;

                    case Command.leaveChannel:
                        dataContext.SetContext(clientLeaveChannel);
                        break;

                    case Command.List:
                        dataContext.SetContext(clientListManager);
                        break;

                    case Command.manageFriend:
                        dataContext.SetContext(manageClientFriend);
                        break;

                    case Command.ignoreUser:
                        dataContext.SetContext(clientIgnoreUser);
                        break;

                    case Command.kick:
                        dataContext.SetContext(clientKick);
                        break;

                    case Command.ban:
                        dataContext.SetContext(clientBan);
                        break;

                    case Command.kickUserChannel:
                        dataContext.SetContext(clientKickFromChannel);
                        break;

                    case Command.banUserChannel:
                        dataContext.SetContext(clientBanFromChannel);
                        break;

                    case Command.sendFile:
                        if (Received.strFileMsg != null)
                            dataContext.SetContext(clientSendFile);
                        else
                            dataContext.SetContext(clientSendFileInfo);
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
