namespace Server.Interfaces.ClientRegistration
{
    public interface ICheckForAlreadyExistsUser
    {
        void GetData(string userName, string userEmail);
        bool isLoginExists();
        bool isEmailExists();
        bool isUserAlreadyRegister();
    }
}
