using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ClientSendActiveCode;
using Server.Modules.ResponseMessagesController;
using System;
using System.Collections.Generic;

namespace Server.Controllers
{
    class SendActiveCodeController : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientReSendAckCode;

        string UserName;
        string UserRegisterCode;

        private readonly IActiveCodeGet DatabaseGetSet;
        private readonly ISendActivationCode SendActivationCode;

        public SendActiveCodeController(IActiveCodeGet databaseGetSet, ISendActivationCode sendActivationCode)
        {
            DatabaseGetSet = databaseGetSet;
            SendActivationCode = sendActivationCode;
        }

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

            DatabaseGetSet.GetRegisterCode(UserName);

            string UserEmail = DatabaseGetSet.GetEmail();

            if (UserRegisterCode != null) //user want finish registration
            {
                if (DatabaseGetSet.ComprareCode(UserRegisterCode))
                {
                    if (DatabaseGetSet.DeleteRegistrationCode(UserEmail))
                        Send.strMessage = "Now you can login into application";
                    else
                        Send.strMessage = "Error when Activation contact to support";
                }
                else
                    Send.strMessage = "Activation code not match.";
            }
            else // user want resend activation code to email
            {
                sendActivatonCodeToUserEmail(UserEmail);
                Send.strMessage = "You are not registred";
            }

        }

        private void sendActivatonCodeToUserEmail(string userEmail)
        {
            string RegisterCode = DatabaseGetSet.ReturnRegisterCode();
            if (RegisterCode != null)
            {
                SendActivationCode.Send(UserName, userEmail, RegisterCode);
                Send.strMessage = "Activation code sended.";
                OnClientSendAckCode(UserName, userEmail);
            }
            else
                Send.strMessage = "Your account is already activate.";
        }

        protected virtual void OnClientSendAckCode(string cName, string cEmail)
        {
            ClientReSendAckCode?.Invoke(this, new ClientEventArgs() { clientName = cName, clientEmail = cEmail });
        }
    }
}
