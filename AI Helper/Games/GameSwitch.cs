using System;
using AIHelper.Manage;

namespace AIHelper.Games
{
    /// <summary>
    /// Makes one of the found games the game the application works with.
    /// </summary>
    /// <remarks>
    /// The rest of the application reads the current game through
    /// <see cref="ManageSettings"/>, and part of that state is built from the game
    /// folder: the Mod Organizer files are links to the ones of the game, and the
    /// paths which are resolved per game are cached on first read. Resetting those
    /// caches is what this type is for - nothing which belongs to the game being
    /// left may be read after the switch.
    /// </remarks>
    internal static class GameSwitch
    {
        /// <summary>
        /// Makes <paramref name="game"/> the current game and rebuilds the state which
        /// belongs to it.
        /// </summary>
        /// <param name="game">game to work with</param>
        /// <remarks>
        /// The order matters: the game itself is set first, because everything below
        /// reads the current game through it.
        /// </remarks>
        internal static void Apply(GameBase game)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));

            ManageSettings.Games.Game = game;

            // the Mod Organizer data of the game is maintained only while Mod Organizer is
            // installed: the files of the app which link to it are not read without it
            if (ManageModOrganizer.IsInstalled)
            {
                // the Mod Organizer files of the app are links to the ones of the current
                // game, so they are relinked before anything reads them
                ManageModOrganizer.RedefineGameMoData();

                ManageModOrganizer.CheckBaseGamesPy();
            }

            // both are resolved for the current game on first read and cached, so the
            // values of the game being left must not survive the switch
            ManageSettings.BepInExCfgFilePath = string.Empty;
            ManageSettings.MOSelectedProfileDirName = string.Empty;

            // the preset of the new game gets its chance to prepare the game folder
            game.InitActions();
        }
    }
}
