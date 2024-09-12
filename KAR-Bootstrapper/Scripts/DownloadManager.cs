using Godot;
using System;
using System.IO;
using System.Diagnostics;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;

public partial class DownloadManager : Node2D
{
	[Export] InstallerData installerData;

	Vector2 lastPos; //the last position of a text notification

	//the various font color items
	[Export] Theme itemHasFinished_FontColor;
	[Export] Theme itemIsDownloading_FontColor;
	[Export] Theme systemEventInProgess_FontColor;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		VisibilityChanged += StartDownloads;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	//generates the settings files and lets KARphin and Workshop know about eachother
	//and the location of ROMs
	void GenerateKWSettings()
	{
		//user settings
		Godot.Collections.Dictionary KWSettings_User = new Godot.Collections.Dictionary();
		KWSettings_User.Add("displayName", "Hey Now Star");
		KWSettings_User.Add("discordHandle", "Hey Now");
		KWSettings_User.Add("geoLocation", "NA");
		
		System.IO.StreamWriter file = new System.IO.StreamWriter(installerData.installPath + "/KWData/User.KWU");
		file.WriteLine(Json.Stringify(KWSettings_User));
		file.Close();

		//mod settings
		Godot.Collections.Dictionary KWSettings_Mod = new Godot.Collections.Dictionary();
		KWSettings_Mod.Add("RomDir", installerData.installPath + "/ROMs");
		KWSettings_Mod.Add("TexturePackDir", installerData.installPath + "/TexturePacks");
		KWSettings_Mod.Add("AudioPackDir", installerData.installPath + "/AudioPacks");
		KWSettings_Mod.Add("KWQIDir", installerData.installPath + "/KWQI");
		KWSettings_Mod.Add("ToolsDir", installerData.installPath + "/Tools");

		file = new System.IO.StreamWriter(installerData.installPath + "/KWData/ModSettings.KWM");
		file.WriteLine(Json.Stringify(KWSettings_Mod));
		file.Close();

		//netplay settings
		Godot.Collections.Dictionary KWSettings_Netplay = new Godot.Collections.Dictionary();
		KWSettings_Netplay.Add("user", installerData.installPath + "/KWData/user.KWU");
		KWSettings_Netplay.Add("RomDir",installerData.installPath + "/ROMs");
		KWSettings_Netplay.Add("NetClientsDir", installerData.installPath + "/Clients");
		KWSettings_Netplay.Add("ReplaysDir", installerData.installPath + "/Replays");
		KWSettings_Netplay.Add("KWQIDir", installerData.installPath + "/KWQI");

		file = new System.IO.StreamWriter(installerData.installPath + "/KWData/Netplay.KWN");
		file.WriteLine(Json.Stringify(KWSettings_Netplay));
		file.Close();
	}

	//adds a new text notification
	private void AddTextNotification(string text, Theme theme)
	{
		//creates a text
		Label textNotify = new Label();
		textNotify.Position = lastPos;
		textNotify.Text = text;
		textNotify.Theme = theme;
		AddChild(textNotify);

		//increment position
		lastPos.Y += 20.0f;
	}

	//downloads and unpacks the skin packs
	void DownloadAndUnpack_SkinPacks(DirectoryInfo installDir)
	{
		//downloads
			DirectoryInfo skinPackDir = KWStructure.GenerateKWStructure_SubDirectory_Mod_SkinPacks(installDir);
			KWQICommonInstalls.GetLatest_SkinPacks(skinPackDir);

			//installs the new content into the netplay client directory
			DirectoryInfo user = new DirectoryInfo(KWStructure.GenerateKWStructure_SubDirectory_Clients_User(installDir) + "/Load/Textures/KBSE01/");

			KWInstaller.CopyAllDirContents(new DirectoryInfo(skinPackDir + "/[L] B2 Non Outline Skins"),
					new DirectoryInfo(user + "[L] B2 Non Outline Skins"));
			KWInstaller.CopyAllDirContents(new DirectoryInfo(skinPackDir + "/[R] B2 Outline Skins"),
				new DirectoryInfo(user + "[R] B2 Outline Skins"));

			//installs the new content into the netplay client legacy directory
			user = new DirectoryInfo(KWStructure.GenerateKWStructure_SubDirectory_Clients_User(installDir) + "Legacy/User/Load/Textures/KBSE01/");

			KWInstaller.CopyAllDirContents(new DirectoryInfo(skinPackDir + "/[L] B2 Non Outline Skins"),
					new DirectoryInfo(user + "[L] B2 Non Outline Skins"));
			KWInstaller.CopyAllDirContents(new DirectoryInfo(skinPackDir + "/[R] B2 Outline Skins"),
				new DirectoryInfo(user + "[R] B2 Outline Skins"));
	}

