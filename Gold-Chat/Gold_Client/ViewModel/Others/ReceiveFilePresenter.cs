using CommandClient;
using Gold_Client.Model;
using System.Windows.Forms;
using System.Windows.Input;

namespace Gold_Client.ViewModel.Others
{
    public class ReceiveFilePresenter : ObservableObject
    {
        ProcessReceivedByte proccesReceiverInformation = ProcessReceivedByte.Instance;
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        SaveReceivedFile saveFile = new SaveReceivedFile();
        Configuration config = new Configuration();

        private string savePatchTextBox;
        private string receiveFileMessage;
        private int currentDownloadProgress;

        private string patchOfSaveFile;

        public ReceiveFilePresenter()
        {
            patchOfSaveFile = config.SaveFilePatch;
            if (patchOfSaveFile == null)
            {
                patchOfSaveFile = "C:/";
                SavePatchTextBox = "C:/";
            }
            else SavePatchTextBox = patchOfSaveFile;

            proccesReceiverInformation.ClientReceiveFile += OnClientReceiveFile;
        }

        public string ReceiveFileMessage
        {
            get { return receiveFileMessage; }
            set
            {
                receiveFileMessage = value;
                RaisePropertyChangedEvent(nameof(ReceiveFileMessage));
            }
        }

        public string SavePatchTextBox
        {
            get { return savePatchTextBox; }
            set
            {
                savePatchTextBox = value;
                RaisePropertyChangedEvent(nameof(SavePatchTextBox));
            }
        }

        public int CurrentDownloadProgress
        {
            get { return currentDownloadProgress; }
            set
            {
                if (currentDownloadProgress != value)
                {
                    currentDownloadProgress = value;
                    RaisePropertyChangedEvent(nameof(CurrentDownloadProgress));
                }
            }
        }

        public ICommand SelectPatchCommand => new DelegateCommand(() =>
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowDialog();
            patchOfSaveFile = folderDlg.SelectedPath;
        });

        public ICommand StartReceiveCommand => new DelegateCommand(() =>
        {
            clientSendToServer.SendToServer(Command.sendFile, "Accept");
        });

        private void OnClientReceiveFile(object sender, ClientEventArgs e)
        {
            if (e.clientFriendName == App.Client.strName)
            {
                // TODO first message is send about fileName length etc, then file bytes

                saveFile.FileSavePath = patchOfSaveFile;
                saveFile.OpenFile(e.FileName);
                saveFile.SaveFile(e.FileByte);


            }
        }

    }
}
