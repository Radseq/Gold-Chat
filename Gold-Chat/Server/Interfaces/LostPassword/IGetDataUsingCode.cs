namespace Server.Interfaces.LostPassword
{
    interface IGetDataUsingCode
    {
        string[] GetData(string code);
    }
}
