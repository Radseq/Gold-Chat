using Server.Interfaces;
using Server.Interfaces.ClientLogin;

namespace Server.Controllers
{
    class LoginController
    {
        private readonly ICheckUserBan CheckBan;
        private readonly ISendLoginNotyfication SendLoginNotyfication;
        private readonly IGetClientProperties GetClientProperties;
        private readonly IDataBase DataBase;

        public LoginController(ICheckUserBan checkBan, ISendLoginNotyfication sendLoginNotyfication, IGetClientProperties getClientProperties, IDataBase dataBase)
        {
            CheckBan = checkBan;
            SendLoginNotyfication = sendLoginNotyfication;
            DataBase = dataBase;
            GetClientProperties = getClientProperties;
        }
    }
}
