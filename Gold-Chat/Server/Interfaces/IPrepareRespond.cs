using CommandClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.ResponseMessages
{
    interface IPrepareRespond
    {
        void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null);
        void Execute();
    }
}
