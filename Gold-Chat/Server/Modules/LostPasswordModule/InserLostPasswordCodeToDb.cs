using Server.Interfaces;
using Server.Interfaces.LostPassword;
using Server.Utilies;

namespace Server.Modules.LostPasswordModule
{
    class InserLostPasswordCodeToDb : IInsertUserLostPasswordCode
    {
        private readonly IDataBase DataBase;

        public InserLostPasswordCodeToDb(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public int InsertCode(string id_user, string generatedCode)
        {
            DataBase.bind(new string[] { "idUser", id_user, "Code", generatedCode, "CodeCreateDate", Utilities.getDataTimeNow() });
            int created = DataBase.executeNonQuery("INSERT INTO user_lost_pass (id_user, code, code_create_date) VALUES (@idUser, @Code, @CodeCreateDate)");
            return created;
        }
    }
}
