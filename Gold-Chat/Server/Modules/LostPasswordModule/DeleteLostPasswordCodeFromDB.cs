using Server.Interfaces;
using Server.Interfaces.LostPassword;

namespace Server.Modules.LostPasswordModule
{
    class DeleteLostPasswordCodeFromDB : IDeleteLostPasswordCode
    {
        private readonly IDataBase DataBase;

        public DeleteLostPasswordCodeFromDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public int Delete(string code)
        {
            DataBase.bind("Code", code);
            return DataBase.executeNonQuery("DELETE FROM user_lost_pass WHERE code = @Code");
        }
    }
}
