using CommandClient;
using Gold_Client.Model;
using Gold_Client.ViewModel.Others;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel
{
    public class SendFilePresenter : ObservableObject, IFileDialog
    {
        public static string FileToSend { get; set; }
        public string NameOfUserToSendFile;

        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        ProcessReceivedByte getMessageFromServer = ProcessReceivedByte.Instance;

        private string filePatchTextBox;
        private string sendingStep;

        public SendFilePresenter()
        {
            getMessageFromServer.ClientReceiveFile += OnReceiveFile;
            getMessageFromServer.ClientReceiveFileInfo += OnClientReceiveFileInfo;
        }

        private void OnClientReceiveFileInfo(object sender, ClientEventArgs e)
        {
            if (e.FileLen == "AcceptReceive" && e.clientFriendName != App.Client.strName)
            {
                SendFile();
                SendingStep = $"{e.clientFriendName} starting receive file from you";
            }
        }

        private void OnReceiveFile(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientFriendName, $"Gold Chat: {App.Client.strName}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public string FilePatchTextBox
        {
            get { return filePatchTextBox; }
            set
            {
                filePatchTextBox = value;
                RaisePropertyChangedEvent(nameof(FilePatchTextBox));
            }
        }

        public string SendingStep
        {
            get { return sendingStep; }
            set
            {
                sendingStep = value;
                RaisePropertyChangedEvent(nameof(SendingStep));
            }
        }

        public ICommand BrowseFileCommand => new DelegateCommand(() =>
        {
            FileToSend = BrowseFileDialog();
            FilePatchTextBox = FileToSend;
        });

        public ICommand SendFileCommand => new DelegateCommand(() =>
        {
            if (!string.IsNullOrWhiteSpace(NameOfUserToSendFile) && !string.IsNullOrWhiteSpace(FileToSend))
            {
                SendingStep = "Send File Info to user, Waiting for accept";
                //SendFileInfo and then Send();
                SendFileInfo();
                //Send();
            }
        });

        private string parseDirIntoFileName()
        {
            string fileName = FileToSend.Replace("\\", "/");

            while (fileName.IndexOf("/") > -1)
            {
                fileName = fileName.Substring(fileName.IndexOf("/") + 1);
            }

            return fileName;
        }

        public string BrowseFileDialog()
        {
            string fileName = string.Empty;

            OpenFileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == true)
            {
                fileName = dlg.FileName;
                if (fileName == null)
                    fileName = string.Empty;
            }

            return fileName;
        }

        private void SendFileInfo()
        {
            clientSendToServer.SendToServer(Command.sendFile, NameOfUserToSendFile, getFileLen().ToString(), parseDirIntoFileName());
        }

        private long getFileLen()
        {
            FileInfo fileInfo = new FileInfo(FileToSend);
            return fileInfo.Length;
        }

        public void SendFile()
        {
            SendFileAsync();
        }

        private async void SendFileAsync()
        {
            int readBytes = 0;
            byte[] buffer = new byte[1048000];
            bool result = await Task.Run(() => ProcessFileAsync(buffer, readBytes));
        }

        private async Task<bool> ProcessFileAsync(byte[] buffer, int readBytes)
        {
            using (FileStream stream = new FileStream(FileToSend, FileMode.Open))
            {
                do
                {
                    stream.Flush();

                    readBytes = stream.Read(buffer, 0, buffer.Length);

                    clientSendToServer.SendToServer(Command.sendFile, NameOfUserToSendFile, parseDirIntoFileName(), readBytes.ToString(), null, buffer);
                    System.Console.WriteLine(Command.sendFile + " " + NameOfUserToSendFile + " " + parseDirIntoFileName() + " " + readBytes);
                    await Task.Delay(1000);
                }
                while (readBytes > 0);
            }
            return true;
        }
    }
}
