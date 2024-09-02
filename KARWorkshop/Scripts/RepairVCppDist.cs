using Godot;
using System;
using System.Diagnostics;
using System.IO;

//invokes the Visual C++ Distributon 2015-2022 installer
public partial class RepairVCppDist : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += InvokeVCInstaller;
	}

	//invokes the installer and repairs
	void InvokeVCInstaller()
	{
		DirectoryInfo installDir = new DirectoryInfo(System.Environment.CurrentDirectory);

		var installer = new Process();
		installer.StartInfo.FileName = KWStructure.GenerateKWStructure_Directory_Tools(installDir).FullName + "/Windows/VC_redist.x64.exe";
		installer.StartInfo.WorkingDirectory = installDir.FullName;
		installer.Start();
	}
}
