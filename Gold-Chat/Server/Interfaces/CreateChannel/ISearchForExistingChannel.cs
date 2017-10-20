namespace Server.Interfaces.CreateChannel
{
    interface ISearchForExistingChannel
    {
        string[] Search(Client client, string channelName);
    }
}
