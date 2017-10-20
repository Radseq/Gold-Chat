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
                returnItems += item + "*";

            return returnItems;
        }
    }
}
