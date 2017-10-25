using CommandClient;
using Gold_Client.Model;
using System.Windows;
using System.Windows.Input;

namespace Gold_Client.ViewModel.Others
{
    public class ReceiveFilePresenter : ObservableObject
    {
        ProcessReceivedByte proccesReceiverInformation = ProcessReceivedByte.Instance;
        ClientSendToServer clientSendToServer = ClientSendToServer.Instance;
        SaveReceivedFile saveFile = new SaveReceivedFile();
        Configuration<Settings> config = new Configuration<Settings>();
        Settings userSettings = new Settings();

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
            userSettings = config.LoadConfig(userSettings);
            patchOfSaveFile = userSettings.SaveFilePatch;
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

            ReceiveFileMessage = $"{FriendName} want to send you file {FileName}, File size {GetBytesReadable(FileLen)}. Press Reveive to get this file.";
        }

        private void OnClientReceiveFileInfo(object sender, ClientEventArgs e)
        {
            MessageBox.Show(e.clientFriendName, $"Gold Chat: {App.Client.strName}", MessageBoxButton.OK, MessageBoxImage.Information);
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
                userSettings.SaveFilePatch = patchOfSaveFile;
            }
            config.SaveConfig(userSettings);
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
                MessageBox.Show($"{e.clientFriendName} send you diffrent file", $"Gold Chat: {App.Client.strName}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public object ReceiveProgress()
        {
            int progressLen = checked((int)(FileLen / 980 + 1));
            object[] length = new object[1];
            return length[0] = progressLen;
        }

        public string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }
    }
}
