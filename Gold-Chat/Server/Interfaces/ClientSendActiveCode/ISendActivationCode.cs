namespace Server.Interfaces.ClientSendActiveCode
{
    public interface ISendActivationCode
    {
        bool Send(string UserName, string destination, string code);
    }
}
