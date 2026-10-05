using System;
using System.Windows.Forms;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// The control which adds a new mod of one update source.
    /// </summary>
    /// <remarks>
    /// It reads its own fields and raises <see cref="AddRequested"/>; it does not touch the
    /// mods folder. Which folder a mod lands in, whether its name may be used at all and
    /// whether a mod fed by the same release is already listed are the editor's questions,
    /// because the editor is the only part which knows the mods directory and the mods on
    /// screen.
    /// </remarks>
    internal abstract class UpdateSourceAddControl
    {
        protected UpdateSourceAddControl(Action<string> log)
        {
            Log = log;
        }

        /// <summary>
        /// Raised when the user asks for a mod to be added, with the form already read.
        /// </summary>
        internal event EventHandler<AddModRequestEventArgs> AddRequested;

        /// <summary>The control holding the whole form of this source.</summary>
        internal Control Control { get; private set; }

        /// <summary>Reports a line to the editor's log box.</summary>
        protected Action<string> Log { get; }

        /// <summary>
        /// Hands the built form to the base, so that the editor can put it in a tab.
        /// </summary>
        /// <remarks>
        /// Called once by the source's form, from its own constructor: the form cannot be
        /// built before the fields it is built from exist, and those are set in the body of
        /// that constructor, so it cannot be passed to this one.
        /// </remarks>
        protected void SetControl(Control control)
        {
            Control = control;
        }

        protected void RaiseAddRequested(AddModRequestEventArgs e)
        {
            var handler = AddRequested;
            if (handler != null) handler(this, e);
        }
    }

    /// <summary>
    /// One request to add a mod: the folder to add it under and the data which says what to
    /// download.
    /// </summary>
    /// <remarks>
    /// The data carries a mod of its own with the name and the priority already set; the
    /// path is left for the editor to fill in, because the mods directory is the editor's
    /// and the name is not a path until it has been checked against it.
    /// </remarks>
    internal sealed class AddModRequestEventArgs : EventArgs
    {
        internal AddModRequestEventArgs(string modDirectoryName, UpdateSourceData data)
        {
            ModDirectoryName = modDirectoryName;
            Data = data;
        }

        /// <summary>Name of the folder the mod is to be added as.</summary>
        internal string ModDirectoryName { get; }

        /// <summary>Data which says what to download.</summary>
        internal UpdateSourceData Data { get; }
    }
}
