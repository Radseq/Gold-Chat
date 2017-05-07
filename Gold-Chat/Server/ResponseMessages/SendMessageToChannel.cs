﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using CommandClient;

namespace Server.ResponseMessages
{
    class SendMessageToChannel : Respond, IServerSend
    {
        List<Client> ClientList;
        string ChannelName;

        public Data Send { get; set; }

        public SendMessageToChannel(Data send, List<Client> clientList, string channelName)
        {
            Send = send;
            ClientList = clientList;
            ChannelName = channelName;
        }

        public void ResponseToChannel()
        {
            foreach (Client cInfo in ClientList)
            {
                if (cInfo.enterChannels.Contains(ChannelName))
                    Response(Send.ToByte(), cInfo);
            }
        }
    }
}