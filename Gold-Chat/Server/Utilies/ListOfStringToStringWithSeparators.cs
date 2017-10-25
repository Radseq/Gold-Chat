using Server.Interfaces.Utilities;
using System.Collections.Generic;

namespace Server.Utilies
{
    class ListOfStringToStringWithSeparators : ISeparateListOfStringToString
    {
        public string separate(List<string> list)
        {
            string returnItems = null;
            foreach (var item in list)
                returnItems += $"{item}*";

            return returnItems;
        }

        public string separate(List<Channel> list)
        {
            string returnItems = null;

            foreach (Channel item in list)
                returnItems += $"{item.ChannelName}*";

            return returnItems;
        }

        public string separate(List<Client> list)
        {
            string returnItems = null;

            foreach (Client item in list)
                returnItems += $"{item.strName}*";

            return returnItems;
        }
    }
}
