using CommandClient;
using Server.ClientService;
using Server.Interfaces;
using Server.Interfaces.CreateChannel;
using System;
using System.Collections.Generic;
using Xunit;

namespace Server.Tests
{
    public class CreateChannelControllerTests
    {
        readonly CreateChannelController _controller;
        private readonly ISearchForExistingChannel SearchForExistingChannel;
        private readonly IInsertChannel InsertChannel;
        private readonly IDataBase DataBase;
        Client client;
        Data receive = new Data();
        Data send = new Data();
        List<Client> clientlist;
        List<Channel> channelList;

        public CreateChannelControllerTests()
        {
            SearchForExistingChannel = Substitute.For<ISearchForExistingChannel>();
            InsertChannel = Substitute.For<IInsertChannel>();
            DataBase = Substitute.For<IDataBase>();

            _controller = new CreateChannelController(SearchForExistingChannel, InsertChannel, DataBase);

            clientlist = new List<Client>();
            channelList = new List<Channel>();

            receive.cmdCommand = Command.createChannel;
            send = receive;

            SetUpClient();
            SetUpChannel();
        }

        private bool SetUpChannel()
        {
            Channel channel = new Channel(1, "TestRoom", 1);
            channel.Users.Add("TestUser");
            channelList.Add(channel);
            return true;
        }

        private bool SetUpClient()
        {
            client = new Client();
            client.id = 1;
            client.strName = "TestUser";
            clientlist.Add(client);
            return true;
        }

        void execute()
        {
            if (SetUpClient() && SetUpChannel())
            {
                _controller.Load(client, receive, clientlist, channelList);
                _controller.Execute();
                _controller.Response();
            }
        }

        [Fact]
        public void LoadObjects()
        {
            _controller.Load(client, receive, clientlist, channelList);
            Assert.True(SetUpClient() && SetUpChannel());
        }

        [Fact]
        public void throws_when_email_not_valid()
        {
            //_emailValidator.Validate(_email).Returns(false);

            Assert.Throws<ArgumentException>(
                () => execute()
            );
        }
    }
}
