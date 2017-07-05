using Gold_Client.Model;
using System.IO;

namespace Gold_Client
{
    class SaveReceivedFile : IFileWriter
    {
        private BinaryWriter writer;

        public void OpenFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                writer = new BinaryWriter(File.Open(fileName, FileMode.Create));
            }
            else
            {
                writer = new BinaryWriter(File.Open(fileName, FileMode.Append));
            }
        }

        public void SaveFile(byte[] fileByte)
        {
            writer.Write(fileByte, 0, fileByte.Length);
            writer.Flush();
            writer.Close();
        }
    }
}
