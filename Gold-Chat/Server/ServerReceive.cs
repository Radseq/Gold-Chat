using CommandClient;
using Server.ClientService;
using Server.ResponseMessages;
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
                        clientLogin.Load(client, Received, ListOfClientsOnline);
                        clientLogin.Execute();
                        clientLogin.Response();
                        break;

                    case Command.Registration:
                        clientRegistration.Load(client, Received);
                        clientRegistration.Execute();
                        clientRegistration.RespondToClient();
                        break;

                    case Command.changePassword:
                        clientChangePassword.Load(client, Received);
                        clientChangePassword.Execute();
                        clientChangePassword.RespondToClient();
                        break;

                    case Command.lostPassword:
                        clientLostPassword.Load(client, Received);
                        clientLostPassword.Execute();
                        clientLostPassword.RespondToClient();
                        break;

                    case Command.SendActivationCode:
                        clientReSendActivCode.Load(client, Received);
                        clientReSendActivCode.Execute();
                        clientReSendActivCode.RespondToClient();
                        break;

                    case Command.Logout:
                        clientLogout.Load(client, Received, ListOfClientsOnline, ChannelsList);
                        clientLogout.Execute();
                        clientLogout.Response();
                        break;

                    case Command.Message:
                        clientMessage.Load(client, Received, ListOfClientsOnline);
                        clientMessage.Execute();
                        clientMessage.Response();
                        break;

                    case Command.privMessage:
                        clientPrivateMessage.Load(client, Received, ListOfClientsOnline);
                        clientPrivateMessage.Response();
                        break;

                    case Command.createChannel:
                        clientCreateChannel.Load(client, Received, ListOfClientsOnline, ChannelsList);
                        clientCreateChannel.Execute();
                        //clientCreateChannel.RespondToClient();
                        break;

                    case Command.joinChannel:
                        clientJoinChannel.Load(client, Received, ListOfClientsOnline);
                        clientJoinChannel.Execute();
                        clientJoinChannel.RespondToClient();
                        break;

                    case Command.exitChannel:
                        clientExitChannel.Load(client, Received);
                        clientExitChannel.Execute();
                        clientExitChannel.RespondToClient();
                        break;

                    case Command.editChannel:
                        // TODO ALL
                        break;

                    case Command.deleteChannel:
                        clientDeleteChannel.Load(client, Received, ListOfClientsOnline, ChannelsList);
                        clientDeleteChannel.Execute();
                        //clientDeleteChannel.Response();
                        break;
                    case Command.enterChannel:
                        clientEnterChannel.Load(client, Received, ListOfClientsOnline, ChannelsList);
                        clientEnterChannel.Execute();
                        break;
                    case Command.leaveChannel:
                        clientLeaveChannel.Load(client, Received, ListOfClientsOnline, ChannelsList);
                        clientLeaveChannel.Execute();
                        clientLeaveChannel.Response();
                        break;

                    case Command.List:  // Send the names of all users in the chat room to the new user
                        clientListManager.Load(client, Received, ListOfClientsOnline, ChannelsList);
                        clientListManager.Execute();
                        break;

                    case Command.manageFriend:
                        manageClientFriend.Load(client, Received, ListOfClientsOnline);
                        manageClientFriend.Execute();
                        //manageClientFriend.RespondToClient();
                        //SendServerRespond(ref client, msgToSend);
                        break;
                    case Command.ignoreUser:
                        clientIgnoreUser.Load(client, Received);
                        clientIgnoreUser.Execute();
                        clientIgnoreUser.RespondToClient();
                        break;
                    case Command.kick:
                        clientKick.Load(client, Received, ListOfClientsOnline);
                        clientKick.Execute();
                        clientKick.RespondToClient();
                        break;
                    case Command.ban:
                        clientBan.Load(client, Received, ListOfClientsOnline);
                        clientBan.Execute();
                        clientBan.RespondToClient();
                        break;
                    case Command.kickUserChannel:
                        clientKickFromChannel.Load(client, Received, ListOfClientsOnline, ChannelsList);
                        clientKickFromChannel.Execute();
                        clientKickFromChannel.RespondToClient();
                        break;
                    case Command.banUserChannel:
                        clientBanFromChannel.Load(client, Received, ListOfClientsOnline, ChannelsList);
                        clientBanFromChannel.Execute();
                        clientBanFromChannel.RespondToClient();
                        break;

                    case Command.sendFile:
                        if (Received.strFileMsg != null)
                        {
                            clientSendFile.Load(client, Received, ListOfClientsOnline);
                            clientSendFile.Execute();
                            clientSendFile.RespondToClient();
                        }
                        else
                        {
                            clientSendFileInfo.Load(client, Received, ListOfClientsOnline);
                            clientSendFileInfo.Execute();
                            clientSendFileInfo.RespondToClient();
                        }
                        break;
                }

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
                Console.WriteLine(ex.StackTrace);
                servLogger.Log(exMessage);

                clientLogout.Response();
            }
        }

        private void ReceivedMessage(Client conClient, Data msgReceived, byte[] byteData)
        {
            if (msgReceived.cmdCommand != Command.Logout)
            {
                conClient.cSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), conClient);
            }
            else if (msgReceived.strMessage != null) // I want to see messages, messages will be null on login/logout
            {
                OnClientReceiMessage((int)msgReceived.cmdCommand, msgReceived.strName, msgReceived.strMessage, msgReceived.strMessage);
            }
        }
        protected virtual void OnClientReceiMessage(int command, string cName, string cMessage, string cFriendName)
        {
            ClientReceiMessage?.Invoke(this, new ClientEventArgs() { clientCommand = command, clientName = cName, clientMessageReciv = cMessage, clientFriendName = cFriendName });// tu jeszcze nie wiem
        }
    }
}
