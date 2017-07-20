using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Client
    {
        public Socket cSocket { get; set; }
        public Int64 id { get; set; }
        public int permission { get; set; }
        public IPEndPoint addr { get; set; }
        public string strName;
        public List<string> enterChannels { get; set; }
        public List<string> ignoredUsers { get; set; }
    }
}
