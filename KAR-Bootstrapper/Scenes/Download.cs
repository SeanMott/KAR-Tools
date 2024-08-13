using Godot;
using System;
using System.Diagnostics;

public partial class Download : Button
{
	//starts a download
	private void _on_pressed()
	{
		//execute Duma for the download
		var p = new Process();
		p.StartInfo.FileName = "SupportProgs/Duma.exe";
		p.StartInfo.Arguments = "https://github.com/SeanMott/KARphin/releases/download/Replay_0.0.1/KARphin_Win_Replay_Prerelease.7z -O Test.7z";
		p.StartInfo.WorkingDirectory = "";
		p.Start();

		//hide the button
	}

}
