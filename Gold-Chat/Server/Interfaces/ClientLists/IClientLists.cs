using System;
using System.Collections.Generic;

namespace Server.Interfaces.ClientLists
{
    interface IClientLists
    {
        List<string> GetList(Int64 id_user);
    }
}
