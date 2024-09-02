using System.IO;

//downloads the latest KARphin
public partial class DownloadKARphin
{
	//gets the latest KARphin Dev build
	static public void GetKARphinDev(DirectoryInfo installDir, FileInfo brotliEXEFP)
	{
		//attempt to load KWQI data, if not found use a baked in URL
		string KWQIFilePath = "KWQI/KARphinDev.KWQI";
		KWQI content = new KWQI();
		if(!File.Exists(KWQIFilePath))
		{
			content.internalName = "KARphinDev";
			content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KARphin_Modern/releases/download/latest-dev/KARphinDev.br";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
		}
		else
		{
			content = KWQI.LoadKWQI(KWQIFilePath);
		}

		//downloads || returns the file it downloaded
		FileInfo archive = KWQIWebClient.Download_Archive_Windows(installDir, content.ContentDownloadURL_Windows, "KARphinDev");

		//extracts || returns the final extracted folder
		DirectoryInfo uncompressedData = KWQIArchive.Unpack_Windows(brotliEXEFP, archive, installDir);
		
		//installs the content
		KWInstaller.NetplayClients_AllContent(uncompressedData, installDir);

		//deletes the uncompressed and packaged data
		uncompressedData.Delete(true);
		archive.Delete();
	}

	//gets the latest KARphin build
	static public void GetKARphin(DirectoryInfo installDir, FileInfo brotliEXEFP)
	{
		//attempt to load KWQI data, if not found use a baked in URL
		string KWQIFilePath = "KWQI/KARphin.KWQI";
		KWQI content = new KWQI();
		if(!File.Exists(KWQIFilePath))
		{
			content.internalName = "KARphin";
			content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KARphin_Modern/releases/download/latest/KARphin_Test.br";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
		}
		else
		{
			content = KWQI.LoadKWQI(KWQIFilePath);
		}

		//downloads || returns the file it downloaded
		FileInfo archive = KWQIWebClient.Download_Archive_Windows(installDir, content.ContentDownloadURL_Windows, "KARphin");

		//extracts || returns the final extracted folder
		DirectoryInfo uncompressedData = KWQIArchive.Unpack_Windows(brotliEXEFP, archive, installDir);
		
		//installs the content
		KWInstaller.NetplayClients_AllContent(uncompressedData, installDir);

		//deletes the uncompressed and packaged data
		uncompressedData.Delete(true);
		archive.Delete();
	}
}
