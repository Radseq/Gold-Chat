namespace Server.Interfaces.LostPassword
{
    interface ILostPasswordEmailMessage
    {
        string lostPassEmailMessage(string userName, string code);
    }
}
