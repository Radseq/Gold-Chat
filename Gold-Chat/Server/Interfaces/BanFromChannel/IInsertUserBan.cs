namespace Server.Interfaces.BanFromChannel
{
    interface IInsertUserBan
    {
        bool Insert(Client client, string banReason, string Bantime, Channel channel);
    }
}
