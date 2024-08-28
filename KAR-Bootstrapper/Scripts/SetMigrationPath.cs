using Godot;
using System;

public partial class SetMigrationPath : Button
{

	[Export] InstallerData installerSettings;

	//ref nodes
	[Export] FileDialog dialogWindow;
	[Export] Label label;
	[Export] AcceptDialog notUserFolder_PopUp;

	public override void _Ready()
	{
		label.Text = "The path to the User folder you would like to port over.";
	}

	private void _on_pressed()
	{
		//open the file dialogue
		dialogWindow.Visible = !dialogWindow.Visible;
	}

	private void OnR10DirSelected(string dir)
	{
		//reject if the ending isn't a User
		System.IO.DirectoryInfo i = new System.IO.DirectoryInfo(dir);
		if(i.Name != "User")
		{
			notUserFolder_PopUp.Popup();
			//notUserFolder_PopUp.PopupCentered();
		}

		//if it's a proper user folder
		else
		{
			installerSettings.userFolderToPortOver = dir;
			installerSettings.isMigrating = true;
			label.Text = installerSettings.userFolderToPortOver;
		}
	}
}
