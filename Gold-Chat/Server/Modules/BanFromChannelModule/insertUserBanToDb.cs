using Server.Interfaces;
using Server.Interfaces.BanFromChannel;
using Server.Utilies;

namespace Server.Modules.BanFromChannelModule
{
    class insertUserBanToDb : IInsertUserBan
    {
        private readonly IDataBase DataBase;

        public insertUserBanToDb(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public bool Insert(Client client, string banReason, string Bantime, Channel channel)
        {
            DataBase.bind(new string[] { "idUser", client.id.ToString(), "idChannel", channel.ChannelId.ToString(),
                "BanReason", banReason, "StartBanDateTime", Utilities.getDataTimeNow(), "EndBanDateTime", Bantime });

            if (DataBase.executeNonQuery("INSERT INTO channel_user_bans (id_user, id_channel, reason, start_ban, end_ban) VALUES (@idUser, @idChannel, @BanReason, @StartBanDateTime, @EndBanDateTime)") > 0)
                return true;

            return false;
        }
    }
}
