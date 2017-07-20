using CommandClient;
using Server.ResponseMessages;
using System;
using System.Collections.Generic;

namespace Server
{
    class ClientLogin : ServerResponds, IClient, IPrepareRespond
    {
        public event EventHandler<ClientEventArgs> clientLoginEvent;

        DataBaseManager db = DataBaseManager.Instance;
        EmailSender emailSender = EmailSender.Instance;

        private List<Client> ListOfClientsOnline;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelsList = null)
        {
            Received = receive;
            Client = client;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            prepareResponse();

            string userName = Received.strName;
            string userPassword = Received.strMessage;
            string loginNotyfiUser = Received.strMessage2;

            db.bind(new string[] { "@userName", userName, "@password", userPassword });
            db.manySelect("SELECT register_id, email, id_user, login, permission FROM users WHERE login = @userName AND password = @password");
            string[] query = db.tableToRow();

            if (query == null || userName != query[3])
                Send.strMessage = "Wrong login or password";
            else if (query[0] != "") // query[0] Activation code must be "" if user want to login
            {
                // User wont send activation code
                Send.cmdCommand = Command.SendActivationCode;
                Send.strMessage = "You must activate your account first.";
            }
            else
            {
                string ban = CheckUserBan(Int64.Parse(query[2]));
                if (ban == null)
                {
                    ClientSuccesfullyLogIn(query);
                    if (loginNotyfiUser == "1") // User wants to be notyficated when login to account
                        emailSender.SendEmail(Client.strName, query[1], "Gold Chat: Login Notyfication", "You have login: " + DateTime.Now.ToString("dd:MM On HH:mm:ss") + " To Gold Chat Account.");

                }
                else Send.strMessage = "You are banned untill " + ban;
            }
        }

        private void ClientSuccesfullyLogIn(string[] query)
        {
            Client.id = Int64.Parse(query[2]);
            Client.permission = Int16.Parse(query[4]);

            // All is correct so user can use app
            Client.strName = query[3];
            Send.strMessage = "You are succesfully Log in";
            Send.strMessage2 = Client.permission.ToString(); //send to user if he is admin, used to visibility some oprions and rights on client side
            Client.enterChannels = new List<string>(); // Init of channels whitch i joined
            Client.ignoredUsers = new List<string>(); // Init of ignored users
            OnClientLogin(Client.strName, Client.addr.Address.ToString(), Client.addr.Port.ToString()); // Server OnClientLogin occur only when succes program.cs -> OnClientLogin
        }

        private string CheckUserBan(Int64 id_user)
        {
            db.bind("IdUser", id_user.ToString());
            string endBanDateFromDB = db.singleSelect("SELECT end_ban FROM user_bans WHERE id_user = @IdUser");

            if (endBanDateFromDB != "")
            {
                DateTime dt1 = DateTime.Parse(endBanDateFromDB);
                DateTime dt2 = DateTime.Now;

                if (dt1.Date > dt2.Date)
                    return endBanDateFromDB;
                else
                    return null;
            }
            return null;
        }

        public void Response()
        {
            Respond();
            if (Send.strMessage == "You are succesfully Log in") // Client succesfully login and rest of online users will got this msg below
            {
                Send.strMessage = "<<<" + Received.strName + " has joined the room>>>";
                SendMessageToAll sendMsgToAll = new SendMessageToAll(Client, Send, ListOfClientsOnline);
                sendMsgToAll.ResponseToAll();
            }
        }

        protected virtual void OnClientLogin(string cName, string cIpadress, string cPort)
        {
            clientLoginEvent?.Invoke(this, new ClientEventArgs() { clientName = cName, clientIpAdress = cIpadress, clientPort = cPort });
        }
    }
}