	//downloads the gekko codes
	void DownloadAndUnpack_GekkoCodes(DirectoryInfo installDir)
	{
		//hack pack
		KWQICommonInstalls.GetLatest_GekkoCodes_HackPack(KWStructure.GenerateKWStructure_SubDirectory_Clients_User_GameSettings(installDir));
		//backside
		KWQICommonInstalls.GetLatest_GekkoCodes_Backside(KWStructure.GenerateKWStructure_SubDirectory_Clients_User_GameSettings(installDir));

		//hack pack
		KWQICommonInstalls.GetLatest_GekkoCodes_HackPack(KWStructure.GenerateKWStructure_SubDirectory_Clients_User_GameSettings(installDir));
		//backside
		KWQICommonInstalls.GetLatest_GekkoCodes_Backside(KWStructure.GenerateKWStructure_SubDirectory_Clients_User_GameSettings(installDir));
	}

	//starts downloading
	public void StartDownloads()
	{
		//gets the brotli
		FileInfo brotli = new FileInfo(System.Environment.CurrentDirectory + "/SupportProgs/Windows/brotli.exe");

		//gets the install directory and generates our folder for all our crap
		DirectoryInfo installDir = new DirectoryInfo(installerData.installPath + "/KARNetplay");
		if(installDir.Exists)
			installDir.Delete(true);
		installDir.Create();
		installerData.installPath = installDir.FullName;

		//overwrite the User data folder with the one from the legacy KARphin when migrating
		if(installerData.isMigrating)
		{
			//gets the dolphin.ini file
			if(!File.Exists(installerData.userFolderToPortOver + "/Config/Dolphin.ini"))
			{
				AddTextNotification("ERROR WHILE GETTING THE DOLPHIN SETTINGS TO MIGRATE, WE COULD NOT FIND THE FILE!", itemIsDownloading_FontColor);
				AddTextNotification("MAKE SURE THE FOLDER SELECT VIA \"Migrate\" WAS THE \"User\" FOLDER!", itemIsDownloading_FontColor);
			}
			else
			{
				//copy the whole folder
				KWInstaller.CopyAllDirContents(new DirectoryInfo(installerData.userFolderToPortOver),
				KWStructure.GenerateKWStructure_SubDirectory_Clients_User(installDir));
				
				AddTextNotification("Migrated KARphin Legacy data to KARphin", systemEventInProgess_FontColor);
			}
		}
		else //write the data for the ROM locates
		{
			string config = "[Analytics]\nID = 9fbc80be625d265e9c906466779b9cec\n[NetPlay]\nTraversalChoice = traversal\nChunkedUploadLimit = 0x00000bb8\nConnectPort = 0x0a42\nEnableChunkedUploadLimit = False\nHostCode = 00000000\nHostPort = 0x0a42\nIndexName = KAR\nIndexPassword = \nIndexRegion = NA\nNickname = Kirby\nUseIndex = True\nUseUPNP = False\n[Display]\nDisableScreenSaver = True\n[General]\nHotkeysRequireFocus = True\nISOPath0 = " +
				installDir + "/ROMs\nISOPaths = 1\n[Interface]\nConfirmStop = True\nOnScreenDisplayMessages = True\nShowActiveTitle = True\nUseBuiltinTitleDatabase = True\nUsePanicHandlers = True\n[Core]\nAudioLatency = 20\nAudioStretch = False\nAudioStretchMaxLatency = 80\nDPL2Decoder = False\nDPL2Quality = 2\nDSPHLE = True\n[DSP]\nEnableJIT = False\nVolume = 100\nBackend = OpenAL\nWASAPIDevice = ";
		
			DirectoryInfo configFolder = new DirectoryInfo(KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir) + "/Legacy/User/Config");
			configFolder.Create();

			System.IO.StreamWriter file = new System.IO.StreamWriter(
				configFolder.FullName + "/Dolphin.ini");
			file.Write(config);
			file.Close();
		}

