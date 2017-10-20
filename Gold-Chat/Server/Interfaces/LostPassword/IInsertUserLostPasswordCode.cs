namespace Server.Interfaces.LostPassword
{
    interface IInsertUserLostPasswordCode
    {
        int InsertCode(string id_user, string generatedCode);
    }
}
