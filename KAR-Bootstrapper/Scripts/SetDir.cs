using Godot;
using System;

public partial class SetDir : Button
{

	[Export] InstallerData installerSettings;

	//ref nodes
	[Export] FileDialog dialogWindow;
	[Export] Label label;

	public override void _Ready()
	{
		label.Text = "The path to the folder you would like to install to.";
	}

	private void _on_pressed()
	{
		//open the file dialogue
		dialogWindow.Visible = !dialogWindow.Visible;
	}

	private void OnKARWorkshopDirSelected(string dir)
	{
		installerSettings.installPath = dir;
		label.Text = installerSettings.installPath;
	}
}
