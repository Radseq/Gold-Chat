using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ChangePassword;
using Server.Interfaces.LostPassword;
using Server.Modules.ResponseMessagesController;
using System.Collections.Generic;

namespace Server.Controllers
{
    class ChangePasswordController : ServerResponds, IBuildResponse
    {
        private readonly IUpdateNewPassword UpdateNewPass; //From Lost Password Implementation
        private readonly IGetOldPassword OldPassword;

        public ChangePasswordController(IUpdateNewPassword updateNewPass, IGetOldPassword oldPassword)
        {
            UpdateNewPass = updateNewPass;
            OldPassword = oldPassword;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelsList = null)
        {
            Received = receive;
            Client = client;
        }

        public void Execute()
        {
            prepareResponse();
            string newPassword = Received.strMessage;

            string oldPassword = OldPassword.GetPassword(Client.strName);

            if (oldPassword != "")
            {
                if (oldPassword != newPassword)
                    Send.strMessage = UpdateNewPass.Update(newPassword, Client.strName, oldPassword);
                else
                    Send.strMessage = "New and old password are same!";
            }
        }
    }
}
