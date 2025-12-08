namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Models
{
    public sealed class BuildResult
    {
        public bool Success { get; set; }
        public int TotalFiles { get; set; }
        public int TotalDirectories { get; set; }
        public int HardLinksCreated { get; set; }
        public int SymbolicLinksCreated { get; set; }
        public int ErrorCount { get; set; }
        public string ErrorLogPath { get; set; }
        public long ElapsedMilliseconds { get; set; }
    }
}
