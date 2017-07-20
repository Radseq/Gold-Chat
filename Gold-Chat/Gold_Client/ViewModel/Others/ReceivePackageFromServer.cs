using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Gold_Client.ViewModel.Others
{
    public static class ReceivePackageFromServer
    {
        public static event EventHandler OnDataReceived;

        public static bool IsClientStartReceive { get; set; }

        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            var tcs = new TaskCompletionSource<int>(socket);
            socket.BeginReceive(buffer, offset, size, socketFlags, iar =>
            {
                var t = (TaskCompletionSource<int>)iar.AsyncState;
                var s = (Socket)t.Task.AsyncState;
                try
                {
                    t.TrySetResult(s.EndReceive(iar));
                }
                catch (Exception ex)
                {
                    t.TrySetException(ex);
                }
            }, tcs);
            return tcs.Task;
        }

        public static async void BeginReceive()
        {
            while (true)
            {
                int x = await ReceiveAsync(App.Client.cSocket, App.Client.Buffer, 0, App.Client.Buffer.Length, SocketFlags.None);
                IsClientStartReceive = true;
                OnDataReceived?.Invoke(null, EventArgs.Empty);
            }
            // clientReceive.BeginReceive();
        }
    }
}
