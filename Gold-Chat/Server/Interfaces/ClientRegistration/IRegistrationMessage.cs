namespace Server.Interfaces.ClientRegistration
{
    public interface IRegistrationMessage
    {
        string RegistrationMessage(string UserName, string Code);
    }
}
