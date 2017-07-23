using System.Collections.Generic;

namespace Server.Interfaces
{
    interface IClientList
    {
        List<Client> ListOfClientsOnline { get; set; }
    }
}
