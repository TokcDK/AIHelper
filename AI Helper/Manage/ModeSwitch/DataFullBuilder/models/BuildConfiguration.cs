namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Models
{
    public sealed class BuildConfiguration
    {
        public string DataFolderPath { get; set; }
        public string ModsFolderPath { get; set; }
        public string OverwriteFolderPath { get; set; }
        public string LoadOrderFilePath { get; set; }
        public string OutputFolderPath { get; set; }
        public int MaxDegreeOfParallelism { get; set; } = -1; // -1 = use all cores
    }
}
