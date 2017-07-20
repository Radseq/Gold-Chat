using System.Collections.Generic;

namespace Server
{
    interface IClientList
    {
        List<Client> ListOfClientsOnline { get; set; }
    }
}
