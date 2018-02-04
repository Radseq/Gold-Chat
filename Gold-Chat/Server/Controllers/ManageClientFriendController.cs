using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ManageFriend;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ManageClientFriendController : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientAddFriend;
        public event EventHandler<ClientEventArgs> ClientDeleteFriend;
        private readonly ISendMessageToNick SendMessage;
        private readonly IGetFriendId GetFriend;
        private readonly IAddFriend AddFriend;
        private readonly IDeleteFriend DeleteFriend;

        private List<Client> ListOfClientsOnline;

        string FriendName;

        public ManageClientFriendController(ISendMessageToNick sendMessage, IGetFriendId getFriend, IAddFriend addFriend, IDeleteFriend deleteFriend)
        {
            SendMessage = sendMessage;
            GetFriend = getFriend;
            AddFriend = addFriend;
            DeleteFriend = deleteFriend;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ListOfClientsOnline = clientList;
            Send.strName = Received.strMessage2;
        }

        public void Execute()
        {
            prepareResponse();
            string type = Received.strMessage;
            FriendName = Received.strMessage2;


            Int64 friend_id = GetFriend.GetFriendId(FriendName);
            if (friend_id > 0)
            {
                if (type == "Yes")
                {
                    bool InserClientToDb = AddFriend.Add(Client.id, friend_id); // client add friend in db
                    bool InserFriendToDb = AddFriend.Add(friend_id, Client.id); // friend add client in db

                    if (InserClientToDb && InserFriendToDb)
                    {
                        OnClientAddFriend(Client.strName, FriendName);
                        Send.strMessage = "Yes";

                        ResponseToNick();
                    }
                    else
                    {
                        Send.strMessage = "No";
                        Send.strMessage2 = FriendName;
                    }
                }
                else if (type == "Delete")
                {
                    bool userDeleteFriend = DeleteFriend.Delete(Client.id, friend_id);
                    bool friendDeleteUser = DeleteFriend.Delete(friend_id, Client.id);
                    if (userDeleteFriend && friendDeleteUser)
                    {
                        Send.strMessage = "Delete";

                        ResponseToNick();

                        //when client get delete then  he will send to server list ask
                        OnClientDeleteFriend(Client.strName, FriendName);
                    }
                }
                else if (type == "Add")
                {
                    ResponseToNick();
                }
                else if (type == "No")
                {
                    // Friend type no: he dont want be as friend
                    Send.strMessage = "No";
                    ResponseToNick();
                }
            }
            else
                Send.strMessage = "There is no friend that you want to add.";
        }

        private void ResponseToNick()
        {
            Data privSend;
            privSend = Send;
            privSend.strMessage2 = Client.strName;
            privSend.strName = FriendName;
            // if (Send.strName != privSend.strName)
            SendMessage.ResponseToNick(ListOfClientsOnline, privSend);
            //else
            //  Send.strMessage = "Cant add youself.";
        }

        public override void Response()
        {
            if (Send.strMessage == "No" || Send.strMessage == "Yes" || Send.strMessage == "Delete" || Send.strMessage == "There is no friend that you want to add.")
                base.Response();
        }

        protected virtual void OnClientAddFriend(string ClientName, string ClientFriendName)
        {
            ClientAddFriend?.Invoke(this, new ClientEventArgs() { clientName = ClientName, clientFriendName = ClientFriendName });
        }

        protected virtual void OnClientDeleteFriend(string ClientName, string ClientFriendName)
        {
            ClientDeleteFriend?.Invoke(this, new ClientEventArgs() { clientName = ClientName, clientFriendName = ClientFriendName });
        }
    }
}
