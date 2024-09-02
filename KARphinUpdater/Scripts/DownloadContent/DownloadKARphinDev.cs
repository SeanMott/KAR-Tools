using Godot;
using System.IO;
using System.Diagnostics;

//downloads the latest KARphin Dev build
public partial class DownloadKARphinDev : Button
{
	public override void _Ready()
	{
		Pressed += GetLatest;
	}

	//gets the latest content
	private void GetLatest()
	{
		string installDir = System.Environment.CurrentDirectory;
		string toolsDir =  KWStructure.GenerateKWStructure_Directory_Tools(installDir) + "/Windows/";

		//attempt to load KWQI data, if not found use a baked in URL
        string KWQIFilePath = "KWQI/KARphinDev.KWQI";
        KWQI content = new KWQI();
        if(!File.Exists(KWQIFilePath))
        {
            content.internalName = "KARphinDev";
		    content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KARphin_Modern/releases/download/latest-dev/KARphinDev.tar.gz";
		    KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
        }
        else
        {
            content = KWQI.LoadKWQI(KWQIFilePath);
        }

		//downloads
		KWQIPackaging.DownloadContent_Archive_Windows(content.ContentDownloadURL_Windows, installDir, content.internalName);

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, content.internalName, true);

		//installs the new content into the netplay client directory
		KWQIPackaging.CopyAllDirContents(installDir + "/" + content.internalName,
		KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir));
		if(Directory.Exists(installDir + "/" + content.internalName))
			Directory.Delete(installDir + "/" + content.internalName, true);

		//runs Dolphin
		var dolphin = new Process();
		dolphin.StartInfo.FileName = KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir) + "/KARphinDev";
		dolphin.StartInfo.WorkingDirectory = KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir);
		dolphin.Start();
	}
}