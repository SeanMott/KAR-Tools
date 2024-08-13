using Godot;
using System;

public partial class StartDownload : Node
{
	[Export] Node2D currentScene;
	[Export] Node2D downloadScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_pressed()
	{
		//hide this scene
		currentScene.Visible = false;

		//show the other download scene
		downloadScene.Visible = true;

		
	}

}
