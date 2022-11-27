﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIHelper.Manage;
using INIFileMan;

namespace AIHelper.Data.Modlist
{
    internal class ModlistData
    {
        internal string ModlistPath = "";
        internal List<ModData> Mods = new List<ModData>();
        internal Dictionary<string, ModData> ModsByName = new Dictionary<string, ModData>();
        internal List<ModData> ModsPlusOverwrite { get => new List<ModData>(Mods).Concat(new ModData[1] { Overwrite }).ToList(); }
        internal Dictionary<string, ModData> ModsByNameAndOverwrite { get => new Dictionary<string, ModData>(ModsByName).Concat(new Dictionary<string, ModData> { { Overwrite.Name, Overwrite } }).ToDictionary(k => k.Key, v => v.Value); }
        
        /// <summary>
        /// Overwrite is always exists and enabled
        /// </summary>
        internal ModData Overwrite = new ModData
        {
            Priority = 999999,
            IsOverwrite = true,
            IsEnabled = true,
            IsSeparator = false,
            Name = "Overwrite",
            IsExist = true,
            ParentSeparator = null,
            Path = ManageSettings.CurrentGameOverwriteFolderPath
        };

        internal const string ListDescriptionMarker = "# This file was automatically generated by Mod Organizer.";
        internal const string SeparatorMarker = "_separator";

        /// <summary>
        /// init and load content of modlist for current profile
        /// </summary>
        public ModlistData() { Load(); }

        /// <summary>
        /// init and load content of modlist for selected path
        /// </summary>
        public ModlistData(string modListPath) { Load(modListPath); }

        /// <summary>
        /// modlist load
        /// </summary>
        void Load(string modListPath = null)
        {
            // set modlist path depending on input path
            ModlistPath = modListPath ?? ManageSettings.CurrentMoProfileModlistPath;

            if (!File.Exists(ModlistPath)) return;

            // read modlist file
            var modlistContent = File.ReadAllLines(ManageSettings.CurrentMoProfileModlistPath);

            // fill mod data from modlist
            var modPriority = 0;
            ModData lastSeparator = null;
            foreach (var line in modlistContent)
            {
                if (string.IsNullOrWhiteSpace(line)) continue; // empty
                if (line.StartsWith("#", StringComparison.InvariantCulture)) continue; // comment

                // init mod data
                var indexOfSeparatorMarker = line.IndexOf(SeparatorMarker, StringComparison.InvariantCulture);
                var modName = line.Substring(1);
                var modPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, modName);
                var mod = new ModData
                {
                    Priority = modPriority++,
                    IsEnabled = line[0] == '+',
                    IsSeparator = indexOfSeparatorMarker > -1,
                    Name = modName,
                    Path = modPath,
                    ParentSeparator = lastSeparator
                };

                // get meta.ini data if exists
                var metaIniPath = Path.Combine(mod.Path, "meta.ini");
                if (File.Exists(metaIniPath)) mod.MetaIni = ManageIni.GetINIFile(metaIniPath);

                // add subitems references to understand which items is under the group separator
                if (!mod.IsSeparator && lastSeparator != null) lastSeparator.Childs.Add(mod);

                // reset separator if was changed
                if (mod.IsSeparator && lastSeparator != mod.ParentSeparator) lastSeparator = mod;

                // add mod into lists
                Mods.Add(mod);
                ModsByName.Add(mod.Name, mod);

                modPriority++;
            }

            //// set and add overwrite as mod
            //var owerwrite = new ModData
            //{
            //    Priority = 999999,
            //    IsOverwrite = true,
            //    IsEnabled = true,
            //    IsSeparator = false,
            //    Name = "Overwrite",
            //    IsExist = true,
            //    ParentSeparator = null,
            //    Path = ManageSettings.CurrentGameOverwriteFolderPath
            //};
            //Mods.Add(owerwrite);
            //ModsByName.Add(owerwrite.Name, owerwrite);
        }

        {
            {
            }
        }

            {
                {

                    }

                    item.Priority = modPriority;
                    newItems.Add(item);

                    modPriority++;
                }

                {


            if (!string.IsNullOrWhiteSpace(modNameToPlaceWith))
            {


                }

                if (!added) // add in the end when was not added
                {
                    modToInsert.Priority = modPriority;
                    modToInsert.ParentSeparator = null;
                    newItems.Add(modToInsert);
                }



            // update modbyname
            ModsByName = Mods.ToDictionary(k => k.Name, v => v);

        }

