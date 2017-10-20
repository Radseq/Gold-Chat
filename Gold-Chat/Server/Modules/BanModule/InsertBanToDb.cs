using Server.Interfaces;
using Server.Interfaces.ClientBan;
using Server.Utilies;

namespace Server.Modules.BanModule
{
    public class InsertBanToDb : IAddBanTime
    {
        private readonly IDataBase DataBase;

        public InsertBanToDb(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public bool insertUserToDb(Client client, string banReason, string Bantime)
        {
            DataBase.bind(new string[] { "idUser", client.id.ToString(), "BanReason", banReason, "StartBanDateTime", Utilities.getDataTimeNow(), "EndBanDateTime", Bantime });

            if (DataBase.executeNonQuery("INSERT INTO user_bans (id_user, reason, start_ban, end_ban) " + "VALUES (@idUser, @BanReason, @StartBanDateTime, @EndBanDateTime)") > 0)
                return true;
            else
                return false;
        }
    }
}
