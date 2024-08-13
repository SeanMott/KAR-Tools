using Godot;
using System;

//changes the screen to another
public partial class ChangeScreen : Button
{
	[Export] Node2D currentScene;
	[Export] Node2D nextScene;

	public override void _Ready()
	{
		Pressed += ChangeScene;
	}

	private void ChangeScene()
	{
		currentScene.Visible = false;
		nextScene.Visible = true;
	}
}
