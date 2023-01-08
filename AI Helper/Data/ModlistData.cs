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
            Array.Reverse(modlistContent); // lines in modlist file are reversed

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
            }
        }

        /// <summary>
        /// Mods meta.ini file content as <see cref="INIFile"/>.
        /// </summary>
        /// <param name="modType"></param>
        /// <returns></returns>
        public IEnumerable<INIFile> EnumerateModsMetaIni()
        {
            foreach (var mod in Mods)
            {
                var ini = mod.MetaIni;
                if (ini == null) continue;

                yield return ini;
            }
        }

        /// <summary>
        /// Mods meta.ini file content as <see cref="INIFile"/>. Mods filtered by <paramref name="modType"/>
        /// </summary>
        /// <param name="modType"></param>
        /// <returns></returns>
        public IEnumerable<INIFile> EnumerateModsMetaIni(ModType modType)
        {
            foreach (var mod in GetBy(modType))
            {
                var iniPath = Path.Combine(mod.Path, "meta.ini");
                if (!File.Exists(iniPath)) continue;

                yield return ManageIni.GetINIFile(iniPath);
            }
        }

        internal void Insert(ModData modToInsert, string modNameToPlaceWith = "", bool insertAfter = true, bool skipIfExists = true, bool saveAfterInsert = true)
        {
            // skip when mod already exists
            var existsMod = GetModByName(modToInsert.Name);
            if (existsMod != null) 
            { 
                if (skipIfExists) return; 

                Mods.Remove(existsMod); 
            }

            // try insert by mod name
            if (TryInsertByName(modToInsert, modNameToPlaceWith, insertAfter)) return;

            // insert by priority if was not inserted by mod name
            if (modToInsert.Priority == -1) Mods.Add(modToInsert);
            else Mods.Insert(modToInsert.Priority, modToInsert);

            // update priority
            int priority = 0;
            foreach (var mod in Mods) mod.Priority = priority++;

            // update modbyname
            ModsByName = Mods.ToDictionary(k => k.Name, v => v);

            // save when need
            if (saveAfterInsert) Save();
        }

        private bool TryInsertByName(ModData modToInsert, string modNameToPlaceWith, bool insertAfter)
        {
            if (string.IsNullOrWhiteSpace(modNameToPlaceWith)) return false;

            var modToPlace = GetModByName(modNameToPlaceWith);
            if (modToPlace == null) return false;

            Mods.Insert(modToPlace.Priority + (insertAfter ? 1 : 0), modToInsert); // insert after or before

            if (modToPlace.IsSeparator) modToInsert.ParentSeparator = modToPlace; // set separator if need

            return true;
        }

        private ModData GetModByName(string itemName)
        {
            if (ModsByName.TryGetValue(itemName, out ModData value)) return value;

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

            // return overwrite as last mod when need
            if (modType == ModType.ModEnabledAndOverwrite) yield return Overwrite;
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
        internal bool IsExist { get => Directory.Exists(Path); }
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
        internal ModRelationsData Relations = null;
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
}
