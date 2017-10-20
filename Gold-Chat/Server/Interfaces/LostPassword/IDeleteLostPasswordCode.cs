namespace Server.Interfaces.LostPassword
{
    interface IDeleteLostPasswordCode
    {
        int Delete(string code);
    }
}
