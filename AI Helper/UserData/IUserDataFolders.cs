namespace AIHelper.UserData
{
    public interface IUserDataFolders
    {
        string Foldername { get; }

        string TargetFolderSuffix { get; }

        string Extension { get; }

        string TypeFolder { get; }

        string TargetFolderName { get; }
    }
}
