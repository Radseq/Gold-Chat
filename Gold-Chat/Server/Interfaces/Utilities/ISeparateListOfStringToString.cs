using System.Collections.Generic;

namespace Server.Interfaces.Utilities
{
    interface ISeparateListOfStringToString
    {
        string separate(List<string> list);
        string separate(List<Channel> list);
        string separate(List<Client> list);
    }
}
