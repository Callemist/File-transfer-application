using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_transfer_application
{
    class FileMetadata
    {
        long _size;
        string _extension;
        string _name;

        public FileMetadata() { }

        public FileMetadata(string path)
        {
            _size = new FileInfo(path).Length;
            _extension = Path.GetExtension(path);
            _name = Path.GetFileNameWithoutExtension(path);
        }

        private FileMetadata(long size, string extension, string name)
        {
            _size = size;
            _extension = extension;
            _name = name;
        }

        public static FileMetadata ConvertToFileMetadata(byte[] byteArr)
        {
            long fileSize = BitConverter.ToInt64(byteArr, 0);
            int extensionSize = BitConverter.ToInt32(byteArr, 8);
            string extension = Encoding.ASCII.GetString(byteArr, 12, extensionSize);
            string name = Encoding.ASCII.GetString(byteArr.Skip((12 + extensionSize)).ToArray());

            return new FileMetadata(fileSize, extension, name);
        }

        public byte[] ConvertToByteArry()
        {
            byte[] bFileSize = BitConverter.GetBytes(_size);
            byte[] bExtension = Encoding.ASCII.GetBytes(_extension);
            byte[] bExtensionSize = BitConverter.GetBytes(bExtension.Length);
            byte[] bName = Encoding.ASCII.GetBytes(_name);

            return bFileSize.Concat(bExtension).Concat(bExtensionSize).Concat(bName).ToArray();
        }


    }
}
