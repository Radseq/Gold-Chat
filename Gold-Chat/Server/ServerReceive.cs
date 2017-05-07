﻿using CommandClient;
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
        public List<Channel> ChannelList { get; set; }
        public List<Client> ClientList { get; set; }
        public Client Client { get; set; }

        public ServerReceive(Client client, List<Client> clientList, List<Channel> channelList)
        {
            Client = client;
            ClientList = clientList;
            ChannelList = channelList;
        }

        ClientLogin clientLogin = new ClientLogin();
        ClientRegistration clientRegistration = new ClientRegistration();
        ClientChangePassword clientChangePassword = new ClientChangePassword();
        ClientLostPassword clientLostPassword = new ClientLostPassword();
        ClientReSendActiveCode clientReSendActivCode = new ClientReSendActiveCode();
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
            Client client = (Client)ar.AsyncState; // bad idea but catch need to see client, or maybe not bad idea because object is created and catch will not occur on constructor of class

            //Transform the array of bytes received from the user into an
            //intelligent form of object Data
            Received = new Data(byteData);
            try
            {
                switch (Received.cmdCommand)
                {
                    case Command.Login:
                        clientLogin.Load(client, Received, ClientList);
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

                    case Command.ReSendActiveCode:
                        clientReSendActivCode.Load(client, Received);
                        clientReSendActivCode.Execute();
                        clientReSendActivCode.RespondToClient();
                        break;

                    case Command.Logout:
                        clientLogout.Load(client, Received, ClientList, ChannelList);
                        clientLogout.Execute();
                        clientLogout.Response();
                        break;

                    case Command.Message:
                        clientMessage.Load(client, Received, ClientList);
                        clientMessage.Execute();
                        clientMessage.Response();
                        break;

                    case Command.privMessage:
                        clientPrivateMessage.Load(client, Received, ClientList);
                        clientPrivateMessage.Response();
                        break;

                    case Command.createChannel:
                        clientCreateChannel.Load(client, Received, ClientList, ChannelList);
                        clientCreateChannel.Execute();
                        clientCreateChannel.RespondToClient();
                        break;

                    case Command.joinChannel:
                        clientJoinChannel.Load(client, Received, ClientList);
                        clientJoinChannel.Execute();
                        clientJoinChannel.RespondToClient();
                        break;

                    case Command.exitChannel:
                        clientExitChannel.Load(client, Received);
                        clientExitChannel.Execute();
                        clientExitChannel.RespondToClient();
                        break;

                    case Command.deleteChannel:
                        clientDeleteChannel.Load(client, Received, ClientList, ChannelList);
                        clientDeleteChannel.Execute();
                        clientDeleteChannel.Response();
                        break;
                    case Command.enterChannel:
                        clientEnterChannel.Load(client, Received, ClientList, ChannelList);
                        clientEnterChannel.Execute();
                        break;
                    case Command.leaveChannel:
                        clientLeaveChannel.Load(client, Received, ClientList, ChannelList);
                        clientLeaveChannel.Execute();
                        clientLeaveChannel.Response();
                        break;

                    case Command.List:  // Send the names of all users in the chat room to the new user
                        clientListManager.Load(client, Received, ClientList, ChannelList);
                        clientListManager.Execute();
                        break;

                    case Command.manageFriend:
                        manageClientFriend.Load(client, Received, ClientList);
                        manageClientFriend.Execute();
                        //manageClientFriend.RespondToClient();
                        //SendServerRespond(ref client, msgToSend);
                        break;
                    case Command.ignoreUser:
                        clientIgnoreUser.Load(client, Received);
                        clientIgnoreUser.Execute();
                        clientIgnoreUser.RespondToClient();
                        break;
                    /// Not Implement !!!
                    case Command.kick:
                        clientKick.Load(client, Received, ClientList);
                        clientKick.Execute();
                        clientKick.RespondToClient();
                        break;
                    /// Not Implement !!!
                    case Command.ban:
                        clientBan.Load(client, Received, ClientList);
                        clientBan.Execute();
                        clientBan.RespondToClient();
                        break;
                    /// Not Implement !!!
                    case Command.kickUserChannel:
                        clientKickFromChannel.Load(client, Received, ClientList, ChannelList);
                        clientKickFromChannel.Execute();
                        clientKickFromChannel.RespondToClient();
                        break;
                    /// Not Implement !!!
                    case Command.banUserChannel:
                        clientBanFromChannel.Load(client, Received, null, ChannelList);
                        clientBanFromChannel.Execute();
                        clientBanFromChannel.RespondToClient();
                        break;
                }

                ReceivedMessage(client, Received, byteData);
            }
            catch (Exception ex)
            {
                // So we make sure that client which got crash or internet close, server will send log out message
                clientLogout.Send.cmdCommand = Command.Logout;
                clientLogout.Send.strName = Client.strName;
                clientLogout.Load(client, Received, ClientList, ChannelList);
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