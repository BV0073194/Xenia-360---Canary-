using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace Xenia_360____Canary_.Services;

public class ConfigService
{
    public TomlTable LoadConfig(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException("Config file not found.", configPath);
        }
        var tomlString = File.ReadAllText(configPath);
        return Toml.Parse(tomlString).ToModel();
    }

    public void SaveConfig(string configPath, TomlTable config)
    {
        var tomlString = Toml.FromModel(config);
        File.WriteAllText(configPath, tomlString);
    }

    public void BackupConfig(string configPath)
    {
        if (File.Exists(configPath))
        {
            var backupPath = configPath + ".bak";
            File.Copy(configPath, backupPath, true);
        }
    }

    public void RestoreBackup(string configPath)
    {
        var backupPath = configPath + ".bak";
        if (File.Exists(backupPath))
        {
            File.Copy(backupPath, configPath, true);
        }
    }
}