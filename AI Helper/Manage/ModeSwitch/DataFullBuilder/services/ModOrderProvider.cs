using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIHelper.Manage.ModeSwitch.DataFullBuilder.Interfaces;

namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Services
{
    public sealed class ModOrderProvider : IModOrderProvider
    {
        private readonly string _loadOrderFilePath;
        private readonly string _modsFolderPath;
        private readonly IFileSystemHelper _fileSystemHelper;
        private const string MetaFileName = "meta.ini";

        public ModOrderProvider(
            string loadOrderFilePath, 
            string modsFolderPath,
            IFileSystemHelper fileSystemHelper)
        {
            _loadOrderFilePath = loadOrderFilePath 
                ?? throw new ArgumentNullException(nameof(loadOrderFilePath));
            _modsFolderPath = modsFolderPath 
                ?? throw new ArgumentNullException(nameof(modsFolderPath));
            _fileSystemHelper = fileSystemHelper 
                ?? throw new ArgumentNullException(nameof(fileSystemHelper));
        }

        public IReadOnlyList<string> GetOrderedModFolders()
        {
            var validMods = new List<string>();
            
            if (!File.Exists(_loadOrderFilePath))
            {
                return validMods;
            }

            var lines = File.ReadAllLines(_loadOrderFilePath).Reverse(); // reverse to get correct load order because highest priority mod names is going first

            foreach (var line in lines)
            {
                var modName = line?.Trim();
                
                if (string.IsNullOrEmpty(modName)) continue;
                if (modName.StartsWith("#")) continue; // Skip comments
                if (modName.StartsWith("-")) continue; // Skip disabled mods
                if (modName.EndsWith("_separator")) continue; // Skip separators

                if (modName.StartsWith("+"))
                {
                    modName = modName.Substring(1).Trim();
                }

                var modPath = Path.Combine(_modsFolderPath, modName);

                // Validate mod folder
                if (_fileSystemHelper.DirectoryExists(modPath))
                {
                    validMods.Add(modName);
                }
            }

            return validMods;
        }
    }
}
