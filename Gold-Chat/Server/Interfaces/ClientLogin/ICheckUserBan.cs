using System;

namespace Server.Interfaces.ClientLogin
{
    public interface ICheckUserBan
    {
        string CheckUserBan(Int64 id_user);
    }
}
