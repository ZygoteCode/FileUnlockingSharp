using FileUnlockingSharp.Utils;
using System;
using System.IO;

namespace FileUnlockingSharp
{
    public class FileUnlocker
    {
        public static void ForcefullyCompleteDeletePath(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new Exception("Specified path value is not a valid file or directory.");
            }

            UnlockPath(path);
            TakePathOwnership(path);
            MakePathDeletable(path);
            DeletePath(path);
        }

        public static void UnlockPath(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new Exception("Specified path value is not a valid file or directory.");
            }

            try
            {
               SimpleUnlocker.UnlockPath(path);
            }
            catch
            {

            }

            try
            {
                new ListViewLocker(path, 0).Unlock();
            }
            catch
            {

            }
        }

        public static void MakePathDeletable(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new Exception("Specified path value is not a valid file or directory.");
            }

            try
            {
                PathAttributesChanger.ChangeAttributes(path);
            }
            catch
            {

            }
        }

        public static void TakePathOwnership(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new Exception("Specified path value is not a valid file or directory.");
            }

            try
            {
                OwnershipTaker.TakeOwnership(path);
            }
            catch
            {

            }
        }

        public static void DeletePath(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new Exception("Specified path value is not a valid file or directory.");
            }

            try
            {
                PathDeleter.DeletePath(path);
            }
            catch
            {

            }
        }
    }
}