using System;
using System.IO;

namespace DucommForge.Data;

public static class AppPaths
{
    public static string GetDbPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(localAppData, "DUCOMM", "DucommForge");
        Directory.CreateDirectory(appFolder);
        return Path.Combine(appFolder, "ducomm_forge.db");
    }
}