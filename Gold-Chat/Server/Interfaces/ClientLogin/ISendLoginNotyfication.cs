namespace Server.Interfaces.ClientLogin
{
    interface ISendLoginNotyfication
    {
        bool SendLoginNotyfication(string userName, string destination);
    }
}
