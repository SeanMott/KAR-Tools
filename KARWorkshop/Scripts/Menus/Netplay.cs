using Godot;
using System;
using System.Diagnostics;
using System.IO;

public partial class Netplay : Node
{
	//the index array for which Client to launch
	string[] clientNames = {
		"KARphin",
		"KARphin_Legacy",
		"KARphinDev"
	};
	uint currentClient = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	//attempts to boot the chosen client
	void BootClient()
	{
		DirectoryInfo installDir = new DirectoryInfo(System.Environment.CurrentDirectory);

		//checks if the client exists
		DirectoryInfo clientsFolder = KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir);
		FileInfo client = new FileInfo(clientsFolder.FullName + "/" + clientNames[currentClient] + ".exe");
		if(!client.Exists) //if it doesn't exist we download it
		{
			//if karphin
			if(clientNames[currentClient] == "KARphin")
				DownloadKARphin.GetKARphin(installDir, KWStructure.GetSupportTool_Brotli_Windows(installDir));

			//if KARphin Dev
			else if(clientNames[currentClient] == "KARphinDev")
				DownloadKARphin.GetKARphinDev(installDir, KWStructure.GetSupportTool_Brotli_Windows(installDir));

			//reget the exe and verify it doesn't exist
			client = new FileInfo(clientsFolder.FullName + "/" + clientNames[currentClient] + ".exe");
			if(!client.Exists)
			{
				System.Console.WriteLine($"{clientsFolder.FullName}/{clientNames[currentClient]}");
				System.Console.WriteLine($"{clientNames[currentClient]} does not exist, can not boot.");
				return;
			}
		}

		//boots the client
		var dolphin = new Process();
		dolphin.StartInfo.FileName = client.FullName;
		dolphin.StartInfo.WorkingDirectory = clientsFolder.FullName;
		dolphin.Start();
	}

	//when a new client is selected
	private void _on_client_option_item_selected(long index)
	{
		currentClient = (uint)index;

		//forces it to KARphin if index is too high or low
		if(currentClient < 0 || currentClient > 2)
			currentClient = 0;
	}

	//boots client for configuring
	private void _on_configure_pressed()
	{
		BootClient();
	}


	//resets the client data
	private void _on_reset_client_pressed()
	{
		
	}

	//joins a match for spectating
	private void _on_spectate_pressed()
	{
		BootClient();
	}

	//joins a match
	private void _on_join_match_pressed()
	{
		BootClient();
	}

	//hosts a match
	private void _on_host_match_pressed()
	{
		BootClient();
	}
}


