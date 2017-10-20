using System;

namespace Server.Interfaces.IgnoreUser
{
    interface IAddIgnoredUser
    {
        bool Add(Int64 ClientId, string nickofIgnoredUser);
    }
}
