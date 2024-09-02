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

	KWQI KWStructureBlob = new KWQI();
	KWQI KARphinBlob = new KWQI();

	KWQI SkinPackBlob = new KWQI();

	KWQI ROMBlob = new KWQI();

	KWQI KARDontBlob = new KWQI();
	KWQI GCAdapterBlob = new KWQI();

	//KARDon't
	string KARDont_DownloadURL = "https://github.com/SeanMott/KARDont/releases/download/1.0.0-stable/KARDont.7z";

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

	//generate the folder structure
	void GenerateFolderStructure(string baseDir)
	{
		KWStructure.GenerateKWStructure_Directory_Accounts(baseDir);
		KWStructure.GenerateKWStructure_Directory_NetplayClients(baseDir);
		KWStructure.GenerateKWStructure_Directory_Replays(baseDir);

		KWStructure.GenerateKWStructure_Directory_ROMs(baseDir);
		KWStructure.GenerateKWStructure_Directory_KWQI(baseDir);

		KWStructure.GenerateKWStructure_Directory_Tools(baseDir);

		//KWStructure.GenerateKWStructure_SubDirectory_Mod_SkinPacks(baseDir);
		//KWStructure.GenerateKWStructure_SubDirectory_Mod_AudioPacks(baseDir);
		//KWStructure.GenerateKWStructure_SubDirectory_Mod_StarPacks(baseDir);
		//KWStructure.GenerateKWStructure_SubDirectory_Mod_RiderPacks(baseDir);
		//KWStructure.GenerateKWStructure_SubDirectory_Mod_CityPacks(baseDir);
	}

	//downloads and unpacks the Client eps
	void DownloadAndUnpack_ClientDeps(string toolsDir, string installDir)
	{
		//attempt to load KWQI data, if not found use a baked in URL
		string KWQIFilePath = "KWQI/ClientDeps.KWQI";
		KWQI content = new KWQI();
		if(!File.Exists(KWQIFilePath))
		{
			content.internalName = "ClientDeps";
			content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/ClientDeps.br";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
		}
		else
		{
			content = KWQI.LoadKWQI(KWQIFilePath);
		}

		//downloads
		KWQIPackaging.DownloadContent_Archive_Windows(content.internalName, content.ContentDownloadURL_Windows,
		installDir).GetAwaiter().GetResult();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, content.internalName, installDir, true, toolsDir + "brotli.exe");

		//installs the new content into the netplay client directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + content.internalName,
		KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir));
	}

	//downloads and unpacks the ROMs
	void DownloadAndUnpack_ROMs(string toolsDir, string installDir)
	{
		//attempt to load KWQI data, if not found use a baked in URL
		string KWQIFilePath = "KWQI/ROMs.KWQI";
		KWQI content = new KWQI();
		if(!File.Exists(KWQIFilePath))
		{
			content.internalName = "ROMs";
			content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/ROMs.br";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
		}
		else
		{
			content = KWQI.LoadKWQI(KWQIFilePath);
		}

		//downloads
		KWQIPackaging.DownloadContent_Archive_Windows(content.internalName, content.ContentDownloadURL_Windows,
		installDir).GetAwaiter().GetResult();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, content.internalName, installDir, true, toolsDir + "brotli.exe");

		//installs the new content into the netplay client directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + content.internalName,
		KWStructure.GenerateKWStructure_Directory_ROMs(installDir));
	}

	//downloads and unpacks the skin packs
	void DownloadAndUnpack_SkinPacks(string toolsDir, string installDir)
	{
		//attempt to load KWQI data, if not found use a baked in URL
		string KWQIFilePath = "KWQI/SkinPacks.KWQI";
		KWQI content = new KWQI();
		if(!File.Exists(KWQIFilePath))
		{
			content.internalName = "SkinPacks";
			content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/SkinPacks.br";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
		}
		else
		{
			content = KWQI.LoadKWQI(KWQIFilePath);
		}

		//downloads
		KWQIPackaging.DownloadContent_Archive_Windows(content.internalName, content.ContentDownloadURL_Windows,
		installDir).GetAwaiter().GetResult();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, content.internalName, installDir, true, toolsDir + "brotli.exe");

		//installs the new content into the netplay client directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + content.internalName,
		KWStructure.GenerateKWStructure_SubDirectory_Mod_SkinPacks(installDir));
	}

	//downloads and unpacks the tools
	void DownloadAndUnpack_Tools(string toolsDir, string installDir)
	{
		//attempt to load KWQI data, if not found use a baked in URL
		string KWQIFilePath = "KWQI/Tools.KWQI";
		KWQI content = new KWQI();
		if(!File.Exists(KWQIFilePath))
		{
			content.internalName = "Tools";
			content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/Tools.br";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
		}
		else
		{
			content = KWQI.LoadKWQI(KWQIFilePath);
		}

		//downloads
		KWQIPackaging.DownloadContent_Archive_Windows(content.internalName, content.ContentDownloadURL_Windows,
		installDir).GetAwaiter().GetResult();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, content.internalName, installDir, true, toolsDir + "brotli.exe");

		//installs the new content into the netplay client directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + content.internalName,
		KWStructure.GenerateKWStructure_Directory_Tools(installDir));
	}

	//downloads and unpacks the KAR Tools
	void DownloadAndUnpack_KARTools(string toolsDir, string installDir)
	{
		//attempt to load KWQI data, if not found use a baked in URL
		string KWQIFilePath = "KWQI/KARTools.KWQI";
		KWQI content = new KWQI();
		if(!File.Exists(KWQIFilePath))
		{
			content.internalName = "KARTools";
			content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/KARTools.br";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
		}
		else
		{
			content = KWQI.LoadKWQI(KWQIFilePath);
		}

		//downloads
		KWQIPackaging.DownloadContent_Archive_Windows(content.internalName, content.ContentDownloadURL_Windows,
		installDir).GetAwaiter().GetResult();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, content.internalName, installDir, true, toolsDir + "brotli.exe");

		//installs the new content into the netplay client directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + content.internalName,
		installDir);
	}

	//downloads and unpacks the KARDon't
	void DownloadAndUnpack_KARDont(string toolsDir, string installDir)
	{
		//attempt to load KWQI data, if not found use a baked in URL
		string KWQIFilePath = "KWQI/KARDont.KWQI";
		KWQI content = new KWQI();
		if(!File.Exists(KWQIFilePath))
		{
			content.internalName = "KARDont";
			content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KARDont/releases/download/latest/KARDont.br";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
		}
		else
		{
			content = KWQI.LoadKWQI(KWQIFilePath);
		}

		//downloads
		KWQIPackaging.DownloadContent_Archive_Windows(content.internalName, content.ContentDownloadURL_Windows,
		installDir).GetAwaiter().GetResult();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, content.internalName, installDir, true, toolsDir + "brotli.exe");

		//installs the new content into the netplay client directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + content.internalName,
		KWStructure.GenerateKWStructure_SubDirectory_Mod_Hombrew(installDir) + "/KARDont");
	}

	//downloads the gekko codes
	void DownloadAndUnpack_GekkoCodes(string toolsDir, string installDir)
	{
		//Hack Pack codes
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
		Process p = new Process();
		p.StartInfo.UseShellExecute = true;
		KWQIPackaging.DownloadContent_GekkoCodes_Windows(out p, toolsDir + "Duma.exe", content.internalName, content.ContentDownloadURL_Windows,
		gekkoCodeDstFolder.FullName);
		p.WaitForExit();

		//backside codes
		//attempt to load KWQI data, if not found use a baked in URL
		KWQIFilePath = "KWQI/BSGekkoCodes.KWQI";
		content = new KWQI();
		if(!File.Exists(KWQIFilePath))
		{
			content.internalName = "KBSE01";
			content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KARphin_Modern/releases/download/gekko/KBSE01.ini";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
		}
		else
		{
			content = KWQI.LoadKWQI(KWQIFilePath);
		}

		//downloads
		p = new Process();
		p.StartInfo.UseShellExecute = true;
		KWQIPackaging.DownloadContent_GekkoCodes_Windows(out p, toolsDir + "Duma.exe", content.internalName, content.ContentDownloadURL_Windows,
		gekkoCodeDstFolder.FullName);
		p.WaitForExit();
	}

	//downloads and unpacks the latest KARphin
	void DownloadAndUnpack_KARphin(string toolsDir, string installDir)
	{
		//attempt to load KWQI data, if not found use a baked in URL
		string KWQIFilePath = "KWQI/KARphin.KWQI";
		KWQI content = new KWQI();
		if(!File.Exists(KWQIFilePath))
		{
			content.internalName = "KARphin";
			content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KARphin_Modern/releases/download/latest/KARphin.br";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), content.internalName, content);
		}
		else
		{
			content = KWQI.LoadKWQI(KWQIFilePath);
		}

		//downloads
		Process p = new Process();
		p.StartInfo.UseShellExecute = true;
		KWQIPackaging.DownloadContent_Archive_Windows(out p, toolsDir + "Duma.exe", content.internalName, content.ContentDownloadURL_Windows,
		installDir);
		p.WaitForExit();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, content.internalName, installDir, true, toolsDir + "brotli.exe");

		//installs the new content into the netplay client directory
		KWQIPackaging.CopyAllDirContents(installDir + "/" + content.internalName,
		KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir));
		if(Directory.Exists(installDir + "/" + content.internalName))
			Directory.Delete(installDir + "/" + content.internalName, true);
	}

	//starts downloading
	public void StartDownloads()
	{
		string progDir = System.Environment.CurrentDirectory + "/SupportProgs/Windows/"; //the directory for all programs

		//calls Duma and TAR just to get the notification out of the way
		System.Diagnostics.Process progBurner = new System.Diagnostics.Process();
		progBurner.StartInfo = new System.Diagnostics.ProcessStartInfo
		{
			FileName = "tar",
			Arguments = $"",
			RedirectStandardOutput = false,
			RedirectStandardError = false,
			UseShellExecute = true,
			CreateNoWindow = false
		};
		progBurner.Start();
		progBurner.WaitForExit();

		progBurner = new System.Diagnostics.Process();
		progBurner.StartInfo = new System.Diagnostics.ProcessStartInfo
		{
			FileName = progDir + "Duma.exe",
			Arguments = $"",
			RedirectStandardOutput = false,
			RedirectStandardError = false,
			UseShellExecute = true,
			CreateNoWindow = false
		};
		progBurner.Start();
		progBurner.WaitForExit();

		//gets the install directory and generates our folder for all our crap
		string installDir = installerData.installPath + "/KARNetplay";
		if(Directory.Exists(installDir))
			Directory.Delete(installDir, true);
		Directory.CreateDirectory(installDir);
		installerData.installPath = installDir;

		//generates the folder structure
		GenerateFolderStructure(installDir);

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
				KWQIPackaging.CopyAllDirContents(installerData.userFolderToPortOver,
				Directory.CreateDirectory(KWStructure.GenerateKWStructure_SubDirectory_Clients_User(installDir)).FullName);
				
				AddTextNotification("Migrated KARphin Legacy data to KARphin", systemEventInProgess_FontColor);
			}
		}
		else //write the data for the ROM locates
		{
			string config = "[General]\nShowLag = False\nShowFrameCount = True\nISOPaths = 1\nRecursiveISOPaths = True\nISOPath0 = " +
			installerData.installPath + "/ROMs\n" +
"\n[Interface]\nConfirmStop = False\nUsePanicHandlers = True\nOnScreenDisplayMessages = True\nHideCursor = True\nAutoHideCursor = False\nMainWindowPosX = 514\nMainWindowPosY = 363\nMainWindowWidth = 800\nMainWindowHeight = 800\nLanguageCode = \nShowToolbar = True\nShowStatusbar = True\nShowLogWindow = False\nShowLogConfigWindow = False\nExtendedFPSInfo = False\nThemeName = Clean Pink\nPauseOnFocusLost = False\nDisableTooltips = False\n[Display]\nFullscreenResolution = Auto\nFullscreen = False\nRenderToMain = False\nRenderWindowXPos = 492\nRenderWindowYPos = 3\nRenderWindowWidth = 822\nRenderWindowHeight = 961\nRenderWindowAutoSize = False\nKeepWindowOnTop = False\nProgressiveScan = False\nPAL60 = False\nDisableScreenSaver = True\nForceNTSCJ = False\n[GameList]\nListDrives = False\nListWad = True\nListElfDol = True\nListWii = True\nListGC = True\nListJap = True\nListPal = True\nListUsa = True\nListSort = 3\nListSortSecondary = 0\nColorCompressed = True\nColumnPlatform = True\nColumnBanner = True\nColumnNotes = True\nColumnFileName = True\nColumnID = True\nColumnRegion = True\nColumnSize = True\nColumnState = False\n[Core]\nHLE_BS2 = False\nTimingVariance = 8\nCPUCore = 1\nFastmem = True\nCPUThread = True\nDSPHLE = True\nSyncOnSkipIdle = True\nSyncGPU = False\nSyncGpuMaxDistance = 200000\nSyncGpuMinDistance = -200000\nSyncGpuOverclock = 1.00000000\nFPRF = False\nAccurateNaNs = False\nDefaultISO = \nDVDRoot = \nApploader = \nEnableCheats = True\nSelectedLanguage = 0\nOverrideGCLang = False\nDPL2Decoder = False\nTimeStretching = False\nRSHACK = False\nLatency = 0\nMemcardAPath = F:/Documents/Kirby Workshop/ARGC-win32-shipping/FM 5.9F (Current)/FM-v5.9-Slippi-r10-Win/User/GC/MemoryCardA.USA.raw\nMemcardBPath = F:/Documents/Kirby Workshop/ARGC-win32-shipping/FM 5.9F (Current)/FM-v5.9-Slippi-r10-Win/User/GC/MemoryCardB.USA.raw\nAgpCartAPath = \nAgpCartBPath = \nSlotA = 255SlotB = 10\nSerialPort1 = 255\nBBA_MAC = \nSIDevice0 = 6\nAdapterRumble0 = False\nSimulateKonga0 = False\nSIDevice1 = 0\nAdapterRumble1 = False\nSimulateKonga1 = False\nSIDevice2 = 0\nAdapterRumble2 = False\nSimulateKonga2 = False\nSIDevice3 = 0\nAdapterRumble3 = False\nSimulateKonga3 = False\nWiiSDCard = False\nWiiKeyboard = False\nWiimoteContinuousScanning = False\nWiimoteEnableSpeaker = False\nRunCompareServer = False\nRunCompareClient = False\nEmulationSpeed = 1.00000000\nFrameSkip = 0x00000000\nOverclock = 1.00000000\nOverclockEnable = False\nGFXBackend = \nGPUDeterminismMode = auto\nPerfMapDir = \nEnableCustomRTC = False\nCustomRTCValue = 0x386d4380\nAllowAllNetplayVersions = True\nQoSEnabled = True\nAdapterWarning = True\nShownLagReductionWarning = False\n[Movie]\nPauseMovie = False\nAuthor = \nDumpFrames = False\nDumpFramesSilent = False\nShowInputDisplay = False\nShowRTC = False\n[DSP]\nEnableJIT = True\nDumpAudio = False\nDumpAudioSilent = False\nDumpUCode = False\nBackend = Cubeb\nVolume = 25\nCaptureLog = False\n[Analytics]\nID = a6857848fbbd3db6a6bac458d4c559c3\nEnabled = False\nPermissionAsked = True\n[Network]\nSSLDumpRead = False\nSSLDumpWrite = False\nSSLVerifyCert = False\nSSLDumpRootCA = False\nSSLDumpPeerCert = False\n[NetPlay]\nSelectedHostGame = Kirby Air Ride Hack Pack v1.01 (KHPE01)\nTraversalChoice = traversal\nNickname = Kirby\nHostCode = 72e6152e\nConnectPort = 2626\nHostPort = 2626\nListenPort = 0\nNetWindowPosX = 110\nNetWindowPosY = 140\nNetWindowWidth = 1149\nNetWindowHeight = 726";
		
			//generates the folder structure
			string configFolder = Directory.CreateDirectory(KWStructure.GenerateKWStructure_SubDirectory_Clients_User(installDir) + "/Config").FullName;

			System.IO.StreamWriter file = new System.IO.StreamWriter(configFolder + "/Dolphin.ini");
			file.Write(config);
			file.Close();
		}

		//the queue of big downloads
		const UInt16 BIG_DOWNLOAD_QUEUE_COUNT = 2;
		System.Threading.Thread[] bigDownloadQueue = { //the functions for the downloads
			new Thread(() =>DownloadAndUnpack_ClientDeps(progDir, installDir)),
			new Thread(() =>DownloadAndUnpack_ROMs(progDir, installDir))
		};
		bool[] bigDownloadQueue_isInQueue = { //what downloads are we getting
			false,
			false
		};

		bool[] bigDownloadQueue_isRunning = { //what downloads are we running
			false,
			false
		};

		bool[] bigDownloadQueue_isDone = { //what downloads are done
			false,
			false
		};

		string[] bigDownloadQueue_names = { //the names of the downloads
			"Client Deps",
			"ROMs"
		};

		//defines the indexes into the download array for specific things
		const UInt16 BIG_DOWNLOAD_QUEUE_INDEX_CLIENT_DEPS = 0;
		const UInt16 BIG_DOWNLOAD_QUEUE_INDEX_ROMS = 1;

		UInt16 currentBigDownloadIndex = 0; //stores the current download we are working on

		//sets that we are downloading client deps
		bigDownloadQueue_isInQueue[BIG_DOWNLOAD_QUEUE_INDEX_CLIENT_DEPS] = true;

		//sets if we're downloading the ROMs
		bigDownloadQueue_isInQueue[BIG_DOWNLOAD_QUEUE_INDEX_ROMS] = installerData.downloadRoms;

		//gets the number of big downloads we are waiting for
		UInt16 bigDownloadWaitFor = 1;
		if(installerData.downloadRoms)
			bigDownloadWaitFor++;

		//it's always client deps first
 		currentBigDownloadIndex = BIG_DOWNLOAD_QUEUE_INDEX_CLIENT_DEPS;
		
		//the queue of tiny downloads
		const UInt16 DOWNLOAD_QUEUE_COUNT = 6;
		System.Threading.Thread[] downloadQueue = { //the functions for the downloads
			new Thread(() =>DownloadAndUnpack_SkinPacks(progDir, installDir)),
			new Thread(() =>DownloadAndUnpack_GekkoCodes(progDir, installDir)),
			new Thread(() =>DownloadAndUnpack_Tools(progDir, installDir)),
			new Thread(() =>DownloadAndUnpack_KARTools(progDir, installDir)),
			new Thread(() =>DownloadAndUnpack_KARphin(progDir, installDir)),
			new Thread(() =>DownloadAndUnpack_KARDont(progDir, installDir))
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
			"Tools",
			"KAR Tools",
			"KARphin",
			"KARDon't"
		};

		//defines the indexes into the download array for specific things
		const UInt16 DOWNLOAD_QUEUE_INDEX_SKIN_PACKS = 0;
		const UInt16 DOWNLOAD_QUEUE_INDEX_GEKKO_CODES = 1;
		const UInt16 DOWNLOAD_QUEUE_INDEX_TOOLS = 2;
		const UInt16 DOWNLOAD_QUEUE_INDEX_KAR_TOOLS = 3;
		const UInt16 DOWNLOAD_QUEUE_INDEX_KARPHIN = 4;
		const UInt16 DOWNLOAD_QUEUE_INDEX_KARDONT = 5;

		UInt16 currentDownloadIndex = 0; //stores the current download we are working on

		//sets the download
		downloadQueue_isInQueue[DOWNLOAD_QUEUE_INDEX_TOOLS] = true;
		downloadQueue_isInQueue[DOWNLOAD_QUEUE_INDEX_KAR_TOOLS] = true;
		downloadQueue_isInQueue[DOWNLOAD_QUEUE_INDEX_GEKKO_CODES] = true;
		downloadQueue_isInQueue[DOWNLOAD_QUEUE_INDEX_KARPHIN] = true;

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
			if(bigDownloadQueue_isInQueue[currentBigDownloadIndex] && !bigDownloadQueue_isDone[currentBigDownloadIndex] &&
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
			if(bigDownloadQueue_isInQueue[currentBigDownloadIndex] && !bigDownloadQueue_isDone[currentBigDownloadIndex] &&
				!bigDownloadQueue_isRunning[currentBigDownloadIndex] &&
			 !bigDownloadQueue[currentBigDownloadIndex].IsAlive)
			 {
				bigDownloadQueue_isRunning[currentBigDownloadIndex] = true;
				AddTextNotification(bigDownloadQueue_names[currentBigDownloadIndex] + " download started....", itemIsDownloading_FontColor);
				bigDownloadQueue[currentBigDownloadIndex].Start();
			 }

			//----SMALL DOWNLOADS-----//

			//sees if the download is finished
			if(downloadQueue_isInQueue[currentDownloadIndex] && !downloadQueue_isDone[currentDownloadIndex] &&
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
			if(downloadQueue_isInQueue[currentDownloadIndex] && !downloadQueue_isDone[currentDownloadIndex] &&
				!downloadQueue_isRunning[currentDownloadIndex] &&
			 !downloadQueue[currentDownloadIndex].IsAlive)
			 {
				downloadQueue_isRunning[currentDownloadIndex] = true;
				AddTextNotification(downloadQueue_names[currentDownloadIndex] + " download started....", itemIsDownloading_FontColor);
				downloadQueue[currentDownloadIndex].Start();
			 }
		}

		//delete the "uncompress folder" and "KARphin" folders
		Directory.Delete(installDir + "/UncompressedPackages", true);

		//installs the skin packs
		if(installerData.downloadSkinPacks)
		{
			KWQIPackaging.CopyAllDirContents(KWStructure.GenerateKWStructure_SubDirectory_Mod_SkinPacks(installDir) + "/[L] B2 Non Outline Skins",
			KWStructure.GenerateKWStructure_SubDirectory_Clients_User(installDir) + "/Load/Textures/KBSE01/[L] B2 Non Outline Skins");
			KWQIPackaging.CopyAllDirContents(KWStructure.GenerateKWStructure_SubDirectory_Mod_SkinPacks(installDir) + "/[R] B2 Outline Skins",
			KWStructure.GenerateKWStructure_SubDirectory_Clients_User(installDir) + "/Load/Textures/KBSE01/[R] B2 Outline Skins");
		}

		//runs Dolphin
		var dolphin = new Process();
		dolphin.StartInfo.UseShellExecute = true;
		dolphin.StartInfo.FileName = KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir) + "/KARphin";
		dolphin.StartInfo.WorkingDirectory = KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir);
		dolphin.Start();
	
		AddTextNotification("Installing is done, feel free to close the installer. Welcome to The City :3", itemHasFinished_FontColor);
	}
}
