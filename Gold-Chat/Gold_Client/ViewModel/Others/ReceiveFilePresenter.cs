using CommandClient;
using Gold_Client.Model;
using System;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel.Others
{
    public class ReceiveFilePresenter : ObservableObject, IDisposable
    {
        ProcessReceivedByte proccesReceiverInformation = ProcessReceivedByte.Instance;
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        SaveReceivedFile saveFile = new SaveReceivedFile();
        Configuration config = new Configuration();

        private string savePatchTextBox;
        private string receiveFileMessage;
        private int currentDownloadProgress;
        private int maxValueOfProgress;

        private string FileName;
        private string FriendName;
        private long FileLen;

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
            proccesReceiverInformation.ClientReceiveFileInfo += OnClientReceiveFileInfo;

            MaxValueOfProgress = (int)ReceiveProgress();
        }

        public void SetProperies(string friendName, string fileNameToReceive, long fileLen)
        {
            FriendName = friendName;
            FileName = fileNameToReceive;
            FileLen = fileLen;

            ReceiveFileMessage = FriendName + " want to send you file " + FileName + ", Length file " + FileLen + ". Press Reveive to get this file.";
        }

        private void OnClientReceiveFileInfo(object sender, ClientEventArgs e)
        {
            System.Windows.MessageBox.Show(e.clientFriendName, "Gold Chat: " + App.Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
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

        public int MaxValueOfProgress
        {
            get { return maxValueOfProgress; }
            set
            {
                if (maxValueOfProgress != value)
                {
                    maxValueOfProgress = value;
                    RaisePropertyChangedEvent(nameof(MaxValueOfProgress));
                }
            }
        }

        public ICommand SelectPatchCommand => new DelegateCommand(() =>
        {
            System.Windows.Forms.FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                patchOfSaveFile = folderDlg.SelectedPath.Replace("\\", "/");
                SavePatchTextBox = patchOfSaveFile;
            }
        });

        public ICommand StartReceiveCommand => new DelegateCommand(() =>
        {
            clientSendToServer.SendToServer(Command.sendFile, FriendName, "AcceptReceive");
        });

        private void OnClientReceiveFile(object sender, ClientEventArgs e)
        {
            if (e.clientFriendName != App.Client.strName && e.FileName == FileName)
            {
                saveFile.FileSavePath = patchOfSaveFile;
                saveFile.OpenFile(e.FileName);
                saveFile.SaveFile(e.FileByte);
                CurrentDownloadProgress += 1;
            }
            else
                System.Windows.MessageBox.Show(e.clientFriendName + " send you diffrent file", "Gold Chat: " + App.Client.strName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void Dispose()
        {
            config.SaveConfig(config);
        }

        public object ReceiveProgress()
        {
            int progressLen = checked((int)(FileLen / 980 + 1));
            object[] length = new object[1];
            return length[0] = progressLen;
        }

    }
}
