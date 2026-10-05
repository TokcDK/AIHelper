using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Data.Modlist;
using AIHelper.Manage.Update;
using AIHelper.Manage.Update.Sources;
using NLog;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// Mods which are updated from a github repository.
    /// </summary>
    /// <remarks>
    /// The source is stateless: one instance serves every block and the add tab, and
    /// everything which differs between two mods travels in the data it is handed. That is
    /// what lets the editor hold a list of sources without knowing what any of them are.
    /// </remarks>
    internal sealed class GithubUpdateSource : IUpdateSource
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>Section and key Mod Organizer keeps the mod url in.</summary>
        private const string UrlSection = "General";
        private const string UrlKey = "url";

        /// <summary>The one instance: the source keeps no state, so a second one would only
        /// be a second object to keep in step.</summary>
        internal static readonly GithubUpdateSource Instance = new GithubUpdateSource();

        private GithubUpdateSource()
        {
        }

        public string Name
        {
            get { return T._("Github"); }
        }

        public bool TryReadData(ModData mod, out UpdateSourceData data)
        {
            data = null;

            var ini = mod.MetaIni;
            if (ini == null) return false; // a mod without meta.ini carries nothing

            var url = ini.GetKey(UrlSection, UrlKey);
            var updateInfo = ini.GetKey(ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyUpdateName);
            if (string.IsNullOrWhiteSpace(url) && string.IsNullOrWhiteSpace(updateInfo)) return false;

            var githubData = new GithubUpdateData(mod);

            // A stored record is only read when it really is one. The value may equally well
            // be a plain note, and taking "first,second" out of a sentence would invent an
            // owner and a repository.
            GithubUpdateRecord storedRecord = null;
            var hasStoredRecord = !string.IsNullOrWhiteSpace(updateInfo)
                                  && updateInfo.TrimStart().StartsWith(
                                      UpdateRecordParser.SourceId, StringComparison.OrdinalIgnoreCase)
                                  && UpdateRecordParser.TryParseStoredValue(updateInfo, out storedRecord);

            if (hasStoredRecord)
            {
                githubData.Owner = storedRecord.Owner;
                githubData.Repository = storedRecord.Repository;
                githubData.FileStartsWith = storedRecord.FileStartsWith;
                githubData.FileEndsWith = storedRecord.FileEndsWith;
                githubData.VersionFromFile = storedRecord.VersionFromFile;

                data = githubData;
                return true;
            }

            // No record: the url Mod Organizer shows is the other place the coordinates may
            // be. When neither yields them the mod is not one this source can show, and
            // saying so is what keeps a plain note from becoming a mod entry.
            string owner;
            string repository;
            if (!GithubUrl.TryParse(url, out owner, out repository)) return false;

            githubData.Owner = owner;
            githubData.Repository = repository;

            data = githubData;
            return true;
        }

        public UpdateSourceInfoControl CreateInfoControl(UpdateSourceData data, ToolTip toolTip)
        {
            // The coordinates are five plain fields, so the shared control which builds a
            // field per writable property shows them as they are.
            return new UpdateSourceInfoControl(data, toolTip);
        }

        public UpdateSourceAddControl CreateAddControl(Action<string> log)
        {
            return new GithubAddModControl(log);
        }

        public string GetWebUrl(UpdateSourceData data)
        {
            var githubData = data as GithubUpdateData;
            if (githubData == null) return null;

            // The newest release is what a user opening the page of a mod wants to see, and
            // the release list is where the file the record describes lives.
            return GithubUrl.BuildRepositoryUrl(
                UpdateRecordParser.SourceHost, githubData.Owner, githubData.Repository) + "/releases/latest";
        }

        public async Task<bool> DownloadAndInstallAsync(UpdateSourceData data, string targetFolderPath,
            Action<string> log, CancellationToken cancellationToken)
        {
            var githubData = data as GithubUpdateData;
            if (githubData == null)
            {
                // Not reachable through the editor, which only ever hands back the data its
                // own add control built. Reported rather than thrown so that a mistake here
                // cannot take the window down.
                _log.Error("Github source was handed data of another source.");
                return false;
            }

            using (var engine = new UpdateEngineFactory())
            {
                var probe = new GithubUpdateProbe(engine.GithubSource);

                var latest = await probe
                    .FindLatestAsync(githubData.ToRecord(), cancellationToken)
                    .ConfigureAwait(true);

                if (latest == null || string.IsNullOrWhiteSpace(latest.VersionString))
                {
                    log(T._("Cant get last version"));
                    return false;
                }

                if (!await probe
                        .DownloadAndInstallAsync(latest, targetFolderPath, cancellationToken)
                        .ConfigureAwait(true))
                {
                    log(T._("Cant download file!"));
                    return false;
                }
            }

            return true;
        }
    }
}
