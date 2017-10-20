namespace Server.Interfaces.LostPassword
{
    interface IUpdateNewPassword
    {
        string Update(string newPassword, string userName, string oldPassword);
    }
}
