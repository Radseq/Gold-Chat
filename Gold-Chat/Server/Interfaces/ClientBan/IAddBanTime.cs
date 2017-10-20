namespace Server.Interfaces.ClientBan
{
    interface IAddBanTime
    {
        bool insertUserToDb(Client client, string banReason, string Bantime);
    }
}
