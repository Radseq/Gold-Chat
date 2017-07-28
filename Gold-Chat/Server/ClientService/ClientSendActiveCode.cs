using CommandClient;
using Server.Interfaces;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientSendActiveCode : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientReSendAckCode;

        DataBaseManager db = DataBaseManager.Instance;
        EmailSender emailSender = new EmailSender();

        string UserName;
        string UserRegisterCode;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
        }

        public void Execute()
        {
            prepareResponse();
            UserName = Received.strName;
            UserRegisterCode = Received.strMessage;

            if (UserRegisterCode != null)
            {
                string[] query = GetUserRegisterIDAndEmailUsingReg();
                if (query != null)
                {
                    if (query[0] == UserRegisterCode)
                        UpdateRegistrationCode(query[1]);
                    else
                        Send.strMessage = "Activation code not match.";
                }
                else Send.strMessage = "No user with that activation code";
            }
            else
            {
                string[] query = GetUserRegisterIDAndEmail();
                if (query != null)
                    sendActivatonCodeToUserEmail(query[0], query[1]);
                else Send.strMessage = "You are not registred";
            }
        }

        private string[] GetUserRegisterIDAndEmailUsingReg()
        {
            db.bind(new string[] { "RegId", UserRegisterCode, "Login", UserName });
            db.manySelect("SELECT register_id, email FROM users WHERE register_id = @RegId AND login = @Login");
            return db.tableToRow();
        }

        private string[] GetUserRegisterIDAndEmail()
        {
            db.bind("login", UserName);
            db.manySelect("SELECT register_id, email FROM users WHERE login = @login");
            return db.tableToRow();
        }

        private void UpdateRegistrationCode(string userEmail)
        {
            db.bind(new string[] { "reg_id", "", "email", userEmail });
            int updated = db.executeNonQuery("UPDATE users SET register_id = @reg_id WHERE email = @email");

            if (updated > 0)
                Send.strMessage = "Now you can login into application";
            else
                Send.strMessage = "Error when Activation contact to support";
        }

        private void sendActivatonCodeToUserEmail(string regCode, string userEmail)
        {
            if (regCode != "")
            {
                emailSender.SetProperties(UserName, userEmail, "Gold Chat: Resended Register Code", "Here is your activation code: " + regCode);
                emailSender.SendEmail();
                Send.strMessage = "Activation code sended.";
                OnClientSendAckCode(UserName, userEmail);
            }
            else
                Send.strMessage = "You account is activ.";
        }

        protected virtual void OnClientSendAckCode(string cName, string cEmail)
        {
            ClientReSendAckCode?.Invoke(this, new ClientEventArgs() { clientName = cName, clientEmail = cEmail });
        }
    }
}
