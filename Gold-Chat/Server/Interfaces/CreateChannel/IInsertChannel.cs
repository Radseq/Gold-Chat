namespace Server.Interfaces.CreateChannel
{
    public interface IInsertChannel
    {
        int Insert(Client client, string roomName, string enterPassword, string adminPassword, int maxEnteredUsers, string welcomeMsg);
    }
}
