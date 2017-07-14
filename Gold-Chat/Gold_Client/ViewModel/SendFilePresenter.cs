using CommandClient;
using Gold_Client.Model;
using Gold_Client.ViewModel.Others;
using Microsoft.Win32;
using System.IO;
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
            getMessageFromServer.ClientReceiveFile += GetMessageFromServer;
        }

        private void GetMessageFromServer(object sender, ClientEventArgs e)
        {
            // should we received message that user accept receive or deny if accept use SendMethod else stop
            if (e.clientFriendName != App.Client.strName)
            {
                MessageBox.Show(e.FileLen, "Gold Chat: " + App.Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
                if (e.clientMessage == "AcceptReceive")
                {
                    SendFile();
                    SendingStep = e.clientFriendName + " starting receive file from you";
                }
            }
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
                SendingStep = "Send File Info to user Waiting for accept";
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
            clientSendToServer.SendToServer(Command.sendFile, getFileLen().ToString(), NameOfUserToSendFile, parseDirIntoFileName());
        }

        private long getFileLen()
        {
            FileInfo fileInfo = new FileInfo(FileToSend);
            return fileInfo.Length;
        }

        public void SendFile()
        {
            int readBytes = 0;
            byte[] buffer = new byte[980];

            // Blocking read file and send to the clients asynchronously.
            using (FileStream stream = new FileStream(FileToSend, FileMode.Open))
            {
                do
                {
                    stream.Flush();
                    readBytes = stream.Read(buffer, 0, buffer.Length);

                    clientSendToServer.SendToServer(Command.sendFile, NameOfUserToSendFile, null, null, null, buffer);
                }
                while (readBytes > 0);
            }
        }
    }
}
