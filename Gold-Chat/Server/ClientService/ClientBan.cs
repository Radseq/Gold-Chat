using CommandClient;
using Server.Interfaces;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientBan : ServerResponds, IClient, IBuildResponse
    {
        DataBaseManager db = DataBaseManager.Instance;
        List<Client> ListOfClientsOnline;

        private string BanTime;
        private string BanReason;
        private string BannedUserName;
        private Client BannedUser;
        private bool IsUserBanned;

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
                    IsUserBanned = insertUserBanToDb(BannedUser, BanReason, BanTime);
                else Send.strMessage2 = "Cannot ban " + BannedUserName + " because he is admin.";
            }
            else Send.strMessage2 = "You dont have permission to ban " + BannedUserName;
        }

        private bool insertUserBanToDb(Client client, string banReason, string Bantime)
        {
            db.bind(new string[] { "idUser", client.id.ToString(), "BanReason", banReason, "StartBanDateTime", Utilities.getDataTimeNow(), "EndBanDateTime", Bantime });

            if (db.delUpdateInsertDb("INSERT INTO user_bans (id_user, reason, start_ban, end_ban) " + "VALUES (@idUser, @BanReason, @StartBanDateTime, @EndBanDateTime)") > 0)
                return true;
            Send.strMessage2 = "Cannot ban user with unknown reason.";
            return false;
        }

        private void RemoveBannedUser()
        {
            ListOfClientsOnline.Remove(BannedUser);
            BannedUser.cSocket.Close();
        }

        private void sendMessageToUsers()
        {
            SendMessageToAll sendToAll = new SendMessageToAll(Client, Send, ListOfClientsOnline);
            sendToAll.Send.strMessage2 = "User: " + BannedUserName + " banned for: " + BanReason + " untill: " + BanTime;
            sendToAll.ResponseToAll();
        }

        public override void Response()
        {
            if (IsUserBanned)
            {
                sendMessageToUsers(); // Banned user will check if his name = banned user name
                RemoveBannedUser();
                Send.strMessage2 = "You banned: " + BannedUserName + " for: " + BanReason + " untill: " + BanTime;
            }

            base.Response();
        }
    }
}
