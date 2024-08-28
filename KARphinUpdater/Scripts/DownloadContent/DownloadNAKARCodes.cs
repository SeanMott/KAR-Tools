using Godot;
using System.IO;
using System.Diagnostics;

//downloads the latest vannilla KAR NA Gekko Codes
public partial class DownloadNAKARCodes : Button
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
        string KWQIFilePath = "KWQI/KAR_NA_GekkoCodes.KWQI";
        KWQI content = new KWQI();
        if(!File.Exists(KWQIFilePath))
        {
            content.internalName = "GKYE01";
		    content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KARphin_Modern/releases/download/gekko/GKYE01.ini";
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
		Process p = new Process();
		p.StartInfo.UseShellExecute = true;
		KWQIPackaging.DownloadContent_GekkoCodes_Windows(out p, toolsDir + "Duma.exe", content.internalName, content.ContentDownloadURL_Windows,
		gekkoCodeDstFolder.FullName);
		p.WaitForExit();
	}
}