using FileUnlockingSharp;
using System;

public class Program
{
    public static void Main()
    {
        while (true)
        {
            Console.Write("Path to be unlocked & deleted > ");

            string path = Console.ReadLine().Trim().ToLower().Replace("'", "").Replace('\t'.ToString(), "").Replace("\"", "");

            FileUnlocker.UnlockPath(path);
            FileUnlocker.TakePathOwnership(path);
            FileUnlocker.MakePathDeletable(path);
            FileUnlocker.DeletePath(path);
        }
    }
}