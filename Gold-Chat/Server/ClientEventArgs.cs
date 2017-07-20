using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ClientEventArgs : EventArgs
    {
        public string clientName { get; set; }
        public string clientMessageToSend { get; set; }
        public string clientMessageTwoToSend { get; set; }
        public string clientMessageReciv { get; set; }
        public Socket clientSocket { get; set; }
        public string clientIpAdress { get; set; }
        public string clientPort { get; set; }
        public string clientEmail { get; set; }
        public int clientCommand { get; set; }
        public string clientFriendName { get; set; }
        public string clientChannelName { get; set; }
        public string clientNameChannel { get; set; } //client name with joined to chanel
    }
}
