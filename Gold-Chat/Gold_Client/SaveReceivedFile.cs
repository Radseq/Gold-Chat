using Gold_Client.Model;
using System.IO;

namespace Gold_Client
{
    class SaveReceivedFile : IFileWriter
    {
        private BinaryWriter writer;

        private string fileSavePath = "C:/";

        public string FileSavePath
        {
            get
            {
                return fileSavePath;
            }
            set
            {
                fileSavePath = value.Replace("\\", "/");
            }
        }

        public void OpenFile(string fileName)
        {
            fileSavePath = fileSavePath + "/" + fileName;
            if (!File.Exists(fileSavePath))
                writer = new BinaryWriter(File.Open(fileSavePath, FileMode.Create));
            else
                writer = new BinaryWriter(File.Open(fileSavePath, FileMode.Append));
        }

        public void SaveFile(byte[] fileByte)
        {
            writer.Write(fileByte, 0, fileByte.Length);
            writer.Flush();
            writer.Close();
        }
    }
}
