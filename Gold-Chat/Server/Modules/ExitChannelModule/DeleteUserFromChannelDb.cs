using Server.Interfaces;
using Server.Interfaces.ExitChannel;
using System;

namespace Server.Modules.ExitChannelModule
{
    class DeleteUserFromChannelDb : IDeleteUserFromChannel
    {
        private readonly IDataBase DataBase;

        public DeleteUserFromChannelDb(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public bool Delete(int idChannelDb, Int64 ClientId)
        {
            DataBase.bind(new string[] { "idUser", ClientId.ToString(), "idChannel", idChannelDb.ToString() });
            int deleted = DataBase.executeNonQuery("DELETE FROM user_channel WHERE id_user = @idUser AND id_channel = @idChannel");

            if (deleted > 0)
                return true;
            else
                return false;
        }
    }
}
