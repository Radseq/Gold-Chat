using System;
using System.Collections.Generic;

namespace Server
{
    static class ClientGets
    {
        public static Client getClinetByName(List<Client> ClientList, string clientName)
        {
            foreach (Client client in ClientList)
            {
                if (client.strName == clientName)
                    return client;
            }
            return null;
        }

        public static Client getClientById(List<Client> ClientList, Int64 clientId)
        {
            foreach (Client client in ClientList)
            {
                if (client.id == clientId)
                    return client;
            }
            return null;
        }

        public static List<Client> getClientsWhoEnterToChannel(List<Client> ClientList, string channelName)
        {
            List<Client> clientsInChannel = new List<Client>();
            foreach (Client client in ClientList)
            {
                if (client.enterChannels.Contains(channelName))
                    clientsInChannel.Add(client);
            }
            return clientsInChannel;
        }

        public static Client getClientEnterChannel(List<Client> ClientList, string channelName)
        {
            foreach (Client client in ClientList)
            {
                if (client.enterChannels.Contains(channelName))
                    return client;
            }
            return null;
        }

        public static List<string> getClientIgnoredUsers(List<Client> ClientList, string clientName)
        {
            foreach (Client client in ClientList)
            {
                if (client.strName == clientName)
                    return client.ignoredUsers;
            }
            return null;
        }
    }
}
