using System;

namespace Server.Interfaces.IgnoreUser
{
    interface IDeleteIgnoredUser
    {
        bool Delete(Int64 ClientId, string nickofIgnoredUser);
    }
}
