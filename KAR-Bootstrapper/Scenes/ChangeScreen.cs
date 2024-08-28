using Godot;
using System;

//changes the screen to another
public partial class ChangeScreen : Button
{
	[Export] Node2D currentScene;
	[Export] Node2D nextScene;


	//install warning stuff
	[Export] bool throwWarningIfNoUserDir = false;
	[Export] bool throwWarningIfNoInstallDir = false;
	[Export] InstallerData installerData;
	[Export] AcceptDialog noInstallFolder_PopUp;
	[Export] AcceptDialog noUserFolder_PopUp;

	public override void _Ready()
	{
		Pressed += ChangeScene;
	}

	private void ChangeScene()
	{
		//if no install directory
		if(throwWarningIfNoInstallDir && installerData.installPath == "")
		{
			noInstallFolder_PopUp.PopupCentered();
			return;
		}

		//if no User directory
		if(throwWarningIfNoUserDir && installerData.userFolderToPortOver == "")
		{
			noUserFolder_PopUp.PopupCentered();
			return;
		}

		//if we can change scenes
		currentScene.Visible = false;
		nextScene.Visible = true;
	}
}
