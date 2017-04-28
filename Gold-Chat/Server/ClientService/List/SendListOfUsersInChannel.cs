using CommandClient;
using System.Collections.Generic;

namespace Server.ClientService.List
{
    class SendListOfUsersInChannel : ClientListManager
    {
        public SendListOfUsersInChannel(List<Channel> channelList, Data send, Data received)
        {
            ChannelList = channelList;
            Send = send;
            Received = received;
        }

        // When user enter to channel, he send to server ask for users already in this channel
        public new void Execute()
        {
            //prepareResponse();
            foreach (Channel channel in ChannelList)
            {
                if (channel.ChannelName == Received.strMessage2)
                {
                    foreach (string userName in channel.Users)
                        Send.strMessage3 += userName + "*";
                }
            }
        }
    }
}
