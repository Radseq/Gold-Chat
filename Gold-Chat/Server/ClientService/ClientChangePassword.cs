using CommandClient;
using Server.Interfaces;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientChangePassword : ServerResponds, IBuildResponse
    {
        DataBaseManager db = DataBaseManager.Instance;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelsList = null)
        {
            Received = receive;
            Client = client;
        }

        public void Execute()
        {
            prepareResponse();
            string newPassword = Received.strMessage;

            db.bind("userName", Client.strName);
            string oldPassword = db.singleSelect("SELECT password FROM users WHERE login = @userName");

            if (oldPassword != "")
            {
                if (oldPassword != newPassword)
                    Send.strMessage = updateUserPasswordToDb(newPassword, Client.strName, oldPassword);
                else
                    Send.strMessage = "New and old password are same!";
            }
        }

        private string updateUserPasswordToDb(string newPassword, string userName, string oldPassword)
        {
            db.bind(new string[] { "pass", newPassword, "Login", userName, "oldPass", oldPassword });
            if (db.executeNonQuery("UPDATE users SET password = @pass WHERE login = @Login AND password = @oldPass") > 0)
                return "Your Password has been changed!";
            else
                return "Unknow Error while changing password";
        }
    }
}
