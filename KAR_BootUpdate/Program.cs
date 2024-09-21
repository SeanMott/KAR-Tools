using System;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Text;
using System.Runtime.CompilerServices;

//kills the process
static bool KillProcess(string processName)
{
    // Get all processes with the given name
    Process[] processes = Process.GetProcessesByName(processName);
    bool processNeedsToBeReOpened = false;

    foreach (var process in processes)
    {
        processNeedsToBeReOpened = true;
        try
        {
            // Try to close the process gracefully
            process.CloseMainWindow();
            process.WaitForExit(5000);  // Wait 5 seconds for the process to exit

            if (!process.HasExited)
            {
                // If it hasn't exited, kill it forcefully
                process.Kill();
            }

            Console.WriteLine($"{processName} closed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error closing {processName}: {ex.Message}");
        }
    }

    return processNeedsToBeReOpened;
}

//entry point
int main(String[] args)
{
    //checks what argument was passed
    if(args.Length < 1)
    {
        Console.WriteLine("This program is not meant to be ran by a user. It's only for KAR Launcher and KAR Updater.");
        Console.ReadKey();
        return -1;
    }

    //if launcher should update
    if (args[0] == "-launcher")
    {
        Console.WriteLine("Launcher updating.....");

        bool VersionsMatch = KWQIWebClient.CheckVersion_GitRelease("RiskiVR", "KAR-Launcher", args[1]);
        Console.WriteLine("KAR Launcher " + (VersionsMatch ? "is already up to date" : "needs to be updated."));

        if (!VersionsMatch)
        {
            //close the launcher process if it's currently running
            bool reopen = KillProcess("KAR Launcher");
            //Process[] processes = Process.GetProcesses();
            //Process launcherProcess = processes.FirstOrDefault(p => p.ProcessName == "KAR Launcher");
            //bool reopen = (launcherProcess != null ? true : false);
            //if (reopen)
            //    launcherProcess.Kill();
            //
            //perform update
            KWQICommonInstalls.GetLatest_KARLauncher(new System.IO.DirectoryInfo(System.Environment.CurrentDirectory));

            //reopen the launcher
            if (reopen)
            {
                Process launcherProcess = new Process();
                launcherProcess.StartInfo.FileName = new System.IO.DirectoryInfo(System.Environment.CurrentDirectory).FullName + "/KAR Launcher.exe";
                launcherProcess.Start();
            }
        }

        Console.WriteLine("Launcher done updating.");
        //Console.ReadKey(true);
        return 0;
    }

    //if updater should update
    else if (args[0] == "-updater")
    {
        Console.WriteLine("Updater updating.....");

        //checks for a new version
        bool VersionsMatch = KWQIWebClient.CheckVersion_GitRelease("RiskiVR", "KAR-Updater", args[1]);
        Console.WriteLine("KAR Updater " + (VersionsMatch ? "is already up to date" : "needs to be updated."));

        if (!VersionsMatch)
        {
            //close the update process if it's currently running
            bool reopen = KillProcess("KAR Updater");
            //Process[] processes = Process.GetProcesses();
            //Process launcherProcess = processes.FirstOrDefault(p => p.ProcessName == "KAR Updater");
            //bool reopen = (launcherProcess != null && !launcherProcess.HasExited ? true : false);
            //if (reopen)
            //    launcherProcess.Kill();

            //perform update
            KWQICommonInstalls.GetLatest_KARUpdater(new System.IO.DirectoryInfo(System.Environment.CurrentDirectory));

            //reopen the update
            if (reopen)
            {
                Process launcherProcess = new Process();
                launcherProcess.StartInfo.FileName = new System.IO.DirectoryInfo(System.Environment.CurrentDirectory).FullName + "/KAR Updater.exe";
                launcherProcess.Start();
            }
        }

        Console.WriteLine("Updater done updating.");
        //Console.ReadKey(true);
        return 0;
    }

    Console.ReadKey();
    return -1;
}

//call main
main(args);