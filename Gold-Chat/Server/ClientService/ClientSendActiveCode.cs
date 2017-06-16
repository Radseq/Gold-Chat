using CommandClient;
using Server.ResponseMessages;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class ClientSendActiveCode : ServerResponds, IPrepareRespond
    {
        public event EventHandler<ClientEventArgs> ClientReSendAckCode;

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
            string userName = Received.strName;
            string userRegisterCode = Received.strMessage;

            if (userRegisterCode != null)
            {
                db.bind(new string[] { "RegId", userRegisterCode, "Login", userName });
                db.manySelect("SELECT register_id, email FROM users WHERE register_id = @RegId AND login = @Login");
                string[] query = db.tableToRow();
                if (query != null)
                    clientSendCorrectActivationCode(query[0], userRegisterCode, query[1]);
                else Send.strMessage = "No user with that activation code";
            }
            else
            {
                db.bind("login", userName);
                db.manySelect("SELECT register_id, email FROM users WHERE login = @login");
                string[] query = db.tableToRow();
                if (query != null)
                    sendActivatonCodeToUserEmail(query[0], userName, query[1]);
                else Send.strMessage = "You are not registred";
            }
        }

        private void clientSendCorrectActivationCode(string regCode, string userRegisterCode, string userEmail)
        {
            if (regCode == userRegisterCode)
            {
                db.bind(new string[] { "reg_id", "", "email", userEmail });
                int updated = db.delUpdateInsertDb("UPDATE users SET register_id = @reg_id WHERE email = @email");

                if (updated > 0)
                    Send.strMessage = "Now you can login in to application";
                else
                    Send.strMessage = "Error when Activation contact to support";
            }
            else
                Send.strMessage = "Activation code not match.";
        }

        private void sendActivatonCodeToUserEmail(string regCode, string userName, string userEmail)
        {
            if (regCode != "")
            {
                emailSender.SendEmail(userName, userEmail, "Gold Chat: Resended Register Code", "Here is your activation code: " + regCode);
                Send.strMessage = "Activation code sended.";
                OnClientSendAckCode(userName, userEmail);
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
