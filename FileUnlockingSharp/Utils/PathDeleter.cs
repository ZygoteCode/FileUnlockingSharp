using System.IO;
using System.Runtime.InteropServices;

namespace FileUnlockingSharp.Utils
{
    internal class PathDeleter
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool DeleteFile(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool RemoveDirectory(string lpPathName);

        public static void DeletePath(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {

            }

            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch
            {

            }

            try
            {
                if (File.Exists(path))
                {
                    DeleteFile(path);
                }
            }
            catch
            {

            }

            try
            {
                if (Directory.Exists(path))
                {
                    RemoveDirectory(path);
                }
            }
            catch
            {

            }

            try
            {
                if (Directory.Exists(path))
                {
                    DeleteDirectory(path);
                }
            }
            catch
            {

            }
        }

        private static void DeleteDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                try
                {
                    foreach (string file in Directory.GetFiles(dirPath))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {

                        }

                        try
                        {
                            DeleteFile(file);
                        }
                        catch
                        {

                        }
                    }
                }
                catch
                {

                }

                try
                {
                    foreach (string dir in Directory.GetDirectories(dirPath))
                    {
                        try
                        {
                            DeleteDirectory(dir);
                        }
                        catch
                        {

                        }
                    }
                }
                catch
                {

                }

                try
                {
                    Directory.Delete(dirPath);
                }
                catch
                {

                }

                try
                {
                    RemoveDirectory(dirPath);
                }
                catch
                {

                }
            }
        }
    }
}