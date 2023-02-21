using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class Commands
{
    public static void RelaunchServer()
    {
        string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.ToString();
        string pathEXE = Path.Combine(path, "Launcher.exe");

        var activeProcesses = Process.GetProcessesByName("Launcher");
        foreach (var process in activeProcesses)
        {
            process.Kill();
        }

        if (File.Exists(pathEXE))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(pathEXE);
            startInfo.WorkingDirectory = path;
            startInfo.Arguments = "relaunch_server";
            Process.Start(startInfo);
        }
    }
}
