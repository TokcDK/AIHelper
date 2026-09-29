using System;
using System.Collections.Generic;
using System.Linq;
using GetListOfSubClasses;
using NLog;

namespace AIHelper.Games
{
    /// <summary>
    /// The game presets of the app, found by reflection.
    /// </summary>
    /// <remarks>
    /// A preset is discovered rather than registered: every non abstract subclass of
    /// <see cref="GameBase"/> is one, so a new game only has to add a class. Reflection
    /// does not promise a stable order, and a folder which holds the exes of two games
    /// would be claimed by a different preset on the next run, so the presets are sorted
    /// by type name and the list is built once.
    /// </remarks>
    internal static class GamePresets
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        static IReadOnlyList<GameBase> _all;

        /// <summary>
        /// One instance of every preset, ordered by type name.
        /// </summary>
        /// <remarks>
        /// The instances are shared, so only members which are constant for the preset may
        /// be read from them: a member which dereferences <see cref="GameBase.GameDirInfo"/>
        /// or the current game throws while no game is selected. Use <see cref="Types"/>
        /// when the folder of a game has to be kept in the instance.
        /// </remarks>
        internal static IReadOnlyList<GameBase> All
        {
            get { return _all ?? (_all = CreateAll()); }
        }

        /// <summary>
        /// The preset types, in the same order as <see cref="All"/>.
        /// </summary>
        /// <remarks>
        /// A game which was found gets an instance of its own, because the folder of the
        /// game is kept in it and the shared instances must stay untouched.
        /// </remarks>
        internal static IEnumerable<Type> Types
        {
            get
            {
                foreach (var preset in All)
                {
                    yield return preset.GetType();
                }
            }
        }

        /// <summary>
        /// Looks for the preset an exe belongs to.
        /// </summary>
        /// <param name="exeName">exe file name, with or without the extension</param>
        /// <param name="preset">the preset which claims the name, or null</param>
        /// <returns>true when a preset was found</returns>
        internal static bool TryFindByExeName(string exeName, out GameBase preset)
        {
            preset = null;

            if (string.IsNullOrWhiteSpace(exeName))
            {
                return false;
            }

            foreach (var candidate in All)
            {
                bool isKnownExeName;
                try
                {
                    isKnownExeName = candidate.IsKnownExeName(exeName);
                }
                catch (Exception ex)
                {
                    // a preset which cannot be read must not cost the exe names of the others
                    _log.Warn("Failed to read the exe names of the preset " + candidate.GetType().Name + ". Error:\r\n" + ex);
                    continue;
                }

                if (!isKnownExeName)
                {
                    continue;
                }

                preset = candidate;
                return true;
            }

            return false;
        }

        private static List<GameBase> CreateAll()
        {
            var presets = new List<GameBase>();

            List<Type> presetTypes;
            try
            {
                presetTypes = Inherited.GetListOfInheritedTypes(typeof(GameBase));
            }
            catch (Exception ex)
            {
                // the app can still run without a preset, it just cannot find a game
                _log.Error("Failed to read the list of game preset types. Error:\r\n" + ex);
                return presets;
            }

            foreach (var presetType in presetTypes.OrderBy(type => type.Name, StringComparer.Ordinal))
            {
                try
                {
                    presets.Add((GameBase)Activator.CreateInstance(presetType));
                }
                catch (Exception ex)
                {
                    // a preset which cannot be created must not cost the other presets
                    _log.Warn("Failed to create the game preset " + presetType.Name + ". Error:\r\n" + ex);
                }
            }

            return presets;
        }
    }
}
