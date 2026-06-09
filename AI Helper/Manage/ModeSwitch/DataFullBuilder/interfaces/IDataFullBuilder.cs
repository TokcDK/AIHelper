
using AIHelper.Manage.ModeSwitch.DataFullBuilder.Models;

namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.interfaces
{
    public interface IDataFullBuilder
    {
        BuildResult Build(BuildConfiguration configuration);
    }
}
