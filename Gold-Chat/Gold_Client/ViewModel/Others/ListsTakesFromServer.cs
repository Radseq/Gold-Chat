using CommandClient;
using System;

namespace Gold_Client.ViewModel.Others
{
    class InformServerToSendUserLists
    {
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        int second = 0;

        public InformServerToSendUserLists()
        {
            dispatcherTimer.Tick += new EventHandler(SendToServerAsk);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void SendToServerAsk(object sender, EventArgs e)
        {
            if (second == 0)
                clientSendToServer.SendToServer(Command.List);
            else if (second == 1)
                clientSendToServer.SendToServer(Command.List, "IgnoredUsers"); // The names of all ignored users
            else if (second == 2)
                clientSendToServer.SendToServer(Command.List, "ChannelsJoined"); // The names of all channels that he have joined
            else if (second == 3)
                clientSendToServer.SendToServer(Command.List, "Friends"); // The names of all users that he have in friend list
            else if (second == 4)
                clientSendToServer.SendToServer(Command.List, "Channel"); // The names of all channels that he have acces to
            if (second > 4)
                dispatcherTimer.Stop();
            second = second + 1;
        }
    }
}
