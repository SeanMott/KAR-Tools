using Godot;
using System;

public partial class SetMigrationPath : Button
{

	[Export] InstallerData installerSettings;

	//ref nodes
	[Export] FileDialog dialogWindow;
	[Export] Label label;

	public override void _Ready()
	{
		label.Text = "The path to the folder containing your R10 build.";
	}

	private void _on_pressed()
	{
		//open the file dialogue
		dialogWindow.Visible = !dialogWindow.Visible;
	}

	private void OnR10DirSelected(string dir)
	{
		installerSettings.R10Path = dir;
		installerSettings.isMigrating = true;
		label.Text = installerSettings.R10Path;
	}
}
