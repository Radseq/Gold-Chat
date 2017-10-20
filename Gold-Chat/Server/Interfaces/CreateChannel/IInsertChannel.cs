namespace Server.Interfaces.CreateChannel
{
    interface IInsertChannel
    {
        int Insert(Client client, string roomName, string enterPassword, string adminPassword, int maxEnteredUsers, string welcomeMsg);
    }
}
