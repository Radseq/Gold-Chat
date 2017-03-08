using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Client
    {
        public Socket cSocket { get; set; }
        public int id { get; set; }
        public int permission { get; set; }
        public IPEndPoint addr { get; set; }
        public string strName;  //Name by which the user logged into the chat room
        public List<string> enterChannels { get; set; }
        public List<string> ignoredUsers { get; set; }
    }
}
