using System;

namespace Server
{
    static class Utilities
    {
        public static string getDataTimeNow()
        {
            DateTime now = DateTime.Now;
            return now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
