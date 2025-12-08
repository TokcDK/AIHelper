using System;

namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Interfaces
{
    public interface IErrorLogger : IDisposable
    {
        void LogError(string relativePath, string operation, Exception exception);
        void LogError(string relativePath, string operation, string message);
        void Flush();
    }
}
