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
        public long FileSize { get; }
        public string Extension { get; }
        public string Name { get; }

        public FileMetadata() { }

        public FileMetadata(string path)
        {
            FileSize = new FileInfo(path).Length;
            Extension = Path.GetExtension(path);
            Name = Path.GetFileNameWithoutExtension(path);
        }

        private FileMetadata(long size, string extension, string name)
        {
            FileSize = size;
            Extension = extension;
            Name = name;
        }

        public static FileMetadata ConvertToObject(byte[] byteArr)
        {
            long fileSize = BitConverter.ToInt64(byteArr, 0);
            int extensionSize = BitConverter.ToInt32(byteArr, 8);
            string extension = Encoding.ASCII.GetString(byteArr, 12, extensionSize);
            string name = Encoding.ASCII.GetString(byteArr.Skip((12 + extensionSize)).ToArray());

            return new FileMetadata(fileSize, extension, name);
        }

        public byte[] ConvertToByteArry()
        {
            byte[] bFileSize = BitConverter.GetBytes(FileSize);
            byte[] bExtension = Encoding.ASCII.GetBytes(Extension);
            byte[] bExtensionSize = BitConverter.GetBytes(bExtension.Length);
            byte[] bName = Encoding.ASCII.GetBytes(Name);

            return bFileSize.Concat(bExtensionSize).Concat(bExtension).Concat(bName).ToArray();
        }


    }
}
