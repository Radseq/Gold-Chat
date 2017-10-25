using Server.Interfaces;
using Server.Interfaces.ClientRegistration;

namespace Server.Modules.ClientRegistrationModule
{
    public class InsertRegisterUserIntoDB : ISaveRegisterUser
    {
        private readonly IDataBase DataBase;

        public InsertRegisterUserIntoDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        public bool insertUserToDb(string userName, string userPassword, string destination, string token)
        {
            DataBase.bind(new string[] { "user_name", userName, "user_password", userPassword, "user_email", destination, "perm", 0.ToString() });
            int created = DataBase.executeNonQuery("INSERT INTO users(login, password, email, permission) VALUES(@user_name, @user_password, @user_email, @perm)");
            if (created > 0 && InserUserRegistrationCode(token))
                return true;
            else
                return false;
        }

        private bool InserUserRegistrationCode(string token)
        {
            DataBase.bind(new string[] { "idUser", DataBase.getLastInsertedID().ToString(), "Code", token });
            int created = DataBase.executeNonQuery("INSERT INTO client_register_code(id_user, code) VALUES(@idUser, @Code)");
            if (created == 1)
                return true;
            else return false;
        }
    }
}
