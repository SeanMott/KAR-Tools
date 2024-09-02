using Godot;
using System.IO;
using System.Diagnostics;

//downloads the latest skin packs
public partial class DownloadKW : Button
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
        string KWQIFilePath = "KWQI/KARWorkshop.KWQI";
        KWQI content = new KWQI();
        if(!File.Exists(KWQIFilePath))
        {
            content.internalName = "KARWorkshop";
		    content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/KARWorkshop.tar.gz";
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
		installDir);
		if(Directory.Exists(installDir + "/UncompressedPackages"))
			Directory.Delete(installDir + "/UncompressedPackages", true);
	}
}