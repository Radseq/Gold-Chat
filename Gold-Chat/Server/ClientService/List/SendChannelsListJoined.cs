using CommandClient;
using System.Collections.Generic;

namespace Server.ClientService.List
{
    class SendChannelsListJoined : ClientListManager
    {
        public SendChannelsListJoined(Client client, Data send)
        {
            Client = client;
            Send = send;
        }

        // Send to user, list of channels that he joined before
        public new void Execute()
        {
            //prepareResponse();
            Send.strName = null;

            db.bind("idUser", Client.id.ToString());
            db.manySelect("SELECT c.channel_name FROM channel c, user_channel uc WHERE uc.id_channel = c.id_channel AND uc.id_user = @idUser");
            List<string> query = db.tableToColumn();

            Send.strMessage2 = foreachInQuery(query);

            OnClientList(Send.strMessage, Send.strMessage2);
        }
    }
}
