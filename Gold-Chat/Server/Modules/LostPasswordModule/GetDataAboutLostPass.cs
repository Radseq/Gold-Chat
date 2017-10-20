using Server.Interfaces;
using Server.Interfaces.LostPassword;

namespace Server.Modules.LostPasswordModule
{
    class GetDataAboutLostPass : IGetDataUsingCode
    {
        private readonly IDataBase DataBase;

        public GetDataAboutLostPass(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public string[] GetData(string code)
        {
            DataBase.bind("Code", code);
            DataBase.manySelect("SELECT ulp.code, u.email, u.password, u.login FROM user_lost_pass ulp, users u WHERE u.id_user = ulp.id_user AND code = @Code");
            return DataBase.tableToRow();
        }
    }
}
