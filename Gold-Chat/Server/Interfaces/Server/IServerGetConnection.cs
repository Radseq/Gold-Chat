using System;
using System.Net.Sockets;

namespace Server.Interfaces.Server
{
    interface IServerGetConnection
    {
        void acceptCallback(IAsyncResult ar);
        void getConnection(Socket sock);
    }
}
