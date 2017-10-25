using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ClientBan;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using Server.Utilies;
using System.Collections.Generic;

namespace Server.Controllers
{
    class BanController : ServerResponds, IClient, IBuildResponse
    {
        private readonly IAddBanTime InsertBanToDb;
        private readonly ISendMessageToAll SendMessage;

        List<Client> ListOfClientsOnline;

        private string BanTime;
        private string BanReason;
        private string BannedUserName;
        private Client BannedUser;
        private bool IsUserBanned;

        public BanController(IAddBanTime insertBanToDb, ISendMessageToAll sendMessage)
        {
            InsertBanToDb = insertBanToDb;
            SendMessage = sendMessage;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            ListOfClientsOnline = clientList;
            Received = receive;
            Client = client;
        }

        public void Execute()
        {
            prepareResponse();
            BannedUserName = Received.strMessage;
            BanTime = Received.strMessage2;
            BanReason = Received.strMessage3;

            Send.strMessage3 = null;
            Send.strMessage4 = null;

            if (Client.permission > 0)
            {
                BannedUser = ClientGets.getClinetByName(ListOfClientsOnline, BannedUserName);
                if (BannedUser != null && BannedUser.permission == 0)
                    IsUserBanned = InsertBanToDb.insertUserToDb(BannedUser, BanReason, BanTime);
                else Send.strMessage2 = $"Cannot ban {BannedUserName} because he is admin.";
            }
            else Send.strMessage2 = $"You dont have permission to ban {BannedUserName}";
        }

        private void RemoveBannedUser()
        {
            ListOfClientsOnline.Remove(BannedUser);
            BannedUser.cSocket.Close();
        }

        private void sendMessageToUsers()
        {
            Send.strMessage2 = $"User: {BannedUserName} banned for: {BanReason} untill: {BanTime}";
            SendMessage.ResponseToAll(Client, Send, ListOfClientsOnline);
        }

        public override void Response()
        {
            if (IsUserBanned)
            {
                sendMessageToUsers(); // Banned user will check if his name = banned user name
                RemoveBannedUser();
                Send.strMessage2 = $"You banned: {BannedUserName} for: {BanReason} untill: {BanTime}";
            }
            else Send.strMessage2 = "Cannot ban user with unknown reason.";

            base.Response();
        }
    }
}

