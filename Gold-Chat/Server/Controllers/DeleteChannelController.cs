using CommandClient;
using Server.Interfaces;
using Server.Interfaces.DeleteChannel;
using Server.Interfaces.ResponseMessages;
using Server.Modules.ResponseMessagesController;
using Server.Utilies;
using System;
using System.Collections.Generic;

namespace Server.Controllers
{
    class DeleteChannelController : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientDeleteChannelEvent;
        //list of all channels
        List<Channel> ChannelsList;
        List<Client> ListOfClientsOnline;
        List<Client> ListOfUsersThatEnterToThisChanel;

        string channelName;
        bool isChannelExists = false;

        Channel channelToDelete;

        private readonly IGetAdminPass GetAdminChannelPassword;
        private readonly IDeleteChannel DeleteChannel;
        private readonly ISendMessageToChannel SendMessageToChannel;

        public DeleteChannelController(IGetAdminPass getAdminChannelPassword, IDeleteChannel deleteChannel, ISendMessageToChannel sendMessageToChannel)
        {
            GetAdminChannelPassword = getAdminChannelPassword;
            DeleteChannel = deleteChannel;
            SendMessageToChannel = sendMessageToChannel;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ListOfClientsOnline = clientList;
            ChannelsList = channelList;
        }

        public void Execute()
        {
            prepareResponse();
            channelName = Received.strMessage;
            string adminPass = Received.strMessage2;

            Send.strMessage2 = "Deny";

            if (adminPass == GetAdminChannelPassword.GetPassword(channelName, Client.id))
                deleteChannel(channelName);
            else
                Send.strMessage = "Wrong admin Password for delete Your Channel:" + channelName + "";
        }

        private void deleteChannel(string channelName)
        {
            if (DeleteChannel.Delete(channelName, Client.id))
            {
                channelToDelete = ChannelGets.getChannelByName(ChannelsList, channelName);
                if (channelToDelete != null)
                {
                    ListOfUsersThatEnterToThisChanel = ClientGets.getClientsWhoEnterToChannel(ListOfClientsOnline, channelName);
                    isChannelExists = true;

                    Send.strMessage2 = Client.strName;
                }
                else Send.strMessage = "You channel not exists";
            }
            else Send.strMessage = "You cannot delete your channel by exit with unknown reason (error).";
        }

        private void usersEnteredChannelsDeleteChannel() // i mean clint have list of channel where entered and users need delete that deleted channel
        {
            Client client = ClientGets.getClientEnterChannel(ListOfUsersThatEnterToThisChanel, channelName);
            if (client != null)
                client.enterChannels.Remove(channelName);
        }

        private void ResponseToChannelAboutDeleteThisChannel()
        {
            SendMessageToChannel.ResponseToChannel(Send, ListOfClientsOnline, channelName);
        }

        public override void Response()
        {
            if (isChannelExists)
            {
                ResponseToChannelAboutDeleteThisChannel();

                usersEnteredChannelsDeleteChannel();
                ChannelsList.Remove(channelToDelete);

                OnClientDeleteChannel(channelName, Client.strName);
            }
            else
                base.Response();
        }

        protected virtual void OnClientDeleteChannel(string channelName, string userName)
        {
            ClientDeleteChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}
