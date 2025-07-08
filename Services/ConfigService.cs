using System;
using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace Xenia_360____Canary_.Services;

public class ConfigService
{
    private readonly string _configPath;
    private readonly string _backupPath;

    private TomlTable _originalConfig;

    public ConfigService(string configFilePath)
    {
        _configPath = configFilePath;
        _backupPath = _configPath + ".bak";

        BackupConfig();
    }

    private void BackupConfig()
    {
        if (File.Exists(_configPath))
            File.Copy(_configPath, _backupPath, true);

        var tomlString = File.ReadAllText(_configPath);
        _originalConfig = Toml.Parse(tomlString).ToModel();
    }

    public TomlTable LoadConfig()
    {
        var tomlString = File.ReadAllText(_configPath);
        return Toml.Parse(tomlString).ToModel();
    }

    public void SaveConfig(TomlTable config)
    {
        var tomlString = Toml.FromModel(config);
        File.WriteAllText(_configPath, tomlString);
    }

    public void RestoreBackup()
    {
        if (File.Exists(_backupPath))
            File.Copy(_backupPath, _configPath, true);
    }
}
