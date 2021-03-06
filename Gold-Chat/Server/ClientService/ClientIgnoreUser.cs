﻿using CommandClient;
using Server.Interfaces;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientIgnoreUser : ServerResponds, IBuildResponse
    {
        DataBaseManager db = DataBaseManager.Instance;

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
            Send.strMessage3 = userName;

            string idUser = SelectUserToIgnore(userName);

            if (idUser != null)
            {
                if (type == "AddIgnore")
                {
                    if (!Client.ignoredUsers.Contains(userName))
                        Send.strMessage2 = addIgnoredUserToDb(userName, idUser);
                    else
                        Send.strMessage2 = "Cannot ignore " + userName + " because already ignored!";
                }
                else if (type == "DeleteIgnore")
                {
                    if (Client.ignoredUsers.Contains(userName))
                        Send.strMessage2 = deleteIgnoredUserFromDb(userName, idUser);
                    else
                        Send.strMessage2 = "Cannot delete ignore from " + userName + " because not ignored!";
                }
                else Send.strMessage2 = "There is only Add or Delete users option";
            }
            else Send.strMessage2 = "Contact to admin because is too many users with nick" + userName;
        }

        private string SelectUserToIgnore(string userName)
        {
            db.bind("IgnoreName", userName);
            return db.singleSelect("SELECT id_user FROM users WHERE login = @IgnoreName");
        }

        private string addIgnoredUserToDb(string userName, string users)
        {
            db.bind(new string[] { "idUser", Client.id.ToString(), "idUserIgnored", users });
            if (db.executeNonQuery("INSERT INTO user_ignored (id_user, id_user_ignored) " + "VALUES (@idUser, @idUserIgnored)") > 0)
            {
                Client.ignoredUsers.Add(userName);
                return "You are now ignore: " + userName;
                //OnClientIgnoreUser(client.strName, friendName);
            }
            else
                return "Cannot ignore " + userName + " unknown reason";
        }

        private string deleteIgnoredUserFromDb(string userName, string users)
        {
            db.bind(new string[] { "idUser", Client.id.ToString(), "idUserIgnored", users });

            if (db.executeNonQuery("DELETE FROM user_ignored WHERE id_user = @idUser AND id_user_ignored = @idUserIgnored") > 0)
            {
                Client.ignoredUsers.Remove(userName);
                return "You are delete from ignore list user: " + userName;
            }
            else
                return "Cannot delete ignore from " + userName + " unknown reason";
        }
    }
}
