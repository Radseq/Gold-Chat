using CommandClient;
using Server.ResponseMessages;
using System.Collections.Generic;
using System;

namespace Server.ClientService
{
    class ClientKickFromChannel : ServerResponds, IPrepareRespond
    {
        List<Client> ClientList;
        List<Channel> ChannelList;

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ClientList = clientList;
            ChannelList = channelList;
        }

        public void Execute()
        {
            prepareResponse();
            string userName = Received.strMessage;       //nick of kicked user
            string kickReason = Received.strMessage2;    //Reason of kick
            string channelName = Received.strMessage3;

            //todo select if user with want ban another is creator of channel, and/or know admin passowrd

            //todo in client side get kick from channel ->
            //todo make tab list in program cs and when user got kick, close channel from tab list

            foreach (Channel channel in ChannelList)
            {
                if (channel.ChannelName == channelName /*&& ch.FounderiD == client.id*/)
                {
                    if (channel.FounderiD == Client.id)
                    {
                        if (channel.Users.Contains(userName))
                        {
                            SendMessageToChannel sendToChannel = new SendMessageToChannel(Send, ClientList, channelName);
                            sendToChannel.ResponseToChannel();
                            channel.Users.Remove(userName);

                            foreach (Client cInfo in ClientList)
                            {
                                if (cInfo.strName == userName)
                                    if (cInfo.enterChannels.Contains(channelName))
                                    {
                                        cInfo.enterChannels.Remove(userName);
                                        //todo msg to client that got kicked form channel, when got this kind of message show msgbox
                                        //delete him form channel
                                    }
                            }
                        }
                        else
                            Send.strMessage2 = "There is no " + userName + " in your channel";
                    }
                    else
                        Send.strMessage2 = "Only channel founder can kick";
                }
                else
                    Send.strMessage2 = "Your Channel not exists";
            }
        }
    }
}
