﻿using Autofac;
using CommandClient;
using Server.Controllers;
using Server.Interfaces;
using Server.Interfaces.CreateChannel;
using Server.Modules.ResponseMessagesController;
using System;
using System.Collections.Generic;

namespace Server.ClientService
{
    class CreateChannelController : ServerResponds, IBuildResponse
    {
        public event EventHandler<ClientEventArgs> ClientCreateChannelEvent;

        //list of all channels
        private List<Channel> ChannelsList;
        List<Client> ListOfClientsOnline;

        string userName;
        string roomName;
        string enterPassword;
        string adminPassword;
        string welcomeMsg;

        private readonly ISearchForExistingChannel SearchForExistingChannel;
        private readonly IInsertChannel InsertChannel;
        private readonly IDataBase DataBase;

        public CreateChannelController(ISearchForExistingChannel searchForChannel, IInsertChannel insertChannel, IDataBase dataBase)
        {
            SearchForExistingChannel = searchForChannel;
            InsertChannel = insertChannel;
            DataBase = dataBase;
        }

        public void Load(Client client, Data receive, List<Client> clientList = null, List<Channel> channelList = null)
        {
            Client = client;
            Received = receive;
            ChannelsList = channelList;
            ListOfClientsOnline = clientList;
        }

        public void Execute()
        {
            roomName = Received.strMessage;

            string[] getFromDb = SearchForExistingChannel.Search(Client, roomName);

            Send.strName = Received.strName;
            Send.strMessage2 = "NotCreated";

            if (getFromDb != null)
            {
                int idOfFounderDB = Int32.Parse(getFromDb[0]);
                string channelNameDB = getFromDb[1];

                if (channelNameDB == roomName)
                    Send.strMessage = "Channel Name is in Use, try other.";
                else if (idOfFounderDB != 0)
                    Send.strMessage = "You are create channel before, you can have one channel at time";
                else // User not have channel and name is free
                    insertChannelToDb();
            }
            else insertChannelToDb(); // There is no exists channelName and idfounder, so we can create channel
        }

        private void insertChannelToDb()
        {
            userName = Received.strName;
            enterPassword = Received.strMessage2;
            adminPassword = Received.strMessage3;
            welcomeMsg = Received.strMessage4;

            prepareResponse();

            if (InsertChannel.Insert(Client, roomName, enterPassword, adminPassword, /*Max Users*/5, welcomeMsg) > 0)
            {
                Send.strMessage = "You are create channel (" + roomName + ")";
                Send.strMessage2 = "CreatedChannel";
                Send.strMessage3 = null;
                Send.strMessage4 = null;
                // Add channel to as channels list
                //last inserted id, gdy jakis insert wykona sie w miedzy czasie to ta wartosc bedzie zla
                ChannelsList.Add(new Channel(DataBase.getLastInsertedID(), roomName, Client.id));

                JoinToOwnChannelAfterCreate();
            }
            else
                Send.strMessage = "Channel NOT created with unknown reason.";

            OnClientCreateChannel(roomName, Client.strName);
        }

        private void JoinToOwnChannelAfterCreate()
        {
            var scope = server._container.BeginLifetimeScope();
            var obj = scope.Resolve<JoinChannelController>();
            obj.Send = Send;
            obj.Load(Client, Received, ListOfClientsOnline);
            obj.Execute(DataBase.getLastInsertedID(), roomName);
            obj.Response();
        }

        protected virtual void OnClientCreateChannel(string channelName, string userName)
        {
            ClientCreateChannelEvent?.Invoke(this, new ClientEventArgs() { clientChannelName = channelName, clientNameChannel = userName });
        }
    }
}