using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ClientRegistration;
using Server.Modules.ResponseMessagesController;
using System;
using System.Collections.Generic;

namespace Server.Controllers
{
    class RegistrationController : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientRegistrationEvent;

        private readonly ISendRegistrationMessage RegisterMessage;
        private readonly ISaveRegisterUser InsertRegisterUser;
        private readonly ICheckForAlreadyExistsUser CheckForExistsUser;

        public RegistrationController(ISendRegistrationMessage registerMessage, ISaveRegisterUser insertRegisterUser, ICheckForAlreadyExistsUser checkForExistsUser)
        {
            RegisterMessage = registerMessage;
            InsertRegisterUser = insertRegisterUser;
            CheckForExistsUser = checkForExistsUser;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
        }

        public void Execute()
        {
            prepareResponse();
            string login = Received.strName;
            string userPassword = Received.strMessage;
            string userEmail = Received.strMessage2;

            CheckForExistsUser.GetData(login, userEmail);

            if (CheckForExistsUser.isLoginExists())
                Send.strMessage = "Your login exists, try other one";
            else if (CheckForExistsUser.isEmailExists())
                Send.strMessage = "Your email exists, try other one";
            else if (CheckForExistsUser.isUserAlreadyRegister())
                Send.strMessage = "You have already register, on next login you will be ask for register key";
            else
                RegisterUser(login, userPassword, userEmail);
        }

        private void RegisterUser(string userName, string userPassword, string userEmail)
        {
            string registrationCode = Guid.NewGuid().ToString();

            bool created = InsertRegisterUser.insertUserToDb(userName, userPassword, userEmail, registrationCode);

            if (created)
                IsRegistrationMessageSended(userName, userEmail, registrationCode);
            else
                Send.strMessage = "Account NOT created with unknown reason.";
        }

        private void IsRegistrationMessageSended(string userName, string userEmail, string registrationCode)
        {
            if (RegisterMessage.Send(userName, userEmail, registrationCode))
            {
                Send.strMessage = "You has been registered";
                OnClientRegistration(userName, userEmail);
            }
        }

        protected virtual void OnClientRegistration(string cName, string cEmail)
        {
            ClientRegistrationEvent?.Invoke(this, new ClientEventArgs() { clientName = cName, clientEmail = cEmail });
        }
    }
}