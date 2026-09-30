namespace AIHelper.Manage.Update
{
    /// <summary>
    /// One folder an update record applies to: a mod folder below the current
    /// game's mods directory, or the Mod Organizer installation itself.
    /// <para>
    /// Deliberately a plain pair and not the engine's <c>UpdateTarget</c>, so the
    /// code that reads the records does not have to know the engine exists.
    /// </para>
    /// </summary>
    internal sealed class UpdatableFolder
    {
        internal UpdatableFolder(string folderPath, GithubUpdateRecord record)
        {
            FolderPath = folderPath ?? string.Empty;
            Record = record;
        }

        /// <summary>Absolute path of the folder to update.</summary>
        internal string FolderPath { get; }

        /// <summary>Where the new version of the folder comes from.</summary>
        internal GithubUpdateRecord Record { get; }
    }
}
