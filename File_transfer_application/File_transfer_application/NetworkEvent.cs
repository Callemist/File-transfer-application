using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_transfer_application
{
    delegate void NetworkEventHandler(object sender, NetworkEventArgs args);
    class NetworkEvent
    {
        public event NetworkEventHandler NetworkUpdate;
        public event NetworkEventHandler DownloadProgressUpdate;
        public void ReceievedFileItem(FileItem receievedItem)
        {
            NetworkUpdate(this, new NetworkEventArgs { item = receievedItem });
        }
        public void DownloadInProgress(int p)
        {
            DownloadProgressUpdate(this, new NetworkEventArgs { percentage = p });
        }
    }

    class NetworkEventArgs : EventArgs
    {
        public FileItem item { get; set; }
        public int percentage { get; set; }
    }
}
