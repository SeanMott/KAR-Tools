
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

        //close the launcher process if it's currently running
        bool reopen = false;

        //perform update
        KWQICommonInstalls.GetLatest_KARLauncher(new System.IO.DirectoryInfo(System.Environment.CurrentDirectory));

        //reopen the launcher
        if (reopen) { }

        Console.WriteLine("Launcher done updating.");
        return 0;
    }

    //if updater should update
    else if (args[0] == "-updater")
    {
        Console.WriteLine("Updater updating.....");

        //close the update process if it's currently running
        bool reopen = false;

        //perform update
        KWQICommonInstalls.GetLatest_KARUpdater(new System.IO.DirectoryInfo(System.Environment.CurrentDirectory));

        //reopen the update
        if (reopen) { }

        Console.WriteLine("Updater done updating.");
        return 0;
    }

    Console.ReadKey();
    return -1;
}

//call main
main(args);