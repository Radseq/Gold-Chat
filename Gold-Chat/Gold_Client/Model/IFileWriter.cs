namespace Gold_Client.Model
{
    interface IFileWriter
    {
        void OpenFile(string fileName);
        void SaveFile(byte[] fileByte);
    }
}
