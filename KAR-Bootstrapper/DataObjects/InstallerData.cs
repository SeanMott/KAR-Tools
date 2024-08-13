using Godot;
using System;

[GlobalClass, Tool]
public partial class InstallerData : Resource
{
	[Export] public string installPath = ""; //where KARphin and KAR Workshop are going to be installed

	[Export] public string R10Path = ""; //where the R10 build is located and what we're migrating from
	[Export] public bool isMigrating = false; //are we migrating from R10 to KARphin

	[Export] public bool downloadRoms = false; //are we installing the ROMs as well
	[Export] public bool downloadSkinPacks = false; //are we downloading the Skin Packs

	[Export] public bool KARDontDownload = false; //KAR Don't should be downloaded
	[Export] public bool GCAdapterDriversShouldInstall = false;
}
