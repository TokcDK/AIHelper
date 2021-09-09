using AIHelper.Manage.ModeSwitch;

namespace AIHelper.Manage
{
    class ManageMOModeSwitch
    {
        internal static void SwitchBetweenMoAndStandartModes()
        {
            if (ManageSettings.IsMoMode())
            {
                new ToCommonMode().Switch();
            }
            else
            {
                new ToMOMode().Switch();
            }
        }
    }
}
