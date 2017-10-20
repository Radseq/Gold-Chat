using Server.Interfaces;
using Server.Interfaces.LostPassword;

namespace Server.Modules.LostPasswordModule
{
    class UpdateDBClientPassword : IUpdateNewPassword
    {
        private readonly IDataBase DataBase;

        public UpdateDBClientPassword(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public string Update(string newPassword, string userName, string oldPassword)
        {
            DataBase.bind(new string[] { "pass", newPassword, "Login", userName, "oldPass", oldPassword });
            if (DataBase.executeNonQuery("UPDATE users SET password = @pass WHERE login = @Login AND password = @oldPass") > 0)
                return "Your Password has been changed!";
            else
                return "Unknow Error while changing password";
        }
    }
}
