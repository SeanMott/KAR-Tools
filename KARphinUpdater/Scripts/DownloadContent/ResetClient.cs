using Godot;
using System.IO;
using System.Diagnostics;

//resets the client folder, gets the latest client deps, latest KARphin, and clears the users cache
public partial class ResetClient : Button
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

		//nukes the whole User folder
		if(Directory.Exists(KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir)))
			Directory.Delete(KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir), true);

		//gets the client deps
		//attempt to load KWQI data, if not found use a baked in URL
		string KWQIFilePath = "KWQI/ClientDeps.KWQI";
		KWQI content = new KWQI();
		if(!File.Exists(KWQIFilePath))
		{
			content.internalName = "ClientDeps";
			content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/ClientDeps.tar.gz";
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
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + content.internalName,
		KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir));

		//clean up
		if(Directory.Exists(installDir + "/UncompressedPackages"))
			Directory.Delete(installDir + "/UncompressedPackages", true);

		//gets the Gekko Codes
		DownloadBSCodes.GetBSCodes(installDir, toolsDir);
		DownloadHPCodes.GetHPCodes(installDir, toolsDir);

		//gets KARphin
		DownloadKARphin.GetKARphin(installDir, toolsDir);
		DownloadKARphin.RunKARphin(installDir);
	}
}
