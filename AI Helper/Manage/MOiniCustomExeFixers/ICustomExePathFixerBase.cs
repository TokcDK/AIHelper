using static AIHelper.Manage.ManageModOrganizer.CustomExecutables;

namespace AIHelper.Manage.MOiniCustomExeFixers
{
    /// <summary>
    /// One custom executable attribute which has to be brought back to the machine it is
    /// being used on, together with the ini record it belongs to.
    /// </summary>
    internal class CustomExeFixData
    {
        internal string Path;
        internal string Attribute;
        internal CustomExecutable CustomExeData;
    }

    /// <summary>
    /// Rebuilds a path stored in an MO custom executable ini when it points at a place
    /// this machine does not have.
    /// </summary>
    /// <remarks>
    /// The implementations are independent of each other on purpose: each one knows the
    /// shape of the attribute it repairs, and the order they run in is decided by the
    /// caller.
    /// </remarks>
    internal interface ICustomExePathFixerBase
    {
        void TryFix(CustomExeFixData data);
    }
}
