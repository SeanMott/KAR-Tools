using Godot;
using System;
using System.IO;
using System.Diagnostics;
using System.Data.Common;
using System.Runtime.CompilerServices;

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

	//downloads and unpacks the Client Boilerplate data
	void DownloadAndUnpack_ClientBoilerPlateBlob(string toolsDir, string installDir)
	{
		//downloads
		AddTextNotification("Netplay Boilerplate Blob download started...", itemIsDownloading_FontColor);
		Process p = new Process();
		p.StartInfo.UseShellExecute = true;
		//KWStructureBlob = KWQI.LoadKWQI("KWQI/KWStructureBlob.KWQI");
		KWStructureBlob.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/Clients.br";
		KWStructureBlob.internalName = "Clients";
		KWQI.WriteKWQI("KWQI", KWStructureBlob.internalName, KWStructureBlob);
		KWQIPackaging.DownloadContent_Archive_Windows(out p, toolsDir + "Duma.exe", KWStructureBlob.internalName, KWStructureBlob.ContentDownloadURL_Windows,
		installDir);
		p.WaitForExit();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, KWStructureBlob.internalName, installDir, true, toolsDir + "brotli.exe", toolsDir + "7z.exe");

		//installs the new content into the netplay client directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + KWStructureBlob.internalName,
		KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir));
		
		AddTextNotification("Netplay Boilerplate Blob done", itemIsDownloading_FontColor);
	}

	//downloads and unpacks the skin packs
	void DownloadAndUnpack_SkinPacks(string toolsDir, string installDir)
	{
		//downloads
		AddTextNotification("Skin Packs download started...", itemIsDownloading_FontColor);
		Process p = new Process();
		p.StartInfo.UseShellExecute = true;
		//KWStructureBlob = KWQI.LoadKWQI("KWQI/KWStructureBlob.KWQI");
		KWStructureBlob.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/SkinPacks.br";
		KWStructureBlob.internalName = "SkinPacks";
		KWQI.WriteKWQI("KWQI", KWStructureBlob.internalName, KWStructureBlob);
		KWQIPackaging.DownloadContent_Archive_Windows(out p, toolsDir + "Duma.exe", KWStructureBlob.internalName, KWStructureBlob.ContentDownloadURL_Windows,
		installDir);
		p.WaitForExit();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, KWStructureBlob.internalName, installDir, true, toolsDir + "brotli.exe", toolsDir + "7z.exe");

		//installs the new content into the mods directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + KWStructureBlob.internalName,
		KWStructure.GenerateKWStructure_Directory_Mods(installDir));
		AddTextNotification("Skin Packs done", itemIsDownloading_FontColor);
	}

	//downloads and unpacks the ROMs
	void DownloadAndUnpack_ROMs(string toolsDir, string installDir)
	{
		//downloads
		AddTextNotification("ROMs download started...", itemIsDownloading_FontColor);
		Process p = new Process();
		p.StartInfo.UseShellExecute = true;
		//KWStructureBlob = KWQI.LoadKWQI("KWQI/KWStructureBlob.KWQI");
		KWStructureBlob.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/ROMs.br";
		KWStructureBlob.internalName = "ROMs";
		KWQI.WriteKWQI("KWQI", KWStructureBlob.internalName, KWStructureBlob);
		KWQIPackaging.DownloadContent_Archive_Windows(out p, toolsDir + "Duma.exe", KWStructureBlob.internalName, KWStructureBlob.ContentDownloadURL_Windows,
		installDir);
		p.WaitForExit();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, KWStructureBlob.internalName, installDir, true, toolsDir + "brotli.exe", toolsDir + "7z.exe");

		//installs the new content into the mods directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + KWStructureBlob.internalName,
		KWStructure.GenerateKWStructure_Directory_ROMs(installDir));
		AddTextNotification("ROMs done", itemIsDownloading_FontColor);
	}

	//downloads and unpacks the tools
	void DownloadAndUnpack_Tools(string toolsDir, string installDir)
	{
		//downloads
		AddTextNotification("Tools download started...", itemIsDownloading_FontColor);
		Process p = new Process();
		p.StartInfo.UseShellExecute = true;
		//KWStructureBlob = KWQI.LoadKWQI("KWQI/KWStructureBlob.KWQI");
		KWStructureBlob.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/Tools.br";
		KWStructureBlob.internalName = "Tools";
		KWQI.WriteKWQI("KWQI", KWStructureBlob.internalName, KWStructureBlob);
		KWQIPackaging.DownloadContent_Archive_Windows(out p, toolsDir + "Duma.exe", KWStructureBlob.internalName, KWStructureBlob.ContentDownloadURL_Windows,
		installDir);
		p.WaitForExit();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, KWStructureBlob.internalName, installDir, true, toolsDir + "brotli.exe", toolsDir + "7z.exe");

		//installs the new content into the mods directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + KWStructureBlob.internalName,
		KWStructure.GenerateKWStructure_Directory_Tools(installDir));
		AddTextNotification("Tools done", itemIsDownloading_FontColor);
	}

	//downloads and unpacks the KAR Tools
	void DownloadAndUnpack_KARTools(string toolsDir, string installDir)
	{
		//downloads
		AddTextNotification("KAR Tools download started...", itemIsDownloading_FontColor);
		Process p = new Process();
		p.StartInfo.UseShellExecute = true;
		//KWStructureBlob = KWQI.LoadKWQI("KWQI/KWStructureBlob.KWQI");
		KWStructureBlob.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/KARTools.br";
		KWStructureBlob.internalName = "KARTools";
		KWQI.WriteKWQI("KWQI", KWStructureBlob.internalName, KWStructureBlob);
		KWQIPackaging.DownloadContent_Archive_Windows(out p, toolsDir + "Duma.exe", KWStructureBlob.internalName, KWStructureBlob.ContentDownloadURL_Windows,
		installDir);
		p.WaitForExit();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, KWStructureBlob.internalName, installDir, true, toolsDir + "brotli.exe", toolsDir + "7z.exe");

		//installs the new content into the mods directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + KWStructureBlob.internalName,
		installDir);
		AddTextNotification("Tools done", itemIsDownloading_FontColor);
	}

	//downloads and unpacks the KWQI files
	void DownloadAndUnpack_KWQI(string toolsDir, string installDir)
	{
		//downloads
		AddTextNotification("KWQI files download started...", itemIsDownloading_FontColor);
		Process p = new Process();
		p.StartInfo.UseShellExecute = true;
		//KWStructureBlob = KWQI.LoadKWQI("KWQI/KWStructureBlob.KWQI");
		KWStructureBlob.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/KWQI.br";
		KWStructureBlob.internalName = "KWQI";
		KWQI.WriteKWQI("KWQI", KWStructureBlob.internalName, KWStructureBlob);
		KWQIPackaging.DownloadContent_Archive_Windows(out p, toolsDir + "Duma.exe", KWStructureBlob.internalName, KWStructureBlob.ContentDownloadURL_Windows,
		installDir);
		p.WaitForExit();

		//extracts
		KWQIPackaging.UnpackArchive_Windows(installDir, KWStructureBlob.internalName, installDir, true, toolsDir + "brotli.exe", toolsDir + "7z.exe");

		//installs the new content into the mods directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + KWStructureBlob.internalName,
		KWStructure.GenerateKWStructure_Directory_KWQI(installDir));
		AddTextNotification("KWQI files done", itemIsDownloading_FontColor);
	}

	//downloads and unpacks the latest KARphin
	void DownloadAndUnpack_KARphin(string toolsDir, string installDir)
	{
		//downloads
		AddTextNotification("latest KARphin download started...", itemIsDownloading_FontColor);
		KWQI KARphin = new KWQI();// = KWQI.LoadKWQI("KWQI/KARphin.KWQI");
			KARphin.internalName = "KARphin";
			KARphin.ContentDownloadURL_Windows = "https://github.com/SeanMott/KARphin_Modern/releases/download/R10-Migration/KARphin.br";
			KWQI.WriteKWQI(KWStructure.GenerateKWStructure_Directory_KWQI(installDir), KARphin.internalName, KARphin);

			Process p = new Process();
			p.StartInfo.UseShellExecute = true;
			KWQIPackaging.DownloadContent_Archive_Windows(out p, toolsDir + "Duma.exe", KARphin.internalName, KARphin.ContentDownloadURL_Windows,
			KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir));
			p.WaitForExit();

			//extracts
			KWQIPackaging.UnpackArchive_Windows(KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir),
			 KARphin.internalName,
			  KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir),
			   true, toolsDir + "brotli.exe", toolsDir + "7z.exe");

			//installs the new content into the mods directory
			KWQIPackaging.CopyAllDirContents(KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir) + "/" + KARphin.internalName,
			KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir));
			Directory.Delete(KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir) + "/" + KARphin.internalName, true);
		AddTextNotification("KARphin has finished updating", itemIsDownloading_FontColor);
	}

	//starts downloading
	public void StartDownloads()
	{
		string progDir = System.Environment.CurrentDirectory + "/SupportProgs/Windows/"; //the directory for all programs

		string installDir = installerData.installPath + "/KARNetplay";
		if(Directory.Exists(installDir))
			Directory.Delete(installDir, true);
		Directory.CreateDirectory(installDir);
		installerData.installPath = installDir;

		//generates the folder structure
		GenerateFolderStructure(installDir);

		//download client boiler plate
		DownloadAndUnpack_ClientBoilerPlateBlob(progDir, installDir);
		
		//download Skin Packs
		if(installerData.downloadSkinPacks)
			DownloadAndUnpack_SkinPacks(progDir, installDir);

		//download ROMs
		if(installerData.downloadRoms)
			DownloadAndUnpack_ROMs(progDir, installDir);

		//download Tools
		DownloadAndUnpack_Tools(progDir, installDir);

		//download KAR Tools
		DownloadAndUnpack_KARTools(progDir, installDir);

		//download KWQI
		DownloadAndUnpack_KWQI(progDir, installDir);

		//download latest KARphin
		DownloadAndUnpack_KARphin(progDir, installDir);

		//delete the "uncompress folder" and "KARphin" folders
		Directory.Delete(installDir + "/UncompressedPackages", true);
		//Directory.Delete(installDir + "/KARphin", true);

		//overwrite the User data folder with the one from the legacy KARphin when migrating
		string netplayClientDir = KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir);
		if(installerData.isMigrating)
		{
			string userFolder = installerData.R10Path + "/User";
			string target = netplayClientDir + "/User";
			Directory.Delete(target, true);
			KWQIPackaging.CopyAllDirContents(userFolder, target);

			AddTextNotification("Migrated KARphin Legacy data to KARphin", systemEventInProgess_FontColor);
		}
		else //write the data for the ROM locates
		{
			string config = "[General]\nShowLag = False\nShowFrameCount = True\nISOPaths = 1\nRecursiveISOPaths = True\nISOPath0 = " +
			installerData.installPath + "/ROMs\n" +
"\n[Interface]\nConfirmStop = False\nUsePanicHandlers = True\nOnScreenDisplayMessages = True\nHideCursor = True\nAutoHideCursor = False\nMainWindowPosX = 514\nMainWindowPosY = 363\nMainWindowWidth = 660\nMainWindowHeight = 450\nLanguageCode = \nShowToolbar = True\nShowStatusbar = True\nShowLogWindow = False\nShowLogConfigWindow = False\nExtendedFPSInfo = False\nThemeName = Clean Pink\nPauseOnFocusLost = False\nDisableTooltips = False\n[Display]\nFullscreenResolution = Auto\nFullscreen = False\nRenderToMain = False\nRenderWindowXPos = 492\nRenderWindowYPos = 3\nRenderWindowWidth = 822\nRenderWindowHeight = 961\nRenderWindowAutoSize = False\nKeepWindowOnTop = False\nProgressiveScan = False\nPAL60 = False\nDisableScreenSaver = True\nForceNTSCJ = False\n[GameList]\nListDrives = False\nListWad = True\nListElfDol = True\nListWii = True\nListGC = True\nListJap = True\nListPal = True\nListUsa = True\nListSort = 3\nListSortSecondary = 0\nColorCompressed = True\nColumnPlatform = True\nColumnBanner = True\nColumnNotes = True\nColumnFileName = True\nColumnID = True\nColumnRegion = True\nColumnSize = True\nColumnState = False\n[Core]\nHLE_BS2 = False\nTimingVariance = 8\nCPUCore = 1\nFastmem = True\nCPUThread = True\nDSPHLE = True\nSyncOnSkipIdle = True\nSyncGPU = False\nSyncGpuMaxDistance = 200000\nSyncGpuMinDistance = -200000\nSyncGpuOverclock = 1.00000000\nFPRF = False\nAccurateNaNs = False\nDefaultISO = \nDVDRoot = \nApploader = \nEnableCheats = True\nSelectedLanguage = 0\nOverrideGCLang = False\nDPL2Decoder = False\nTimeStretching = False\nRSHACK = False\nLatency = 0\nMemcardAPath = F:/Documents/Kirby Workshop/ARGC-win32-shipping/FM 5.9F (Current)/FM-v5.9-Slippi-r10-Win/User/GC/MemoryCardA.USA.raw\nMemcardBPath = F:/Documents/Kirby Workshop/ARGC-win32-shipping/FM 5.9F (Current)/FM-v5.9-Slippi-r10-Win/User/GC/MemoryCardB.USA.raw\nAgpCartAPath = \nAgpCartBPath = \nSlotA = 255SlotB = 10\nSerialPort1 = 255\nBBA_MAC = \nSIDevice0 = 6\nAdapterRumble0 = False\nSimulateKonga0 = False\nSIDevice1 = 0\nAdapterRumble1 = False\nSimulateKonga1 = False\nSIDevice2 = 0\nAdapterRumble2 = False\nSimulateKonga2 = False\nSIDevice3 = 0\nAdapterRumble3 = False\nSimulateKonga3 = False\nWiiSDCard = False\nWiiKeyboard = False\nWiimoteContinuousScanning = False\nWiimoteEnableSpeaker = False\nRunCompareServer = False\nRunCompareClient = False\nEmulationSpeed = 1.00000000\nFrameSkip = 0x00000000\nOverclock = 1.00000000\nOverclockEnable = False\nGFXBackend = \nGPUDeterminismMode = auto\nPerfMapDir = \nEnableCustomRTC = False\nCustomRTCValue = 0x386d4380\nAllowAllNetplayVersions = False\nQoSEnabled = True\nAdapterWarning = True\nShownLagReductionWarning = False\n[Movie]\nPauseMovie = False\nAuthor = \nDumpFrames = False\nDumpFramesSilent = False\nShowInputDisplay = False\nShowRTC = False\n[DSP]\nEnableJIT = True\nDumpAudio = False\nDumpAudioSilent = False\nDumpUCode = False\nBackend = Cubeb\nVolume = 25\nCaptureLog = False\n[Analytics]\nID = a6857848fbbd3db6a6bac458d4c559c3\nEnabled = False\nPermissionAsked = True\n[Network]\nSSLDumpRead = False\nSSLDumpWrite = False\nSSLVerifyCert = False\nSSLDumpRootCA = False\nSSLDumpPeerCert = False\n[NetPlay]\nSelectedHostGame = Kirby Air Ride Hack Pack v1.01 (KHPE01)\nTraversalChoice = traversal\nNickname = Kirby\nHostCode = 72e6152e\nConnectPort = 2626\nHostPort = 2626\nListenPort = 0\nNetWindowPosX = 110\nNetWindowPosY = 140\nNetWindowWidth = 1149\nNetWindowHeight = 726";
		
			System.IO.StreamWriter file = new System.IO.StreamWriter(netplayClientDir + "/User/Config/Dolphin.ini");
			file.Write(config);
			file.Close();
		}

		//runs Dolphin
		var dolphin = new Process();
		dolphin.StartInfo.UseShellExecute = true;
		dolphin.StartInfo.FileName = netplayClientDir + "/KARphin";
		dolphin.StartInfo.WorkingDirectory = netplayClientDir;
		dolphin.Start();
	
		AddTextNotification("Installing is done, feel free to close the installer. Welcome to The City :3", itemHasFinished_FontColor);
	}
}