        {
            if (ModsByName.ContainsKey(itemName)) return ModsByName[itemName];

            return null;
        }

        /// <summary>
        /// get list of mods from all items by selected mod type
        /// </summary>
        /// <param name="modType">Enabled, Disabled, Separator</param>
        /// <returns>list of mods by mod type</returns>
        internal List<ModData> GetListBy(ModType modType) { return GetBy(modType).ToList(); }

        /// <summary>
        /// get mods from all items by selected mod type
        /// </summary>
        /// <param name="modType">Enabled, Disabled, Separators</param>
        /// <param name="exists">True by default. Determines if add only existing mod folders</param>
        /// <returns>list of mods by mod type</returns>
        internal IEnumerable<ModData> GetBy(ModType modType, bool exists = true)
        {
            foreach (var mod in Mods)
            {
                switch (modType)
                {
                    case ModType.Separator when mod.IsSeparator:
                    case ModType.ModAny when !mod.IsSeparator:
                    case ModType.ModEnabled when !mod.IsSeparator && mod.IsEnabled:
                    case ModType.ModEnabledAndOverwrite when !mod.IsSeparator && mod.IsEnabled:
                    case ModType.ModDisabled when !mod.IsSeparator && !mod.IsEnabled:
                        if (!exists || mod.IsExist) yield return mod; // mod exists or exists ver is false
                        break;
                }
            }

            if (modType == ModType.ModEnabledAndOverwrite) yield return Overwrite; // return overwrite in the end when it need
        }

        /// <summary>
        /// mod type
        /// </summary>
        internal enum ModType
        {
            /// <summary>
            /// any enabled or disabled mods
            /// </summary>
            ModAny,
            /// <summary>
            /// only enabled mods
            /// </summary>
            ModEnabled,
            /// <summary>
            /// only disabled mods
            /// </summary>
            ModDisabled,
            /// <summary>
            /// only separators
            /// </summary>
            Separator,
            /// <summary>
            /// only enabled mods plus overwrite
            /// </summary>
            ModEnabledAndOverwrite,
        }

        /// <summary>
        /// save modlist
        /// </summary>
        internal void Save()
        {
            if (!File.Exists(ModlistPath)) return;

            var writeItems = new List<ModData>(Mods);
            writeItems.Reverse();
            using (var newModlist = new StreamWriter(ModlistPath))
            {
                newModlist.WriteLine(ListDescriptionMarker);
                foreach (var item in writeItems) newModlist.WriteLine((item.IsEnabled ? "+" : "-") + item.Name);
            }
        }
    }

    internal class ModData
    {
        /// <summary>
        /// priority in modlist
        /// </summary>
        internal int Priority = -1;
        /// <summary>
        /// name of mod dir or name of separator dir
        /// </summary>
        internal string Name;
        /// <summary>
        /// path to folder
        /// </summary>
        internal string Path;
        /// <summary>
        /// true when enabled. separators is always disabled
        /// </summary>
        internal bool IsEnabled;
        /// <summary>
        /// Determines if mod is overwrite folder
        /// </summary>
        internal bool IsOverwrite = false;
        /// <summary>
        /// true when folder is exists in mods
        /// </summary>
        /// <summary>
        /// true for separators
        /// </summary>
        internal bool IsSeparator = false;
        /// <summary>
        /// parent separator
        /// </summary>
        internal ModData ParentSeparator;
        /// <summary>
        /// child items for separator
        /// </summary>
        internal List<ModData> Childs = new List<ModData>();
        /// <summary>
        /// Mod relations with other mods
        /// </summary>
        internal ModRelationsData Relations = new ModRelationsData();
        /// <summary>
        /// Mod messages
        /// </summary>
        internal List<string> ReportMessages = new List<string>();
        /// <summary>
        /// Ini file content for the mod
        /// </summary>
        internal INIFile MetaIni { get; set; }
    }

    internal class ModRelationsData
    {
        internal List<ModData> Requires = new List<ModData>();
        internal List<ModData> RequiresOr = new List<ModData>();
        internal List<ModData> IncompatibleWith = new List<ModData>();
        internal List<ModData> IncompatibleWithOr = new List<ModData>();
    }

    internal class ModListReportMessage
    {
        internal string Message { get; }

        public ModListReportMessage(string message)
        {
            Message = message;
        }
    }
}
