using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace File_transfer_application
{
    class FileItem
    {
        string _path;
        Icon _ico;

        public FileItem()
        {

        }

        public FileItem(string path)
        {
            _path = path;
            _ico = Icon.ExtractAssociatedIcon(path);
        }

        public FileItem(string path, Icon icon)
        {
            _path = path;
            _ico = icon;
        }


        public static FileItem ConvertToFileItem(byte[] byteArr)
        {
            int pathSize = BitConverter.ToInt32(byteArr, 0);
            string path = Encoding.ASCII.GetString(byteArr, 4, pathSize);
            Icon icon = BytesToIcon(byteArr.Skip((4 + pathSize)).ToArray());

            return new FileItem(path, icon);
        }

        public byte[] ConvertToByteArry()
        {
            byte[] bPath = Encoding.ASCII.GetBytes(_path);
            byte[] bPathSize = BitConverter.GetBytes(bPath.Length);
            //byte[] bIcon = IconToBytes(_ico);
            //byte[] bIconSize = BitConverter.GetBytes(bIcon.Length);

            //return bPathSize.Concat(bPath).Concat(bIconSize).Concat(bIcon).ToArray();
            return bPathSize.Concat(bPath).ToArray();
        }

        private byte[] IconToBytes(Icon icon)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                icon.Save(ms);
                return ms.ToArray();
            }
        }

        private static Icon BytesToIcon(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return new Icon(ms);
            }
        }

    }
}
