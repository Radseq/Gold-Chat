using CommandClient;
using Server.Interfaces;
using Server.Interfaces.ExitChannel;
using Server.Modules.ResponseMessagesController;
using System;
using System.Collections.Generic;

namespace Server.Controllers
{
    class ExitChannelController : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientExitChannelEvent;

        string ChannelName;
        private readonly IGetChannelData ChannelData;
        private readonly IDeleteUserFromChannel DeleteuserFromChannel;

        public ExitChannelController(IGetChannelData channelData, IDeleteUserFromChannel deleteuserFromChannel)
        {
            ChannelData = channelData;
            DeleteuserFromChannel = deleteuserFromChannel;
        }
        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelsList = null)
        {
            Client = client;
            Received = receive;
        }

        public void Execute()
        {
            prepareResponse();
            ChannelName = Received.strMessage;

            string[] getFromDb = ChannelData.Get(ChannelName, Client.id);

            if (getFromDb != null)
            {
                int idChannel = Int32.Parse(getFromDb[0]);
                Int64 adminId = Int64.Parse(getFromDb[1]);

                if (idChannel > 0 && adminId > 0)
                {
                    if (adminId != Client.id)
                    {
                        if (DeleteuserFromChannel.Delete(idChannel, Client.id))
                        {
                            RemoveChannelFromClientChannelList();
                            Send.strMessage2 = "You are exit from the channel";
                        }
                        else
                            Send.strMessage2 = "You connot exit: " + ChannelName + " contact to admin.";
                    }
                    else Send.strMessage2 = "You cannot exit channel that you created";
                }
                else Send.strMessage2 = "You cannot exit this channel because you not joined";
            }
            else Send.strMessage2 = "Channel not exit or you are not joined to";
        }

        private void RemoveChannelFromClientChannelList()
        {
            Client.enterChannels.Remove(ChannelName);
            OnClientExitChannel(ChannelName, Client.strName);
        }

        protected virtual void OnClientExitChannel(string channelName, string userName)
        {
            ClientExitChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}
