using System.Diagnostics;
using System;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;

namespace FileUnlockingSharp
{
    internal class SimpleUnlocker
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        public static void UnlockPath(string filePath)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "handle.exe";
                process.StartInfo.Arguments = $"\"{filePath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                foreach (string line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    try
                    {
                        if (line.Contains("pid:"))
                        {
                            string splitted = Strings.Split(line, "pid: ")[1].Split(' ')[0];

                            if (int.TryParse(splitted, out int pid))
                            {
                                try
                                {
                                    Process proc = Process.GetProcessById(pid);
                                    proc.Kill();
                                }
                                catch
                                {

                                }

                                try
                                {
                                    IntPtr processHandle = OpenProcess(0x001F0FFF, false, process.Id);

                                    if (processHandle == IntPtr.Zero)
                                    {
                                        continue;
                                    }

                                    TerminateProcess(processHandle, 0);
                                    CloseHandle(processHandle);
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {

            }
        }
    }
}