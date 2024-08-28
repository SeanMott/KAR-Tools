using Godot;
using System;

[GlobalClass, Tool]
public partial class InstallerData : Resource
{
	[Export] public string installPath = ""; //where KARphin and KAR Workshop are going to be installed

	[Export] public string userFolderToPortOver = ""; //the user folder we are porting over
	[Export] public bool isMigrating = false; //are we migrating from R10 to KARphin

	[Export] public bool downloadRoms = false; //are we installing the ROMs as well
	[Export] public bool downloadSkinPacks = false; //are we downloading the Skin Packs

	[Export] public bool KARDontDownload = false; //KAR Don't should be downloaded
	[Export] public bool GCAdapterDriversShouldInstall = false;
}
