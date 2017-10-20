namespace Server.Interfaces.ClientLogin
{
    interface ISendLoginNotyfication
    {
        bool Send(string userName, string destination);
    }
}
