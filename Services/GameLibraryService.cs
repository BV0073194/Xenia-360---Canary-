using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Xenia_360____Canary_.Models;
using Microsoft.Maui.Storage; // Add this using statement

namespace Xenia_360____Canary_.Services;

public static class GameLibraryService
{
    // --- FIX: Use FileSystem.AppDataDirectory for a reliable path ---
    private static readonly string romsFolder = Path.Combine(FileSystem.AppDataDirectory, "ROMS");
    private static readonly string libraryFile = Path.Combine(romsFolder, "GameLibrary.json");

    public static List<Game> Games { get; private set; } = new();

    public static void Load()
    {
        if (!Directory.Exists(romsFolder))
            Directory.CreateDirectory(romsFolder);

        if (File.Exists(libraryFile))
        {
            var json = File.ReadAllText(libraryFile);
            Games = JsonConvert.DeserializeObject<List<Game>>(json) ?? new List<Game>();
        }
    }

    public static void AddGame(Game game)
    {
        // Avoid duplicates (by folder path)
        if (!Games.Exists(g => g.GameFolderPath == game.GameFolderPath))
        {
            Games.Add(game);
            Save();
        }
    }

    public static void Save()
    {
        var json = JsonConvert.SerializeObject(Games, Formatting.Indented);
        File.WriteAllText(libraryFile, json);
    }
}