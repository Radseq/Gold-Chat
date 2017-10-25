using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ClientLogin;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using System;
using System.Collections.Generic;

namespace Server.Controllers
{
    class LoginController : ServerResponds, IClient, IBuildResponse
    {
        private readonly ICheckUserBan CheckBan;
        private readonly ISendLoginNotyfication SendLoginNotyfication;
        private readonly IGetClientProperties GetClientProperties;
        private readonly ISendMessageToAll SendMessage;

        public LoginController(ICheckUserBan checkBan, ISendLoginNotyfication sendLoginNotyfication,
            IGetClientProperties getClientProperties, ISendMessageToAll sendMessage)
        {
            CheckBan = checkBan;
            SendLoginNotyfication = sendLoginNotyfication;
            GetClientProperties = getClientProperties;
            SendMessage = sendMessage;
        }

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
            // todo select activation code from new table
            string[] query = GetClientProperties.GetUserProperties(userName, userPassword); //GetUserInformationFromDB(userName, userPassword);
            if (query == null || userName != query[3])
                Send.strMessage = "Wrong login or password";
            else if (!string.IsNullOrEmpty(query[0]))
            {
                // User wont send activation code
                Send.cmdCommand = Command.SendActivationCode;
                Send.strMessage = "You must activate your account first.";
            }
            else
            {
                string ban = CheckBan.CheckUserBan(Int64.Parse(query[2])); //CheckUserBan(Int64.Parse(query[2]));
                if (ban == null)
                {
                    ClientSuccesfullyLogIn(query);
                    userEmailNotification(query[1]);
                }
                else Send.strMessage = $"You are banned untill {ban}";
            }
        }

        private void userEmailNotification(string email)
        {
            string loginNotyfiUser = Received.strMessage2;
            if (loginNotyfiUser == "1") // User wants to be notyficated when login into account
                SendLoginNotyfication.Send(Client.strName, email);
        }

        private void ClientSuccesfullyLogIn(string[] query)
        {
            SetClientProperties(query);

            Send.strMessage = "You are succesfully Log in";
            Send.strMessage2 = Client.permission.ToString();
            OnClientLogin(Client.strName, Client.addr.Address.ToString(), Client.addr.Port.ToString()); // Server OnClientLogin occur only when succes program.cs -> OnClientLogin
        }

        private void SetClientProperties(string[] query)
        {
            Client.id = Int64.Parse(query[2]);
            Client.permission = Int16.Parse(query[4]);
            Client.strName = query[3];
            Client.enterChannels = new List<string>(); // Init of channels whitch i joined
            Client.ignoredUsers = new List<string>(); // Init of ignored users
        }

        public override void Response()
        {
            base.Response();

            ResponseToAll();
        }

        private void ResponseToAll()
        {
            if (Send.strMessage == "You are succesfully Log in") // Client succesfully login and rest of online users will got this msg below
            {
                Send.strMessage = $"<<<{Received.strName} has joined the room>>>";
                SendMessage.ResponseToAll(Client, Send, ListOfClientsOnline);
            }
        }

        protected virtual void OnClientLogin(string cName, string cIpadress, string cPort)
        {
            // clientLoginEvent?.Invoke(this, new ClientEventArgs() { clientName = cName, clientIpAdress = cIpadress, clientPort = cPort });
        }
    }
}
