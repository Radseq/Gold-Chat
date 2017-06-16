using System.Net;
using System.Net.Sockets;

namespace Gold_Client.Model
{
    public class Client
    {
        // Singleton
        static Client instance = null;
        static readonly object padlock = new object();

        // Singleton
        public static Client Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new Client();

                    return instance;
                }
            }
        }

        public Socket cSocket { get; set; }
        public int permission { get; set; }
        public IPEndPoint addr { get; set; }
        public string strName;



        private byte[] buffer = new byte[1024];
        public byte[] Buffer
        {
            get
            {
                return buffer;
            }
            set
            {
                buffer = value;
            }
        }

        //public static Action<object, EventArgs> OnBufferChange { get; internal set; }
    }
}
