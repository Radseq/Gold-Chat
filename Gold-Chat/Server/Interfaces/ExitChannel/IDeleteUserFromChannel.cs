using System;

namespace Server.Interfaces.ExitChannel
{
    interface IDeleteUserFromChannel
    {
        bool Delete(int idChannelDb, Int64 ClientId);
    }
}
