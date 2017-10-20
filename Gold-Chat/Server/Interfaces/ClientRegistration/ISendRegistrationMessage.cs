namespace Server.Interfaces.ClientRegistration
{
    interface ISendRegistrationMessage
    {
        bool Send(string userName, string destination, string code);
    }
}
