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
        public void ReceievedFileItem(FileItem receievedItem)
        {
            NetworkUpdate(this, new NetworkEventArgs { item = receievedItem });
        }
    }

    class NetworkEventArgs : EventArgs
    {
        public FileItem item { get; set; }
    }
}
