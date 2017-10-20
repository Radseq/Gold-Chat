using CommandClient;
using Server.Interfaces;
using Server.Interfaces.IgnoreUser;
using Server.Modules.ResponseMessagesController;
using System.Collections.Generic;

namespace Server.Controllers
{
    class IgnoreUserController : ServerResponds, IBuildResponse
    {
        private readonly IAddIgnoredUser AddIgnoredUser;
        private readonly IDeleteIgnoredUser DeleteIgnoredUser;

        public IgnoreUserController(IAddIgnoredUser addIgnoredUser, IDeleteIgnoredUser deleteIgnoredUser)
        {
            AddIgnoredUser = addIgnoredUser;
            DeleteIgnoredUser = deleteIgnoredUser;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
        }

        public void Execute()
        {
            prepareResponse();
            string type = Received.strMessage;
            string userNameToBeIgnored = Received.strMessage2;
            Send.strMessage3 = userNameToBeIgnored;

            if (type == "AddIgnore")
            {
                if (!Client.ignoredUsers.Contains(userNameToBeIgnored))
                {
                    if (AddIgnoredUser.Add(Client.id, userNameToBeIgnored))
                    {
                        Client.ignoredUsers.Add(userNameToBeIgnored);
                        Send.strMessage2 = "You are now ignore: " + userNameToBeIgnored;
                    }
                    else
                    {
                        Send.strMessage2 = "Cannot ignore " + userNameToBeIgnored + " unknown reason";
                    }
                }
                else
                    Send.strMessage2 = "Cannot ignore " + userNameToBeIgnored + " because already ignored!";
            }
            else if (type == "DeleteIgnore")
            {
                if (Client.ignoredUsers.Contains(userNameToBeIgnored))
                {
                    if (DeleteIgnoredUser.Delete(Client.id, userNameToBeIgnored))
                    {
                        Client.ignoredUsers.Remove(userNameToBeIgnored);
                        Send.strMessage2 = "You are delete from ignore list user: " + userNameToBeIgnored;
                    }
                    else
                    {
                        Send.strMessage2 = "Cannot delete ignore from " + userNameToBeIgnored + " unknown reason";
                    }
                }
                else
                    Send.strMessage2 = "Cannot delete ignore from " + userNameToBeIgnored + " because not ignored!";
            }
            else Send.strMessage2 = "There is only Add or Delete users option";
            //}
            //else Send.strMessage2 = "Contact to admin because is too many users with nick" + userNameToBeIgnored;
        }
    }
}
