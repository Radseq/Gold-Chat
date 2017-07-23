using CommandClient;
using System.Collections.Generic;

namespace Server.Interfaces
{
    public interface IBuildResponse
    {
        void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null);
        void Execute();
        void Response();
    }
}
