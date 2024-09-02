using Godot;
using System;
using System.IO;
using System.Net;

//automatically downloads the latest KARphin if it is out of date
public partial class AutoDownload : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//the URL to the build date file for KARphin
		string fileUrl = "https://github.com/SeanMott/KARphin_Modern/releases/download/latest/new_KARphinBuild.txt";

		//gets the file
		using (WebClient client = new WebClient())
		{
			try
			{
				client.DownloadFile(fileUrl, "new_KARphinBuild.txt");
				GD.Print("Downloaded the new build date data");
			}
			catch (Exception ex)
			{
				GD.Print("An error occurred: " + ex.Message);
			}
		}

		//checks if a previous build data file exits
		string currentBuild = "";
		if(File.Exists("KARphinBuild.txt"))
			currentBuild = File.ReadAllText("KARphinBuild.txt");

		//check the date of the new file and the old file
		//if the new date is more new, get the new KARphin
		string newBuildDate = File.ReadAllText("new_KARphinBuild.txt");
		if(newBuildDate != currentBuild)
		{
			//downloads KARphin
			KWQICommonInstalls.GetLatest_KARphin(KWStructure.GetSupportTool_Brotli_Windows(new DirectoryInfo(System.Environment.CurrentDirectory)),
			KWStructure.GenerateKWStructure_Directory_NetplayClients(new DirectoryInfo(System.Environment.CurrentDirectory)));

			//updates the build data
			File.WriteAllText("KARphinBuild.txt", newBuildDate);
			File.Delete("new_KARphinBuild.txt");
		}
	}
}
