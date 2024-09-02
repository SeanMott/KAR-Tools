using Godot;
using System.IO;
using System.Diagnostics;

//downloads the latest Hack Pack Gekko Codes
public partial class DownloadHPCodes : Button
{
	public override void _Ready()
	{
		Pressed += GetLatest;
	}

	//gets the HP codes
	static public void GetHPCodes(string installDir, string toolsDir)
	{
		//attempt to load KWQI data, if not found use a baked in URL
        string KWQIFilePath = "KWQI/HPGekkoCodes.KWQI";
        KWQI content = new KWQI();
        if(!File.Exists(KWQIFilePath))
        {
            content.internalName = "KHPE01";
		    content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KARphin_Modern/releases/download/gekko/KHPE01.ini";
		    KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
        }
        else
        {
            content = KWQI.LoadKWQI(KWQIFilePath);
        }

        //generates the directories as needed
        string clientsFolder = KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir);
        DirectoryInfo gekkoCodeDstFolder = Directory.CreateDirectory(clientsFolder + "/User/GameSettings");

		//downloads
		KWQIPackaging.DownloadContent_GekkoCodes_Windows(content.ContentDownloadURL_Windows, content.internalName, gekkoCodeDstFolder.FullName);
	}

	//gets the latest content
	private void GetLatest()
	{
		string installDir = System.Environment.CurrentDirectory;
		string toolsDir =  KWStructure.GenerateKWStructure_Directory_Tools(installDir) + "/Windows/";
		GetHPCodes(installDir, toolsDir);		
	}
}