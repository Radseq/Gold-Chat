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
        List<string> users = new List<string>(); //using to send userlist, when new user enter to this channel
        int founderiD;

        public Channel(string name, int creatorId)
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
    }
}
