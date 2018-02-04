using Moq;
using NSubstitute;
using Server.Interfaces;
using System;
using System.Collections.Generic;
using Xunit;

namespace Server.Tests
{
    public class getChannelsTest
    {
        Mock<IGetListOfChannels> mock;

        List<Channel> channelList;

        public getChannelsTest()
        {
            mock = new Mock<IGetListOfChannels>();
            channelList = new List<Channel>();
        }

        void setUpchannel()
        {
            channelList.Add(new Channel(1, "Test", 1));
        }

        void execute()
        {
           // mock.
        }

        [Fact]
        public void throws_when_empty_list()
        {
            List<Channel> a = new List<Channel>();

            Channel b = new Channel(1, "Test", 1);
            a.Add(b);
            //_controller.Get().Returns(a);

            Assert.Throws<ArgumentException>(
                () => execute()
            );
        }
    }
}
