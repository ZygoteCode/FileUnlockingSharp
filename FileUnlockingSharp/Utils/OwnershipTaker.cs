using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace FileUnlockingSharp.Utils
{
    internal class OwnershipTaker
    {
        public static void TakeOwnership(string path)
        {
            string[] commands = new string[]
            {
                $"/c takeown /f \"{path}\"",
                $"/c icacls \"{path}\" /grant \"%username%\":F /c"
            };

            foreach (string command in commands)
            {
                RunCMD(command);
            }

            SystemOwnershipTake(path);
            FullTakeOwnership(path);
        }

        private static void RunCMD(string args)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", args);

                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;

                Process processStarted = Process.Start(processInfo);

                processStarted.Start();
                processStarted.WaitForExit();
            }
            catch
            {

            }
        }

        private static void SystemOwnershipTake(string path)
        {
            try
            {
                WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
                WindowsPrincipal currentPrincipal = new WindowsPrincipal(currentUser);

                if (Directory.Exists(path))
                {
                    DirectorySecurity directorySecurity = Directory.GetAccessControl(path);
                    directorySecurity.SetOwner(currentUser.User);
                    Directory.SetAccessControl(path, directorySecurity);

                    directorySecurity.AddAccessRule(new FileSystemAccessRule(
                        currentUser.User,
                        FileSystemRights.FullControl,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow
                    ));

                    Directory.SetAccessControl(path, directorySecurity);
                }
                else if (File.Exists(path))
                {
                    FileSecurity fileSecurity = File.GetAccessControl(path);
                    fileSecurity.SetOwner(currentUser.User);
                    File.SetAccessControl(path, fileSecurity);

                    fileSecurity.AddAccessRule(new FileSystemAccessRule(
                        currentUser.User,
                        FileSystemRights.FullControl,
                        AccessControlType.Allow
                    ));

                    File.SetAccessControl(path, fileSecurity);
                }
            }
            catch
            {

            }
        }

        private const int SE_FILE_OBJECT = 1;
        private const int OWNER_SECURITY_INFORMATION = 0x00000001;
        private const int DACL_SECURITY_INFORMATION = 0x00000004;
        private const uint FULL_CONTROL = 0xF0000000;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int SetNamedSecurityInfo(
            string pObjectName,
            int ObjectType,
            int SecurityInfo,
            IntPtr pSidOwner,
            IntPtr pSidGroup,
            IntPtr pDacl,
            IntPtr pSacl);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern void LocalFree(IntPtr hMem);

        private static void FullTakeOwnership(string path)
        {
            try
            {
                WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
                IntPtr userSid = GetUserSid(currentUser);

                try
                {
                    SetNamedSecurityInfo(path, SE_FILE_OBJECT, OWNER_SECURITY_INFORMATION, userSid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                }
                catch
                {

                }

                try
                {
                    SetFullControlPermissions(path, userSid);
                }
                catch
                {

                }

                Marshal.FreeHGlobal(userSid);
            }
            catch
            {

            }
        }

        private static IntPtr GetUserSid(WindowsIdentity identity)
        {
            SecurityIdentifier sid = identity.User;
            byte[] sidBytes = new byte[sid.BinaryLength];
            sid.GetBinaryForm(sidBytes, 0);
            IntPtr sidPtr = Marshal.AllocHGlobal(sidBytes.Length);
            Marshal.Copy(sidBytes, 0, sidPtr, sidBytes.Length);
            return sidPtr;
        }

        private static void SetFullControlPermissions(string path, IntPtr userSid)
        {
            IntPtr daclPtr = IntPtr.Zero;

            var rule = new EXPLICIT_ACCESS
            {
                AccessPermissions = FULL_CONTROL,
                AccessMode = AccessMode.SET_ACCESS,
                Trustee = new TRUSTEE
                {
                    TrusteeForm = TrusteeForm.TRUSTEE_IS_SID,
                    TrusteeType = TrusteeType.TRUSTEE_IS_USER,
                    Sid = userSid
                }
            };

            int result = SetEntriesInAcl(1, new[] { rule }, IntPtr.Zero, out daclPtr);

            if (result != 0)
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

            result = SetNamedSecurityInfo(path, SE_FILE_OBJECT, DACL_SECURITY_INFORMATION, IntPtr.Zero, IntPtr.Zero, daclPtr, IntPtr.Zero);
       
            if (result != 0)
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

            LocalFree(daclPtr);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EXPLICIT_ACCESS
        {
            public uint AccessPermissions;
            public AccessMode AccessMode;
            public TRUSTEE Trustee;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct TRUSTEE
        {
            public TrusteeForm TrusteeForm;
            public TrusteeType TrusteeType;
            public IntPtr Sid;
        }

        private enum AccessMode : int
        {
            SET_ACCESS = 0,
            REVOKE_ACCESS = 1,
            DENY_ACCESS = 2,
            REMOVE_ACCESS = 3
        }

        private enum TrusteeForm : int
        {
            TRUSTEE_IS_UNKNOWN = 0,
            TRUSTEE_IS_USER = 1,
            TRUSTEE_IS_GROUP = 2,
            TRUSTEE_IS_DOMAIN = 3,
            TRUSTEE_IS_ALIAS = 4,
            TRUSTEE_IS_WELL_KNOWN_GROUP = 5,
            TRUSTEE_IS_COMPUTER = 6,
            TRUSTEE_IS_SELF = 7,
            TRUSTEE_IS_SID = 8,
            TRUSTEE_IS_INVALID = 9
        }

        private enum TrusteeType : int
        {
            TRUSTEE_IS_UNKNOWN = 0,
            TRUSTEE_IS_USER = 1,
            TRUSTEE_IS_GROUP = 2,
            TRUSTEE_IS_DOMAIN = 3,
            TRUSTEE_IS_ALIAS = 4,
            TRUSTEE_IS_WELL_KNOWN_GROUP = 5,
            TRUSTEE_IS_COMPUTER = 6,
            TRUSTEE_IS_SELF = 7,
            TRUSTEE_IS_INVALID = 8
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int SetEntriesInAcl(
            int cCountOfExplicitEntries,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] EXPLICIT_ACCESS[] pExplicitEntries,
            IntPtr OldAcl,
            out IntPtr NewAcl);
    }
}