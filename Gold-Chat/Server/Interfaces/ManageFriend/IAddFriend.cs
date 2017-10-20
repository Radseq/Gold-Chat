using System;

namespace Server.Interfaces.ManageFriend
{
    interface IAddFriend
    {
        bool Add(Int64 clientId, Int64 friendId);
    }
}
