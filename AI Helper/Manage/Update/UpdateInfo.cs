using AIHelper.Manage.Update.Sources;
using AIHelper.Manage.Update.Targets;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AIHelper.Manage.Update
{
    internal class UpdateInfo
    {
        internal string SourceId;
        internal string TargetCurrentVersion;
        internal string SourceLink;
        internal string TargetLastVersion;
        internal HashSet<string> Excluded;
        internal DirectoryInfo TargetFolderPath;
        internal string[] TargetFolderUpdateInfo;
        /// <summary>
        /// report content about update
        /// </summary>
        internal List<string> Report;
        internal bool NoRemoteFile;
        /// <summary>
        /// path to update file
        /// </summary>
        internal string UpdateFilePath;
        /// <summary>
        /// string with wich update file name starts
        /// </summary>
        internal string UpdateFileStartsWith;
        /// <summary>
        /// string with wich update file name ends
        /// </summary>
        internal string UpdateFileEndsWith;
        /// <summary>
        /// buckup dir path for old versions of mods
        /// </summary>
        internal string BuckupDirPath;
        /// <summary>
        /// selected source
        /// </summary>
        internal UpdateSourceBase Source;
        /// <summary>
        /// selected target
        /// </summary>
        internal UpdateTargetBase Target;
        internal bool VersionFromFile;
        //internal bool GetVersionFromLink;
        //internal Dictionary<string, long> UrlSizeList;
        /// <summary>
        /// file download link
        /// </summary>
        public string DownloadLink { get; internal set; }
        /// <summary>
        /// last error text if any
        /// </summary>
        public StringBuilder LastErrorText { get; internal set; }

        public UpdateInfo()
        {
            Excluded = new HashSet<string>();
            Report = new List<string>();
            LastErrorText = new StringBuilder();
            Reset();
        }

        /// <summary>
        /// vars need to be reset for each folder
        /// </summary>
        internal void Reset()
        {
            TargetCurrentVersion = "";
            SourceLink = "";
            TargetLastVersion = "";
            TargetFolderPath = null;
            TargetFolderUpdateInfo = null;
            NoRemoteFile = false;
            UpdateFilePath = "";
            UpdateFileStartsWith = "";
            UpdateFileEndsWith = "";
            DownloadLink = "";
            LastErrorText.Clear();
        }

    }
}
