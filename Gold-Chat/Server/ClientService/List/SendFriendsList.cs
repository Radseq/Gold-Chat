using CommandClient;
using Server.ResponseMessages;
using System.Collections.Generic;

namespace Server.ClientService.List
{
    class SendFriendsList : ClientListManager
    {

        public SendFriendsList(Client client, Data send)
        {
            Client = client;
            Send = send;
        }

        public new void Execute()
        {
            //prepareResponse();
            Send.cmdCommand = Command.List;
            Send.strName = null;
            Send.strMessage = "Friends";

            db.bind("idUser", Client.id.ToString());
            db.manySelect("SELECT u.login FROM users u, user_friend uf WHERE uf.id_friend = u.id_user AND uf.id_user = @idUser");
            List<string> query = db.tableToColumn();

            Send.strMessage2 = foreachInQuery(query);

            OnClientList(Send.strMessage, Send.strMessage2);
        }
    }
}
