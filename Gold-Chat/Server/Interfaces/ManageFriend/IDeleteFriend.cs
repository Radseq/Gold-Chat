using System;

namespace Server.Interfaces.ManageFriend
{
    interface IDeleteFriend
    {
        bool Delete(Int64 clientId, Int64 friendId);
    }
}
