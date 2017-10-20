using Server.Interfaces;
using Server.Interfaces.ClientLogin;
using System;

namespace Server.Modules.ClientLoginModule
{
    class ClientLoginCheckUserBan : ICheckUserBan
    {
        private readonly IDataBase DataBase;

        public ClientLoginCheckUserBan(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        private string GetDataBaseBanTime(Int64 id_user)
        {
            DataBase.bind("IdUser", id_user.ToString());
            return DataBase.singleSelect("SELECT end_ban FROM user_bans WHERE id_user = @IdUser");
        }

        public string CheckUserBan(Int64 id_user)
        {
            string endBanDateFromDB = GetDataBaseBanTime(id_user);
            if (endBanDateFromDB != "")
            {
                DateTime dt1 = DateTime.Parse(endBanDateFromDB);
                DateTime dt2 = DateTime.Now;

                if (dt1.Date > dt2.Date)
                    return endBanDateFromDB;
                else
                    return null;
            }
            return null;
        }
    }
}
