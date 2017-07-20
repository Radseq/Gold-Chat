using CommandClient;
using System.Collections.Generic;

namespace Server.ClientService.List
{
    class SendListOfIgnoredUsers : ClientListManager
    {
        public SendListOfIgnoredUsers(Client client, Data send)
        {
            Client = client;
            Send = send;
        }

        public new void Execute()
        {
            //prepareResponse();
            Send.cmdCommand = Command.List;
            Send.strName = null;
            Send.strMessage = "IgnoredUsers";

            db.bind("idUser", Client.id.ToString());
            db.manySelect("SELECT u.login FROM users u, user_ignored ui WHERE ui.id_user_ignored = u.id_user AND ui.id_user = @idUser");
            List<string> ignoredUsers = db.tableToColumn();
            foreach (var ignoredUser in ignoredUsers)
            {
                Send.strMessage2 += ignoredUser + "*";
                Client.ignoredUsers.Add(ignoredUser);
            }

            OnClientList(Send.strMessage, Send.strMessage2);
        }
    }
}
