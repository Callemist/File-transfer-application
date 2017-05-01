using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace File_transfer_application
{
    class FileItem
    {
        string _path;
        Icon _icon;
        public int id { get; set; }

        public FileItem() { }

        public FileItem(string path)
        {
            _path = path;
            _icon = Icon.ExtractAssociatedIcon(path);
        }

        private FileItem(string path, Icon icon)
        {
            _path = path;
            _icon = icon;
        }

        public string GetFileName()
        {
            return Path.GetFileName(_path);
        }

        public string GetFullPath()
        {
            return _path;
        }

        public ImageSource GetIcon()
        {
            using (Bitmap bmp = _icon.ToBitmap())
            {
                var stream = new MemoryStream();
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return BitmapFrame.Create(stream);
            }
        }

        public static FileItem ConvertToObject(byte[] byteArr)
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
            byte[] bIcon = IconToBytes(_icon);

            return bPathSize.Concat(bPath).Concat(bIcon).ToArray();
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
