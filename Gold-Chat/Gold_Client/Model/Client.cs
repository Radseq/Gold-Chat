using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Gold_Client.Model
{
    public class Client
    {
        public Socket cSocket { get; set; }
        //public Int64 id { get; set; }
        public int permission { get; set; }
        public IPEndPoint addr { get; set; }
        public string strName;  // Name by which the user logged into the chat room
        //public List<string> enterChannels { get; set; }
        //public List<string> ignoredUsers { get; set; }

        public event EventHandler OnBufferChange;

        private byte[] buffer = new byte[1024];
        public byte[] Buffer
        {
            get { return buffer; }
            set
            {
                buffer = value;
                OnBufferChange?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
