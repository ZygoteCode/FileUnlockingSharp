using System.IO;
using System.Runtime.InteropServices;

namespace FileUnlockingSharp.Utils
{
    internal class PathAttributesChanger
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetFileAttributes(string lpFileName, uint dwFileAttributes);

        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        public static void ChangeAttributes(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    FileInfo fileInfo = new FileInfo(path);
                    fileInfo.Attributes &= ~FileAttributes.Hidden;
                    fileInfo.Attributes &= ~FileAttributes.ReadOnly;
                }
                else if (Directory.Exists(path))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    directoryInfo.Attributes &= ~FileAttributes.Hidden;
                    directoryInfo.Attributes &= ~FileAttributes.ReadOnly;
                }
            }
            catch
            {

            }

            try
            {
                if (File.Exists(path))
                {
                    SetFileAttributes(path, FILE_ATTRIBUTE_NORMAL);
                }
                else if (Directory.Exists(path))
                {
                    SetFileAttributes(path, FILE_ATTRIBUTE_DIRECTORY);
                }
            }
            catch
            {

            }
        }
    }
}