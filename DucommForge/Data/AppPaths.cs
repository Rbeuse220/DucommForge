using System;
using System.IO;

namespace DucommForge.Data;

public static class AppPaths
{
    public static string AppDataDir =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DUCOMM",
            "DucommForge");
}