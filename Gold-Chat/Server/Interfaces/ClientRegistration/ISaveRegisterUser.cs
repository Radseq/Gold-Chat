namespace Server.Interfaces.ClientRegistration
{
    public interface ISaveRegisterUser
    {
        bool insertUserToDb(string userName, string userPassword, string destination, string token);
    }
}
