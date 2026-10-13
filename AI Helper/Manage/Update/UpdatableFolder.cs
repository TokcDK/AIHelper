namespace AIHelper.Manage.Update
{
    /// <summary>
    /// What a folder an update record applies to is, which is what decides how the update
    /// pass treats it.
    /// </summary>
    /// <remarks>
    /// Three places used to answer this question separately, each by comparing the folder
    /// against a path it had been handed: the pass, the version store and the backup. Naming
    /// it once, where the folder is described, is what lets a new kind of updatable folder be
    /// added without teaching all three about it.
    /// </remarks>
    internal enum UpdatableFolderKind
    {
        /// <summary>A mod folder below the current game's mods directory.</summary>
        Mod,

        /// <summary>The Mod Organizer installation itself.</summary>
        ModOrganizer,

        /// <summary>
        /// AI Helper itself. Its new version is unpacked into the payload folder and applied
        /// by aihelper-updater on the next start, because the files an update replaces are the
        /// ones the running process is made of.
        /// </summary>
        Application
    }

    /// <summary>
    /// One folder an update record applies to: a mod folder below the current game's mods
    /// directory, the Mod Organizer installation, or the application itself.
    /// <para>
    /// Deliberately a plain triple and not the engine's <c>UpdateTarget</c>, so the code that
    /// reads the records does not have to know the engine exists.
    /// </para>
    /// </summary>
    internal sealed class UpdatableFolder
    {
        /// <param name="folderPath">Absolute path of the folder to update.</param>
        /// <param name="record">Where the new version of the folder comes from.</param>
        /// <param name="kind">
        /// What the folder is. A mod is the default, because most of them are.
        /// </param>
        internal UpdatableFolder(string folderPath, GithubUpdateRecord record,
            UpdatableFolderKind kind = UpdatableFolderKind.Mod)
        {
            FolderPath = folderPath ?? string.Empty;
            Record = record;
            Kind = kind;
        }

        /// <summary>Absolute path of the folder to update.</summary>
        internal string FolderPath { get; }

        /// <summary>Where the new version of the folder comes from.</summary>
        internal GithubUpdateRecord Record { get; }

        /// <summary>What the folder is.</summary>
        internal UpdatableFolderKind Kind { get; }
    }
}
