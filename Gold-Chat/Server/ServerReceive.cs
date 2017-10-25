using Autofac;
using CommandClient;
using Server.ClientService;
using Server.Controllers;
using Server.Controllers.Lists;
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
            using (var scope = server._container.BeginLifetimeScope())
            {
                try
                {
                    switch (Received.cmdCommand)
                    {
                        case Command.Login:
                            dataContext.SetContext(scope.Resolve<LoginController>());
                            break;

                        case Command.Registration:
                            dataContext.SetContext(scope.Resolve<RegistrationController>());
                            break;

                        case Command.changePassword:
                            dataContext.SetContext(scope.Resolve<ChangePasswordController>());
                            break;

                        case Command.lostPassword:
                            dataContext.SetContext(scope.Resolve<LostPasswordController>());
                            break;

                        case Command.SendActivationCode:
                            dataContext.SetContext(scope.Resolve<SendActiveCodeController>());
                            break;

                        case Command.Logout:
                            dataContext.SetContext(scope.Resolve<ClientLogoutController>());
                            break;

                        case Command.Message:
                            dataContext.SetContext(scope.Resolve<ClientMessagesController>());
                            break;

                        case Command.privMessage:
                            dataContext.SetContext(scope.Resolve<PrivateMessageController>());
                            break;

                        case Command.createChannel:
                            dataContext.SetContext(scope.Resolve<CreateChannelController>());
                            break;

                        case Command.joinChannel:
                            dataContext.SetContext(scope.Resolve<JoinChannelController>());
                            break;

                        case Command.exitChannel:
                            dataContext.SetContext(scope.Resolve<ExitChannelController>());
                            break;

                        case Command.editChannel:
                            // TODO ALL
                            break;

                        case Command.deleteChannel:
                            dataContext.SetContext(scope.Resolve<DeleteChannelController>());
                            break;

                        case Command.enterChannel:
                            dataContext.SetContext(scope.Resolve<EnterChannelController>());
                            break;

                        case Command.leaveChannel:
                            dataContext.SetContext(scope.Resolve<LeaveChannelController>());
                            break;

                        case Command.List:
                            dataContext.SetContext(ResolveList(scope, Received.strMessage));
                            break;

                        case Command.manageFriend:
                            dataContext.SetContext(scope.Resolve<ManageClientFriendController>());
                            break;

                        case Command.ignoreUser:
                            dataContext.SetContext(scope.Resolve<IgnoreUserController>());
                            break;

                        case Command.kick:
                            dataContext.SetContext(scope.Resolve<ClientKickController>());
                            break;

                        case Command.ban:
                            dataContext.SetContext(scope.Resolve<BanController>());
                            break;

                        case Command.kickUserChannel:
                            dataContext.SetContext(scope.Resolve<KickFromChannelController>());
                            break;

                        case Command.banUserChannel:
                            dataContext.SetContext(scope.Resolve<BanFromChannelController>());
                            break;

                        case Command.sendFile:
                            if (Received.strFileMsg != null)
                                dataContext.SetContext(scope.Resolve<ClientSendFileController>());
                            else
                                dataContext.SetContext(scope.Resolve<SendFileInfoController>());
                            break;
                        default:
                            throw new ArgumentException("Wrong Package command from client");
                    }
                    //dataContext.Load(client, Received, ListOfClientsOnline, ChannelsList);
                    //dataContext.Execute();
                    //dataContext.Response();

                    //ReceivedMessage(client, Received, byteData);

                }
                catch (Exception ex)
                {
                    // So we make sure that client which got crash or internet close, server will send log out message
                    dataContext.SetContext(scope.Resolve<ClientLogoutController>());

                    string exMessage = $"client: {client.strName} {ex.Message}";
                    Console.WriteLine(exMessage);
                    //Console.WriteLine(ex.StackTrace);
                    servLogger.Log(exMessage);
                }
                finally
                {
                    dataContext.Load(client, Received, ListOfClientsOnline, ChannelsList);
                    dataContext.Execute();
                    dataContext.Response();

                    ReceivedMessage(client, Received, byteData);
                }
            }
        }

        private IBuildResponse ResolveList(ILifetimeScope scope, string received)
        {
            if (received == "Friends")
                return scope.Resolve<ClientFriendsListModule>();
            else if (received == "ChannelsJoined")
                return scope.Resolve<ClientJoinedChannelsListModule>();
            else if (received == "IgnoredUsers")
                return scope.Resolve<ClientIgnoredUsersListModule>();
            else if (received == "Channel")
                return scope.Resolve<ChannelsListModule>();
            else if (received == "ChannelUsers")
                return scope.Resolve<ChannelUsersListModule>();
            else
                return scope.Resolve<UsersOnlineListModule>();
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
