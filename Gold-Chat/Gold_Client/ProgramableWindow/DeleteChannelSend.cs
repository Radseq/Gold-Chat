﻿using CommandClient;
using Gold_Client.ViewModel;

namespace Gold_Client.ProgramableWindow
{
    static class DeleteChannelSend
    {
        static ClientSendToServer clientSendToServer = ClientSendToServer.Instance;

        public static string ChannelName;

        public static void OnClickOrEnter(object sender, System.Security.SecureString e)
        {
            clientSendToServer.SendToServer(Command.deleteChannel, ChannelName, clientSendToServer.CalculateChecksum(new System.Net.NetworkCredential(string.Empty, e).Password));
        }
    }
}