		//the queue of big downloads
		const UInt16 BIG_DOWNLOAD_QUEUE_COUNT = 1;
		System.Threading.Thread[] bigDownloadQueue = { //the functions for the downloads
			new Thread(() =>KWQICommonInstalls.GetLatest_ClientDeps(KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir))),
			//new Thread(() =>DownloadAndUnpack_ROMs(progDir, installDir))
		};
		bool[] bigDownloadQueue_isInQueue = { //what downloads are we getting
			false
			//false
		};

		bool[] bigDownloadQueue_isRunning = { //what downloads are we running
			false
			//false
		};

		bool[] bigDownloadQueue_isDone = { //what downloads are done
			false
			//false
		};

		string[] bigDownloadQueue_names = { //the names of the downloads
			"Client Deps"
			//"ROMs"
		};

		//defines the indexes into the download array for specific things
		const UInt16 BIG_DOWNLOAD_QUEUE_INDEX_CLIENT_DEPS = 0;
		//const UInt16 BIG_DOWNLOAD_QUEUE_INDEX_ROMS = 1;

		UInt16 currentBigDownloadIndex = 0; //stores the current download we are working on

		//sets that we are downloading client deps
		bigDownloadQueue_isInQueue[BIG_DOWNLOAD_QUEUE_INDEX_CLIENT_DEPS] = true;

		//sets if we're downloading the ROMs
		//bigDownloadQueue_isInQueue[BIG_DOWNLOAD_QUEUE_INDEX_ROMS] = installerData.downloadRoms;

		//gets the number of big downloads we are waiting for
		UInt16 bigDownloadWaitFor = 1;
		if(installerData.downloadRoms)
			bigDownloadWaitFor++;

		//it's always client deps first
 		currentBigDownloadIndex = BIG_DOWNLOAD_QUEUE_INDEX_CLIENT_DEPS;
		
		//the queue of tiny downloads
		const UInt16 DOWNLOAD_QUEUE_COUNT = 6;
		System.Threading.Thread[] downloadQueue = { //the functions for the downloads
			new Thread(() =>DownloadAndUnpack_SkinPacks(installDir)),
			new Thread(() =>DownloadAndUnpack_GekkoCodes(installDir)),
			new Thread(() =>KWQICommonInstalls.GetLatest_KARUpdater(installDir)),
			new Thread(() =>KWQICommonInstalls.GetLatest_Tools(KWStructure.GenerateKWStructure_Directory_Tools(installDir))),
			new Thread(() =>KWQICommonInstalls.GetLatest_KARWorkshop(installDir)),
			new Thread(() =>KWQICommonInstalls.GetLatest_KARDont(KWStructure.GenerateKWStructure_SubDirectory_Mod_Hombrew(installDir)))
		};
		bool[] downloadQueue_isInQueue = { //what downloads are we getting
			false,
			false,
			false,
			false,
			false,
			false
		};

		bool[] downloadQueue_isRunning = { //what downloads are we running
			false,
			false,
			false,
			false,
			false,
			false
		};

		bool[] downloadQueue_isDone = { //what downloads are done
			false,
			false,
			false,
			false,
			false,
			false
		};

		string[] downloadQueue_names = { //the names of the downloads
			"Skin Packs",
			"Hack Pack and Backside Gekko Codes",
			"Updater",
			"Tools",
			"KAR Launcher",
			"KARDon't"
		};

		//defines the indexes into the download array for specific things
		const UInt16 DOWNLOAD_QUEUE_INDEX_SKIN_PACKS = 0;
		const UInt16 DOWNLOAD_QUEUE_INDEX_GEKKO_CODES = 1;
		const UInt16 DOWNLOAD_QUEUE_INDEX_UPDATER = 2;
		const UInt16 DOWNLOAD_QUEUE_INDEX_TOOLS = 3;
		const UInt16 DOWNLOAD_QUEUE_INDEX_KAR_WORKSHOP = 4;
		const UInt16 DOWNLOAD_QUEUE_INDEX_KARDONT = 5;

		UInt16 currentDownloadIndex = 0; //stores the current download we are working on

		//sets the download
		downloadQueue_isInQueue[DOWNLOAD_QUEUE_INDEX_TOOLS] = true;
		downloadQueue_isInQueue[DOWNLOAD_QUEUE_INDEX_UPDATER] = true;
		downloadQueue_isInQueue[DOWNLOAD_QUEUE_INDEX_GEKKO_CODES] = true;
		downloadQueue_isInQueue[DOWNLOAD_QUEUE_INDEX_KAR_WORKSHOP] = true;

		//if we're downloading Skin Packs add another
		downloadQueue_isInQueue[DOWNLOAD_QUEUE_INDEX_SKIN_PACKS] = installerData.downloadSkinPacks;

		//if we're downloading KARDon't add another
		downloadQueue_isInQueue[DOWNLOAD_QUEUE_INDEX_KARDONT] = installerData.KARDontDownload;

		//gets the first download
		for(UInt16 i = 0; i < DOWNLOAD_QUEUE_COUNT; ++i)
		{
			if(downloadQueue_isInQueue[i])
			{
				currentDownloadIndex = i;
				break;
			}
		}

		//sets the number of downloads we need to wait for
		UInt16 downloadsToWaitFor = 0;
		for(int i = 0; i < BIG_DOWNLOAD_QUEUE_COUNT; ++i)
		{
			if(bigDownloadQueue_isInQueue[i])
				downloadsToWaitFor++;
		}

		for(int i = 0; i < DOWNLOAD_QUEUE_COUNT; ++i)
		{
			if(downloadQueue_isInQueue[i])
				downloadsToWaitFor++;
		}

		//while waiting for the threads
		bool isDownloading = true;
		UInt16 finishedDownloads = 0;
		while(isDownloading)
		{
			//exits the loop if everything is done
			if(finishedDownloads == downloadsToWaitFor)
				break;

			//----BIG DOWNLOADS-----//

			//sees if the download is finished
			if(currentBigDownloadIndex < BIG_DOWNLOAD_QUEUE_COUNT && bigDownloadQueue_isInQueue[currentBigDownloadIndex] && !bigDownloadQueue_isDone[currentBigDownloadIndex] &&
				bigDownloadQueue_isRunning[currentBigDownloadIndex] &&
			 !bigDownloadQueue[currentBigDownloadIndex].IsAlive)
			 {
				finishedDownloads++;
				AddTextNotification(bigDownloadQueue_names[currentBigDownloadIndex] + " is done.", itemIsDownloading_FontColor);
				bigDownloadQueue_isDone[currentBigDownloadIndex] = true;
				bigDownloadQueue_isRunning[currentBigDownloadIndex] = false;

				//exits the loop if everything is done
				if(finishedDownloads == downloadsToWaitFor)
					break;

				//otherwise increment to the next useable download
				currentBigDownloadIndex++;
				for(UInt16 i = currentBigDownloadIndex; i < BIG_DOWNLOAD_QUEUE_COUNT; ++i)
				{
					if(bigDownloadQueue_isInQueue[i])
					{
						currentBigDownloadIndex = i;
						break;
					}
				}
			 }

			//starts a new download
			if(currentBigDownloadIndex < BIG_DOWNLOAD_QUEUE_COUNT && bigDownloadQueue_isInQueue[currentBigDownloadIndex] && !bigDownloadQueue_isDone[currentBigDownloadIndex] &&
				!bigDownloadQueue_isRunning[currentBigDownloadIndex] &&
			 !bigDownloadQueue[currentBigDownloadIndex].IsAlive)
			 {
				bigDownloadQueue_isRunning[currentBigDownloadIndex] = true;
				AddTextNotification(bigDownloadQueue_names[currentBigDownloadIndex] + " download started....", itemIsDownloading_FontColor);
				bigDownloadQueue[currentBigDownloadIndex].Start();
			 }

			//----SMALL DOWNLOADS-----//

			//sees if the download is finished
			if(currentDownloadIndex < DOWNLOAD_QUEUE_COUNT && downloadQueue_isInQueue[currentDownloadIndex] && !downloadQueue_isDone[currentDownloadIndex] &&
				downloadQueue_isRunning[currentDownloadIndex] &&
			 !downloadQueue[currentDownloadIndex].IsAlive)
			 {
				finishedDownloads++;
				AddTextNotification(downloadQueue_names[currentDownloadIndex] + " is done.", itemIsDownloading_FontColor);
				downloadQueue_isDone[currentDownloadIndex] = true;
				downloadQueue_isRunning[currentDownloadIndex] = false;

				//exits the loop if everything is done
				if(finishedDownloads == downloadsToWaitFor)
					break;

				//otherwise increment to the next useable download
				currentDownloadIndex++;
				for(UInt16 i = currentDownloadIndex; i < DOWNLOAD_QUEUE_COUNT; ++i)
				{
					if(downloadQueue_isInQueue[i])
					{
						currentDownloadIndex = i;
						break;
					}
				}
			 }

			//starts a new download
			if(currentDownloadIndex < DOWNLOAD_QUEUE_COUNT && downloadQueue_isInQueue[currentDownloadIndex] && !downloadQueue_isDone[currentDownloadIndex] &&
				!downloadQueue_isRunning[currentDownloadIndex] &&
			 !downloadQueue[currentDownloadIndex].IsAlive)
			 {
				downloadQueue_isRunning[currentDownloadIndex] = true;
				AddTextNotification(downloadQueue_names[currentDownloadIndex] + " download started....", itemIsDownloading_FontColor);
				downloadQueue[currentDownloadIndex].Start();
			 }

			 if(currentBigDownloadIndex >= BIG_DOWNLOAD_QUEUE_COUNT && currentDownloadIndex >= DOWNLOAD_QUEUE_COUNT)
			 	break;
		}

		//generates the ROMs folder
		KWStructure.GenerateKWStructure_Directory_ROMs(installDir).Create();

		//runs KAR Workshop
		var dolphin = new Process();
		dolphin.StartInfo.UseShellExecute = true;
		dolphin.StartInfo.FileName = installDir + "/KAR Launcher.exe";
		dolphin.StartInfo.WorkingDirectory = installDir.FullName;
		dolphin.Start();
	
		AddTextNotification("Installing is done, feel free to close the installer. Welcome to The City :3", itemHasFinished_FontColor);
	}
}
