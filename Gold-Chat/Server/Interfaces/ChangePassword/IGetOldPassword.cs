namespace Server.Interfaces.ChangePassword
{
    interface IGetOldPassword
    {
        string GetPassword(string clientName);
    }
}
