using Godot;
using System;

public partial class CheckBoxUpdater : Node2D
{
	[Export] InstallerData installerData;

	private void _on_gc_adapter_checkbox_toggled(bool toggled_on)
	{
		installerData.GCAdapterDriversShouldInstall = toggled_on;
	}

	private void _on_kar_dont_checkbox_toggled(bool toggled_on)
	{
		installerData.KARDontDownload = toggled_on;
	}

	private void _on_ROMNoDownload_checkbox_toggled(bool toggled_on)
	{
		installerData.downloadRoms = !toggled_on;
	}

	private void _on_SkinPacksDownload_checkbox_toggled(bool toggled_on)
	{
		installerData.downloadSkinPacks = toggled_on;
	}

}
