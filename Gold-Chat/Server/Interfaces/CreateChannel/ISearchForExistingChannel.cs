namespace Server.Interfaces.CreateChannel
{
    //to delete read class note ....
    public interface ISearchForExistingChannel
    {
        string[] Search(Client client, string channelName);
    }
}
