using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xenia_360____Canary_.Models;

namespace Xenia_360____Canary_.Services;

public static class GameLibraryService
{
    private static readonly string RomsFolder = Path.Combine(AppContext.BaseDirectory, "ROMS");
    private static readonly string LibraryFile = Path.Combine(RomsFolder, "GameLibrary.json");

    public static List<Game> Games { get; private set; } = new();

    public static void Load()
    {
        if (!Directory.Exists(RomsFolder))
            Directory.CreateDirectory(RomsFolder);

        if (File.Exists(LibraryFile))
        {
            var json = File.ReadAllText(LibraryFile);
            Games = JsonConvert.DeserializeObject<List<Game>>(json) ?? new List<Game>();
        }
    }

    public static void AddOrUpdateGame(Game game)
    {
        var existingGame = Games.FirstOrDefault(g => g.GameFolderPath == game.GameFolderPath);
        if (existingGame != null)
        {
            // Update existing game entry
            existingGame.TitleID = game.TitleID;
            existingGame.TitleName = game.TitleName;
            existingGame.ExecutablePath = game.ExecutablePath;
            existingGame.ContentID = game.ContentID;
            existingGame.LaunchPath = game.LaunchPath;
        }
        else
        {
            Games.Add(game);
        }
        Save();
    }

    public static void RemoveGame(Game game)
    {
        Games.Remove(game);
        // Optionally, delete game files from disk
        // if (Directory.Exists(game.GameFolderPath))
        // {
        //     Directory.Delete(game.GameFolderPath, true);
        // }
        Save();
    }

    public static void Save()
    {
        var json = JsonConvert.SerializeObject(Games, Formatting.Indented);
        File.WriteAllText(LibraryFile, json);
    }
}