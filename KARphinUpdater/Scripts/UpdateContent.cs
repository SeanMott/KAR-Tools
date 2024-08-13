using Godot;
using System;
using System.IO;
using System.Diagnostics;
using System.Data.Common;

//changes the screen to another
public partial class UpdateContent : Button
{
	[Export] bool KARphin = false; //does this update KARphin

	[Export] bool Backside = false; //does this update the Backside mod

	[Export] bool BacksideCodes = false; //does this update the Backside Gekko Codes

	public override void _Ready()
	{
		Pressed += GetLatest;
	}

	//gets the latest content
	private void GetLatest()
	{
		string installDir = System.Environment.CurrentDirectory;
		string toolsDir =  KWStructure.GenerateKWStructure_Directory_Tools(installDir) + "/Windows/";

		Process p = new Process();
		
		if(!Directory.Exists("UpdaterGarbage"))
			Directory.CreateDirectory("UpdaterGarbage");

		if(KARphin)
		{
			KWQI KARphin = new KWQI();// = KWQI.LoadKWQI("KWQI/KARphin.KWQI");
			KARphin.internalName = "KARphin";
			KARphin.ContentDownloadURL_Windows = "https://github.com/SeanMott/KARphin_Modern/releases/download/R10-Migration/KARphin.br";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), KARphin.internalName, KARphin);

			p.StartInfo.UseShellExecute = true;
			KWQIPackaging.DownloadContent_Archive_Windows(out p, toolsDir + "Duma.exe", KARphin.internalName, KARphin.ContentDownloadURL_Windows,
			KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir));
			p.WaitForExit();

			//extracts
			KWQIPackaging.UnpackArchive_Windows(KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir),
			 KARphin.internalName,
			  KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir),
			   true, toolsDir + "brotli.exe", toolsDir + "7z.exe");

			//installs the new content into the mods directory
			KWQIPackaging.CopyAllDirContents(KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir) + "/" + KARphin.internalName,
			KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir));
			Directory.Delete(KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir) + "/" + KARphin.internalName, true);

			//runs Dolphin
			var dolphin = new Process();
			dolphin.StartInfo.FileName = KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir) + "/KARphin";
			dolphin.StartInfo.WorkingDirectory = KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir);
			dolphin.Start();
		}
	}
}
