using System.Net.Sockets;

namespace Server
{
    public class Client
    {
        public bool KeepProcessing { get; set; }
        public Socket cSocket { get; set; }
        public string strName;  //Name by which the user logged into the chat room
    }
}
