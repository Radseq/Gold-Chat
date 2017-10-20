namespace Server.Interfaces.ClientLogin
{
    public interface IGetClientProperties
    {
        string[] GetUserProperties(string userName, string userPassword);
    }
}
