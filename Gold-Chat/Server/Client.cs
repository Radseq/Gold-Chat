using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Client
    {
        public bool KeepProcessing { get; set; }
        public Socket cSocket { get; set; }
        public int id { get; set; }
        public IPEndPoint addr { get; set; }
        public string strName;  //Name by which the user logged into the chat room
    }
}
