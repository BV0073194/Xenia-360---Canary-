using System;
using System.IO;

namespace Xenia_360____Canary_.Services
{
    public class ToolManagerService
    {
        public readonly string ToolsPath;
        public readonly string Aria2Path;
        public readonly string ExtractXisoPath;
        public readonly string VfsDumpPath;

        public ToolManagerService()
        {
            ToolsPath = Path.Combine(AppContext.BaseDirectory, "Tools");
            Aria2Path = Path.Combine(ToolsPath, "aria2c.exe");
            ExtractXisoPath = Path.Combine(ToolsPath, "extract-xiso.exe");
            VfsDumpPath = Path.Combine(ToolsPath, "xenia-vfs-dump.exe");

            ValidateTool(Aria2Path, "aria2c.exe");
            ValidateTool(ExtractXisoPath, "extract-xiso.exe");
            ValidateTool(VfsDumpPath, "xenia-vfs-dump.exe");
        }

        private void ValidateTool(string path, string name)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Required tool '{name}' not found in Tools folder. Please create a 'Tools' folder next to the application executable and place {name} inside.", path);
        }
    }
}