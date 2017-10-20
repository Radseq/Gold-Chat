namespace Server.Interfaces.ClientSendActiveCode
{
    public interface IActiveCodeGet
    {
        void GetRegisterCode(string UserName);
        bool DeleteRegistrationCode(string userEmail);
        string GetEmail();
        string ReturnRegisterCode();
        bool ComprareCode(string Code);
    }
}
