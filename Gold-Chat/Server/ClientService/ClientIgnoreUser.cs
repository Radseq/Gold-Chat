using CommandClient;
using Server.ResponseMessages;
using System.Collections.Generic;
using System;

namespace Server.ClientService
{
    class ClientIgnoreUser : ServerResponds, IPrepareRespond
    {
        DataBaseManager db = DataBaseManager.Instance;
        EmailSender emailSender = EmailSender.Instance;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
        }

        public void Execute()
        {
            prepareResponse();
            string type = Received.strMessage;
            string userName = Received.strMessage2;

            db.bind("IgnoreName", userName);
            db.manySelect("SELECT id_user FROM users WHERE login = @IgnoreName");
            List<string> users = db.tableToColumn();

            if (users.Capacity > 1)
            {
                Send.strMessage3 = userName;

                if (type == "AddIgnore")
                {
                    if (!Client.ignoredUsers.Contains(userName))
                        Send.strMessage2 = addIgnoredUserToDb(userName, users);
                    else
                        Send.strMessage2 = "Cannot ignore " + userName + " because already ignored!";
                }
                else if (type == "DeleteIgnore")
                {
                    if (Client.ignoredUsers.Contains(userName))
                        Send.strMessage2 = deleteIgnoredUserFromDb(userName, users);
                    else
                        Send.strMessage2 = "Cannot delete ignore from " + userName + " because not ignored!";
                }
                else Send.strMessage2 = "There is only Add or Delete users option";
            }
            else Send.strMessage2 = "Contact to admin because is too many users with nick" + userName;
        }

        private string addIgnoredUserToDb(string userName, List<string> users)
        {
            db.bind(new string[] { "idUser", Client.id.ToString(), "idUserIgnored", users[0] });
            if (db.delUpdateInsertDb("INSERT INTO user_ignored (id_user, id_user_ignored) " + "VALUES (@idUser, @idUserIgnored)") > 0)
            {
                Client.ignoredUsers.Add(userName);
                return "You are now ignore: " + userName;
                //OnClientIgnoreUser(client.strName, friendName);
            }
            else
                return "Cannot ignore " + userName + " unknown reason";
        }

        private string deleteIgnoredUserFromDb(string userName, List<string> users)
        {
            db.bind(new string[] { "idUser", Client.id.ToString(), "idUserIgnored", users[0] });

            if (db.delUpdateInsertDb("DELETE FROM user_ignored WHERE id_user = @idUser AND id_user_ignored = @idUserIgnored") > 0)
            {
                Client.ignoredUsers.Remove(userName);
                return "You are delete from ignore list user: " + userName;
            }
            else
                return "Cannot delete ignore from " + userName + " unknown reason";
        }
    }
}
