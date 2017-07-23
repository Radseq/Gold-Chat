using CommandClient;
using Server.Interfaces;
using System.Collections.Generic;

namespace Server.ClientService.List
{
    class SendChannelList : ClientListManager, IBuildResponse
    {
        public SendChannelList(List<Channel> channelListe, Data send)
        {
            ChannelsList = channelListe;
            Send = send;
        }

        /// <summary>
        /// Sending to the user the list of all channels
        /// </summary>
        /// <param name="conClient">User</param>
        /// <param name="msgToSend">List of channels</param>
        public new void Execute()
        {
            //prepareResponse();
            Send.cmdCommand = Command.List;
            Send.strName = null;
            Send.strMessage = "Channel";
            Send.strMessage2 = null;

            foreach (Channel channel in ChannelsList)
                Send.strMessage2 += channel.ChannelName + "*";

            OnClientList(Send.strMessage, Send.strMessage2);
        }
    }
}
