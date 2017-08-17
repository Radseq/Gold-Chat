using System;

namespace Server.Interfaces.ClientLogin
{
    interface ICheckUserBan
    {
        string CheckUserBan(Int64 id_user);
    }
}
