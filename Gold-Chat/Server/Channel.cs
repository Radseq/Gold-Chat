using System;
using System.Collections.Generic;

namespace Server
{

    /// <summary>
    /// Users added when:
    ///     channelEnter
    /// Users deleted when:
    ///     clientLogout
    ///     channelLeave
    /// </summary>
    class Channel
    {
        string channelName;
        List<string> users = new List<string>(); // Using to send userlist, when new user enter to this channel
        Int64 founderiD;

        public Channel(string name, Int64 creatorId)
        {
            channelName = name;
            founderiD = creatorId;
        }

        public List<string> Users
        {
            get
            {
                return users;
            }

            set
            {
                users = value;
            }
        }

        public string ChannelName
        {
            get
            {
                return channelName;
            }

            set
            {
                channelName = value;
            }
        }

        public Int64 FounderiD
        {
            get
            {
                return founderiD;
            }

            set
            {
                founderiD = value;
            }
        }
    }
}
