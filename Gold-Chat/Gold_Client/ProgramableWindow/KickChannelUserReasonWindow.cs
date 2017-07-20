using CommandClient;
using Gold_Client.ViewModel;

namespace Gold_Client.ProgramableWindow
{
    static class ChannelKickUserReason
    {
        static ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        public static string ChannelName;
        public static string UserName;

        public static void OnClickOrEnter(object sender, string reason)
        {
            if (ChannelName == null)
                clientSendToServer.SendToServer(Command.kick, UserName, reason);
            else if (ChannelName != null)
                clientSendToServer.SendToServer(Command.kickUserChannel, UserName, reason, ChannelName);
        }
    }
}
