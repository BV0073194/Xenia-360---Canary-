using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Xenia_360____Canary_.Models;

namespace Xenia_360____Canary_.Services;

public static class GameLibraryService
{
    private static readonly string romsFolder = Path.Combine(AppContext.BaseDirectory, "ROMS");
    private static readonly string libraryFile = Path.Combine(romsFolder, "GameLibrary.json");

    public static List<Game> Games { get; private set; } = new();

    public static void Load()
    {
        if (!Directory.Exists(romsFolder))
            Directory.CreateDirectory(romsFolder);

        if (File.Exists(libraryFile))
            Games = JsonConvert.DeserializeObject<List<Game>>(File.ReadAllText(libraryFile)) ?? new List<Game>();
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
        File.WriteAllText(libraryFile, JsonConvert.SerializeObject(Games, Newtonsoft.Json.Formatting.Indented));
    }
}
