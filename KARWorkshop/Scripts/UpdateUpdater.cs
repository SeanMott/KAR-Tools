using Godot;
using System;
using System.IO;

//updates the Updater
public partial class UpdateUpdater : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += GetLatestUpdater;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	void GetLatestUpdater()
	{
		DirectoryInfo installDir = new DirectoryInfo(System.Environment.CurrentDirectory);
		
		//checks for old Updater with a space, delete it
		FileInfo oldUpdater = new FileInfo(installDir.FullName + "/KAR Updater.exe");
		if(oldUpdater.Exists)
		{
			oldUpdater.Delete();
			DirectoryInfo oldData = new DirectoryInfo(installDir.FullName + "/KAR Updater_Data");
			if(oldData.Exists)
				oldData.Delete(true);
		}

		//downloads
		KWQICommonInstalls.GetLatest_KARUpdater(KWStructure.GetSupportTool_Brotli_Windows(installDir), installDir);
		
		//runs the Updater
		var updater = new System.Diagnostics.Process();
		updater.StartInfo.FileName = installDir.FullName + "/KAR Updater.exe";
		updater.StartInfo.WorkingDirectory = installDir.FullName;
		updater.Start();
	}
}
