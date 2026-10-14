using System;

namespace AIHelper.Manage.ToolsTab
{
    public interface IButtonData
    {
        string Text { get; }
        string Description { get; }

        void OnClick(object o, EventArgs e);
    }

    /// <summary>
    /// Optional part of a button description: whether the button can be used right now.
    /// </summary>
    /// <remarks>
    /// A button which cannot run is shown disabled rather than failing when it is pressed,
    /// so the user sees which actions need something that is not there. A description
    /// which does not implement this is always available, which keeps the check out of the
    /// buttons which do not need it.
    /// </remarks>
    public interface IButtonDataAvailability
    {
        bool IsAvailable { get; }
    }

    public interface IButtonDataDev
    {
        bool IsVisible { get; }
    }
}
