namespace Server.Interfaces.LostPassword
{
    interface ISendEmailWithPasswordCode
    {
        bool SendLostPasswordMessage(string email, string generatedCode);
    }
}
