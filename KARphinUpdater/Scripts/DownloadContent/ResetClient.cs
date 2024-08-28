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
		    content.ContentDownloadURL_Windows = "https://github.com/SeanMott/KAR-Workshop/releases/download/KWQI-Data-Dev/ClientDeps.br";
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
		KWQIPackaging.UnpackArchive_Windows(installDir, content.internalName, installDir, true, toolsDir + "brotli.exe", toolsDir + "7z.exe");

		//installs the new content into the netplay client directory
		KWQIPackaging.CopyAllDirContents(installDir + "/UncompressedPackages/" + content.internalName,
		KWStructure.GenerateKWStructure_Directory_NetplayClients(installDir));

        //clean up
		if(Directory.Exists(installDir + "/UncompressedPackages"))
			Directory.Delete(installDir + "/UncompressedPackages", true);

        //gets the Gekko Codes
        DownloadBSCodes.GetBSCodes(installDir, toolsDir);
        DownloadHPCodes.GetHPCodes(installDir, toolsDir);

        //generate Dolphin config
        string config = "[General]\nShowLag = False\nShowFrameCount = True\nISOPaths = 1\nRecursiveISOPaths = True\nISOPath0 = " +
			installDir + "/ROMs\n" +
"\n[Interface]\nConfirmStop = False\nUsePanicHandlers = True\nOnScreenDisplayMessages = True\nHideCursor = True\nAutoHideCursor = False\nMainWindowPosX = 514\nMainWindowPosY = 363\nMainWindowWidth = 800\nMainWindowHeight = 1000\nLanguageCode = \nShowToolbar = True\nShowStatusbar = True\nShowLogWindow = False\nShowLogConfigWindow = False\nExtendedFPSInfo = False\nThemeName = Clean Pink\nPauseOnFocusLost = False\nDisableTooltips = False\n[Display]\nFullscreenResolution = Auto\nFullscreen = False\nRenderToMain = False\nRenderWindowXPos = 492\nRenderWindowYPos = 3\nRenderWindowWidth = 822\nRenderWindowHeight = 961\nRenderWindowAutoSize = False\nKeepWindowOnTop = False\nProgressiveScan = False\nPAL60 = False\nDisableScreenSaver = True\nForceNTSCJ = False\n[GameList]\nListDrives = False\nListWad = True\nListElfDol = True\nListWii = True\nListGC = True\nListJap = True\nListPal = True\nListUsa = True\nListSort = 3\nListSortSecondary = 0\nColorCompressed = True\nColumnPlatform = True\nColumnBanner = True\nColumnNotes = True\nColumnFileName = True\nColumnID = True\nColumnRegion = True\nColumnSize = True\nColumnState = False\n[Core]\nHLE_BS2 = False\nTimingVariance = 8\nCPUCore = 1\nFastmem = True\nCPUThread = True\nDSPHLE = True\nSyncOnSkipIdle = True\nSyncGPU = False\nSyncGpuMaxDistance = 200000\nSyncGpuMinDistance = -200000\nSyncGpuOverclock = 1.00000000\nFPRF = False\nAccurateNaNs = False\nDefaultISO = \nDVDRoot = \nApploader = \nEnableCheats = True\nSelectedLanguage = 0\nOverrideGCLang = False\nDPL2Decoder = False\nTimeStretching = False\nRSHACK = False\nLatency = 0\nMemcardAPath = F:/Documents/Kirby Workshop/ARGC-win32-shipping/FM 5.9F (Current)/FM-v5.9-Slippi-r10-Win/User/GC/MemoryCardA.USA.raw\nMemcardBPath = F:/Documents/Kirby Workshop/ARGC-win32-shipping/FM 5.9F (Current)/FM-v5.9-Slippi-r10-Win/User/GC/MemoryCardB.USA.raw\nAgpCartAPath = \nAgpCartBPath = \nSlotA = 255SlotB = 10\nSerialPort1 = 255\nBBA_MAC = \nSIDevice0 = 6\nAdapterRumble0 = False\nSimulateKonga0 = False\nSIDevice1 = 0\nAdapterRumble1 = False\nSimulateKonga1 = False\nSIDevice2 = 0\nAdapterRumble2 = False\nSimulateKonga2 = False\nSIDevice3 = 0\nAdapterRumble3 = False\nSimulateKonga3 = False\nWiiSDCard = False\nWiiKeyboard = False\nWiimoteContinuousScanning = False\nWiimoteEnableSpeaker = False\nRunCompareServer = False\nRunCompareClient = False\nEmulationSpeed = 1.00000000\nFrameSkip = 0x00000000\nOverclock = 1.00000000\nOverclockEnable = False\nGFXBackend = \nGPUDeterminismMode = auto\nPerfMapDir = \nEnableCustomRTC = False\nCustomRTCValue = 0x386d4380\nAllowAllNetplayVersions = True\nQoSEnabled = True\nAdapterWarning = True\nShownLagReductionWarning = False\n[Movie]\nPauseMovie = False\nAuthor = \nDumpFrames = False\nDumpFramesSilent = False\nShowInputDisplay = False\nShowRTC = False\n[DSP]\nEnableJIT = True\nDumpAudio = False\nDumpAudioSilent = False\nDumpUCode = False\nBackend = Cubeb\nVolume = 25\nCaptureLog = False\n[Analytics]\nID = a6857848fbbd3db6a6bac458d4c559c3\nEnabled = False\nPermissionAsked = True\n[Network]\nSSLDumpRead = False\nSSLDumpWrite = False\nSSLVerifyCert = False\nSSLDumpRootCA = False\nSSLDumpPeerCert = False\n[NetPlay]\nSelectedHostGame = Kirby Air Ride Hack Pack v1.01 (KHPE01)\nTraversalChoice = traversal\nNickname = Kirby\nHostCode = 72e6152e\nConnectPort = 2626\nHostPort = 2626\nListenPort = 0\nNetWindowPosX = 110\nNetWindowPosY = 140\nNetWindowWidth = 1149\nNetWindowHeight = 726";
		
			//generates the folder structure
			string configFolder = Directory.CreateDirectory(KWStructure.GenerateKWStructure_SubDirectory_Clients_User(installDir) + "/Config").FullName;

			System.IO.StreamWriter file = new System.IO.StreamWriter(configFolder + "/Dolphin.ini");
			file.Write(config);
			file.Close();

        //gets KARphin
        DownloadKARphin.GetKARphin(installDir, toolsDir);
        DownloadKARphin.RunKARphin(installDir);
	}
}