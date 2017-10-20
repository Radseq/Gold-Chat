namespace Server.Interfaces.LostPassword
{
    interface IGetUserIdAndEmail
    {
        string[] Get(string emailAdr);
    }
}
