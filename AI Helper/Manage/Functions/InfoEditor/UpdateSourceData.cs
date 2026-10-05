using AIHelper.Data.Modlist;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// The data one update source holds for one mod, as the editor edits it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A concrete source's data is the model its fields are bound to, which is why the
    /// properties the user can edit are public: a binding resolves its path through
    /// <see cref="System.ComponentModel.TypeDescriptor"/>, and that only sees public
    /// members. The types themselves stay internal — nothing outside this feature uses
    /// them.
    /// </para>
    /// <para>
    /// Reading the data out of a mod and writing it back are the source's business and live
    /// in the source's own implementation; what is declared here is only what the editor
    /// needs from any source's data to show a tab for it.
    /// </para>
    /// </remarks>
    internal abstract class UpdateSourceData
    {
        protected UpdateSourceData(ModData mod, IUpdateSource source)
        {
            Mod = mod;
            Source = source;
        }

        public ModData Mod { get; }

        /// <summary>
        /// Source the data belongs to. The block's buttons ask it for the page of the mod,
        /// so that the buttons do not have to know which host they are looking at.
        /// </summary>
        internal IUpdateSource Source { get; }

        /// <summary>Text this data contributes to the filter of the mod.</summary>
        internal abstract string FilterText { get; }

        /// <summary>
        /// True when this data and the other describe the same release file, which is what
        /// makes a second mod fed by it a duplicate.
        /// </summary>
        /// <param name="other">Data to compare with, possibly of another source.</param>
        internal abstract bool PointsAtSameFileAs(UpdateSourceData other);

        /// <summary>
        /// Caption and explanation of one of this data's fields, when it has them.
        /// </summary>
        /// <param name="propertyName">Name of the field's property.</param>
        /// <param name="caption">Caption to show, or null.</param>
        /// <param name="description">Explanation to show as a tooltip, or null.</param>
        /// <returns>True when the field is one the source knows how to label.</returns>
        internal abstract bool TryGetFieldText(string propertyName, out string caption, out string description);

        /// <summary>
        /// Stores the data in the mod's own files, where the update pass reads it back.
        /// </summary>
        internal abstract void Write();
    }
}
