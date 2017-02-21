using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Client
    {
        public Socket cSocket { get; set; }
        public int id { get; set; }
        public IPEndPoint addr { get; set; }
        public string strName;  //Name by which the user logged into the chat room
        public List<string> channels { get; set; } //on user login give him list of channel that he joined
    }
}
