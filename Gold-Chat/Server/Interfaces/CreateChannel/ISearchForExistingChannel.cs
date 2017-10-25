namespace Server.Interfaces.CreateChannel
{
    public interface ISearchForExistingChannel
    {
        string[] Search(Client client, string channelName);
    }
}
