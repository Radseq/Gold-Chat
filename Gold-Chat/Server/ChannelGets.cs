using System;
using System.Collections.Generic;

namespace Server
{
    static class ChannelGets
    {
        public static Channel getChannelByName(List<Channel> ChannelsList, string channelName)
        {
            foreach (Channel channel in ChannelsList)
            {
                if (channel.ChannelName == channelName)
                    return channel;
            }
            return null;
        }

        public static Channel getChannelByFounderId(List<Channel> ChannelsList, Int64 userFounderId)
        {
            foreach (Channel channel in ChannelsList)
            {
                if (channel.FounderiD == userFounderId)
                    return channel;
            }
            return null;
        }

        public static List<string> getChannelUsers(List<Channel> ChannelsList, string channelName)
        {
            foreach (Channel channel in ChannelsList)
            {
                if (channel.ChannelName == channelName)
                    return channel.Users;
            }
            return null;
        }
    }
}
