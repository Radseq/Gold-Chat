namespace Server.Interfaces.ClientLogin
{
    interface IGetClientProperties
    {
        string[] GetUserProperties(string userName, string userPassword);
    }
}
