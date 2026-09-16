# Update subsystem — usage guide

How to use, extend and call the updater after the refactoring described in
`todo.md` and `technical_task.md`.

Target framework: **.NET Framework 4.7.2**, language level **C# 7.3**.
Everything in this folder lives in the namespace tree
`AIHelper.Manage.Update[.Sources|.Targets|.Targets.Mods]` and is `internal`
to the `AI Helper` assembly, so all callers below are other files of the same
project.

---

## 1. The pipeline in one paragraph

`Updater` owns the run. It holds a list of **sources** (`UpdateSourceBase`,
currently only `Github`) and a list of **targets** (`UpdateTargetBase`: `Mo`,
`ModsList`, `ModsMeta`). For every (source, target, folder) triple it asks the
target for its folder list, asks the source for the latest version of the
current folder, and — when the folder is out of date — downloads the file,
backs the folder up, applies the file, and restores the backup if anything
failed. One shared `UpdateInfo` instance carries the mutable state of the
folder currently being processed. A single failing folder is logged and the
pass continues.

```
Updater.Update(targets)
  └─ for each source
       └─ for each target
            ├─ target.GetUpdateInfos()          -> { folderKey -> updateRecord }
            └─ for each folder
                 ├─ info.Reset()
                 ├─ target.SetCurrentVersion()  -> info.TargetCurrentVersion
                 ├─ target.PrepareUpdateRequest()
                 ├─ source.GetLastVersion()     -> info.TargetLastVersion
                 ├─ source.CheckIfStopWork(info)
                 ├─ source.PauseAsync()
                 ├─ source.GetFile()            -> file on disk
                 ├─ target.MakeBackup()
                 ├─ target.UpdateFiles()
                 │    on success -> info.Excluded.Add(key), report "updated"
                 │    on failure -> target.RestoreBackup(), report "failed"
                 └─ (never throws: failure is reported, pass continues)
```

---

## 2. The update record

An update record is the string a target stores next to a folder and a source
parses. Two spellings exist; both are supported.

```
<sourceId>::<part0>,<part1>,<part2>[,<part3>[,<part4>]]::
```

| Part | Name                | Meaning                                                    |
|------|---------------------|------------------------------------------------------------|
| 0    | owner               | GitHub user or organisation                                 |
| 1    | repository          | GitHub repository name                                      |
| 2    | fileStartsWith      | required file-name prefix (`BepInEx`)                       |
| 3    | fileEndsWith        | optional file-name suffix (`.exe`, `.zip`, `_x64`)          |
| 4    | versionFromFile     | `"True"` / `"False"` — read the version from the file name  |

Examples:

```
updgit::BepInEx,BepInEx,BepInEx_x64::
updgit::bbepis,XUnity.AutoTranslator,XUnity.AutoTranslator-BepIn-5x-::
updgit::ModOrganizer2,modorganizer,Mod.Organizer-::          (Mo, suffix set by PrepareUpdateRequest)
```

Rules enforced by `Updater.GetUpdateInfoArray`:

* The `sourceId::` prefix and the trailing `::` are stripped **only when both
  are present** and the id matches the current source case-insensitively.
  A record without the prefix is used as-is.
* At least **3** parts are required. Fewer parts means the record is malformed:
  the folder is skipped with a warning and no download is attempted.
* Separators are newline, `\n`, `\r` and `,`.
* Parts are trimmed by `TrimEachValue()` before use.

---

## 3. Entry points

### 3.1 Full run from the update-options dialog

```csharp
// ManageUpdateMods.UpdateByUpdater()
var updater = new Updater(UpdateOptions);           // UpdateOptionsDialogForm

if (ManageSettings.IsMoMode)
{
    await updater.Update().ConfigureAwait(true);
}

if (updater.UpdatedAny)                             // true when anything changed
{
    ManageModOrganizer.CleanMoFolder();
    ManageModOrganizer.CheckBaseGamesPy();
    ManageModOrganizer.RedefineGameMoData();
}
```

`Update()` with no argument builds the default target list
(`Mo`, `ModsList`, `ModsMeta`) and the default source list (`Github`).

### 3.2 Single-target run

Pass a target list to run a subset. The `UpdateInfo` of the first target is
reused as the shared state, so all targets in the list must share one info.

```csharp
// MainForm.cs — update Mod Organizer only
await new Updater(null)
    .Update(new List<UpdateTargetBase> { new Mo(new UpdateInfo(null)) })
    .ConfigureAwait(true);
```

### 3.3 Ad-hoc single-file check (no `Updater`)

`UpdateSourceBase.GetFile()` can be driven directly when only one file is
needed, for example when a user adds a mod by URL. `UpdateInfo` is filled by
hand and the target is used to unpack the downloaded file.

```csharp
// InfoEditorForFlp.cs
var info = new UpdateInfo
{
    TargetFolderPath = targetDirPathInfo,
    TargetFolderUpdateInfo = new[] { ownerName, repositoryName, startsWith }
};

var ghub = new Github(info);
info.TargetLastVersion = ghub.GetLastVersion();

if (info.TargetLastVersion.Length == 0) return;

UpdateTools.CleanVersion(ref info.TargetLastVersion);
if (!info.TargetLastVersion.IsNewerOf("0", false)) return;

if (!await ghub.GetFile().ConfigureAwait(true)) return;

var target = new ModsMeta(ghub.Info);
if (!target.UpdateFiles()) return;

ghub.Dispose();     // the source owns a WebClient and must be disposed
```

> `Github` is `IDisposable`. Whoever creates a source disposes it — `Updater`
> does this for the sources it creates; ad-hoc callers must do it themselves.

---

## 4. `Updater`

```csharp
namespace AIHelper.Manage.Update
{
    class Updater
    {
        public Updater(UpdateOptionsDialogForm updateOptions = null);
        public UpdateOptionsDialogForm UpdateOptions { get; }

        internal bool UpdatedAny { get; }                 // set once a folder was updated
        internal async Task Update(List<UpdateTargetBase> targetList = null);
    }
}
```

* **`Update(targetList)`** — the only public entry point. Runs on the calling
  (UI) thread, awaits internally, and returns when the pass is finished.
  A progress form is shown for the duration and disposed in a `finally`, so it
  is released even when the pass aborts.
* **`UpdatedAny`** — true when at least one folder was updated. Read it after
  `Update()` returns.
* **`UpdateOptions`** — the options dialog the run was started from; may be
  `null` for a headless run. Passed to `UpdateInfo`.

### Behaviour details worth knowing

* **Check window.** Each (source, folder) pair is checked at most once per
  `UpdatesCheckTimeoutMinutesKeyName` minutes (default 10). The value is
  created in the app ini on first use.
* **Failed folder retry.** The check date is written *before* the download and
  put back (or removed) when the update fails, so a failed folder is retried on
  the next pass instead of being skipped for the timeout window.
* **Cross-source de-duplication.** A folder that was updated is added to
  `info.Excluded`; other sources and targets skip it in the same pass.
* **Report.** One HTML report is written to
  `ManageSettings.UpdateReportHtmlFilePath` and opened in the default browser.
  Each folder produces exactly one line: *updated*, *new version but no file*,
  or *failed*.
* **Never aborts.** A failure of a single folder is logged, added to the
  report, and the loop continues. Sources are disposed and the check dates are
  saved in a `finally`.

---

## 5. `UpdateInfo` — the shared state

```csharp
namespace AIHelper.Manage.Update
{
    internal class UpdateInfo
    {
        public UpdateInfo();
        public UpdateInfo(UpdateOptionsDialogForm updateOptions);

        internal string SourceId { get; set; }              // "updgit"
        internal string SourceLink { get; set; }            // release page, for the report
        internal string TargetCurrentVersion { get; set; }  // installed version
        internal string TargetLastVersion { get; set; }     // version offered by the source
        internal DirectoryInfo TargetFolderPath { get; set; }
        internal string[] TargetFolderUpdateInfo { get; set; }
        internal string UpdateFilePath { get; set; }
        internal string UpdateFileStartsWith { get; set; }
        internal string UpdateFileEndsWith { get; set; }
        internal string BackupDirPath { get; set; }
        internal bool NoRemoteFile { get; set; }
        internal bool VersionFromFile { get; set; }
        internal UpdateSourceBase Source { get; set; }
        internal UpdateTargetBase Target { get; set; }
        public  string DownloadLink { get; internal set; }
        public  UpdateOptionsDialogForm UpdateOptions { get; }

        internal HashSet<string> Excluded { get; }          // per run, not reset
        internal List<string>    Report   { get; }          // per run, not reset

        internal void   Reset();                            // per folder
        internal void   AppendError(string message);
        internal string TakeAndClearError();                // returns and clears
        internal string LastError { get; }                  // returns without clearing
    }
}
```

### Per-run vs per-folder

`Excluded`, `Report` and `UpdateOptions` belong to the whole run and are
**deliberately not cleared** by `Reset()`. Everything else is per folder and
**must** be cleared by `Reset()` before a folder is processed — `Updater` does
this automatically. If you drive a target by hand (see §3.3) call
`info.Reset()` yourself when you reuse one `UpdateInfo` for several folders.

### Error collection

`AppendError` never overwrites: messages accumulate, one per line, prefixed
with `" >"`. `TakeAndClearError()` returns the whole text surrounded by spaces
and clears it, so the same text is never reported twice; `LastError` returns it
without clearing.

```csharp
info.AppendError(ex.Message);            // collect
...
var text = info.TakeAndClearError();     // report once
```

---

## 6. Implementing a source

Derive from `UpdateSourceBase` and implement the abstract members. The base
class already provides the `WebClientEx`, the freeze watchdog, the download
helpers and `IDisposable`.

```csharp
namespace AIHelper.Manage.Update.Sources
{
    abstract class UpdateSourceBase : IDisposable
    {
        protected UpdateSourceBase(UpdateInfo info);

        internal UpdateInfo Info;               // assigned by Updater before each folder
        internal bool IsNotWorkingNow;          // skip this source for the rest of the session
        protected WebClientEx WebClient;

        internal abstract string Title { get; }         // "Github"
        internal abstract string InfoId { get; }        // "updgit" — the record prefix
        internal virtual  string Url { get; }           // "github.com"
        internal abstract string GetLastVersion();      // -> Info.TargetLastVersion
        internal abstract Task<bool> GetFile();         // -> true when the file is on disk

        internal virtual byte[] DownloadFileFromTheLink(Uri link);  // "" -> Array.Empty<byte>()
        internal virtual bool   CheckIfStopWork(UpdateInfo info);   // true -> skip the source
        internal virtual Task   PauseAsync();                       // rate-limit hook

        protected async Task<bool> DownloadFileTaskAsync(Uri uri, string updateFilePath);

        protected virtual void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e);
        protected virtual void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e);

        protected bool IsCompletedDownload { get; }   // last download finished, not cancelled
        protected bool IsCancelDownload    { get; }   // watchdog cancelled the last download

        public void Dispose();
        protected virtual void Dispose(bool disposing);
    }
}
```

### Contract

* **`GetLastVersion()`** returns the version offered by the source, or an empty
  string when it cannot be resolved. An empty value makes `Updater` skip the
  folder without downloading.
* **`GetFile()`** returns `true` only when a usable, non-empty file is on disk.
  It must **not throw**: catch, log, call `Info.AppendError(...)` and return
  `false`. `Updater` additionally guards the call, so a throw is survivable but
  produces a worse report.
* **`CheckIfStopWork(info)`** returns `true` to abort this source for the rest
  of the session — use it for hard limits such as a GitHub rate limit. Set
  `IsNotWorkingNow = true` as well (the base class exposes the field for this).
* **`PauseAsync()`** is the awaitable rate-limit hook. Do not use
  `Thread.Sleep`: the pass runs on the UI thread.
* **`DownloadFileTaskAsync(uri, path)`** is the standard download helper. It
  starts the freeze watchdog, raises the timeout to
  `WebClientEx.DownloadRequestTimeoutMilliseconds`, restores the previous
  timeout, and returns `false` on any failure.

### Freeze watchdog

A download that reports no progress for 10 seconds is cancelled. The watchdog
is armed per download (`StartDownloadWatchdog`), restarted on every progress
event, and released on completion or in `Dispose`. The flags
`IsCompletedDownload` / `IsCancelDownload` are cleared at the start of every
download, so they always describe the **current** download.

---

## 7. Implementing a target

Derive from `UpdateTargetBase` (or `ModsBase` for a mod folder) and implement
the abstract members. The base class provides the backup/restore machinery.

```csharp
namespace AIHelper.Manage.Update.Targets
{
    abstract class UpdateTargetBase
    {
        protected UpdateTargetBase(UpdateInfo info);

        internal const string MetaIniFileName     = "meta.ini";
        internal const string MetaIniVersionSection = "General";
        internal const string MetaIniVersionKey     = "version";

        internal UpdateInfo Info;
        internal string BackupDirPath;
        protected readonly HashSet<string> RestoreList;

        // ---- abstract ----
        internal abstract Dictionary<string, string> GetUpdateInfos();  // folderKey -> updateRecord
        internal abstract bool UpdateFiles();                           // apply the downloaded file
        internal abstract void SetCurrentVersion();                     // -> Info.TargetCurrentVersion

        // ---- virtual hooks ----
        internal virtual void   PrepareUpdateRequest()  { }             // constrain the source request
        internal virtual string GetParentFolderPath()   { return ""; }  // base dir for relative keys
        internal virtual bool   MakeBackup();                           // true when a usable backup exists
        internal virtual void   RestoreBackup();                        // undo a failed update
        internal virtual string[] RestorePathsList()      { return new string[0]; }
        internal virtual string[] RestorePathsListExtra() { return new string[0]; }

        // ---- helpers for derived classes ----
        protected bool PerformUpdate();                                 // zip/dll/7z/rar + meta.ini
        protected void RestoreSomeFiles(string oldModBackupDirPath, string updatingModDirPath);
    }
}
```

### Contract

* **`GetUpdateInfos()`** returns `folderKey -> updateRecord`. The key is a plain
  folder name for mod targets and an absolute path for `Mo`; `Updater` decides
  with `Path.IsPathRooted`. Return `null` or an empty dictionary when the
  current source cannot handle this target (as `Mo` does for non-GitHub
  sources).
* **`SetCurrentVersion()`** reads the installed version into
  `Info.TargetCurrentVersion`. Set it to `string.Empty` when no version can be
  determined — an empty installed version is treated as "always older", so the
  folder is updated once and then stamped.
* **`PrepareUpdateRequest()`** is the supported way to constrain the file the
  source looks for. Called after `Reset()` and `SetCurrentVersion()` and before
  `GetLastVersion()`. `Mo` uses it to set `Info.UpdateFileEndsWith = ".exe"`.
  Do not do this as a side effect of another method.
* **`UpdateFiles()`** returns `true` only when the folder really was updated.
  `ModsBase` delegates to the provided `PerformUpdate()`.

### The provided `PerformUpdate()`

Handles the four supported extensions and returns `false` for anything else,
so an unhandled file is never reported as success:

| Extension | Action                                                     |
|-----------|------------------------------------------------------------|
| `.ZIP`    | `ZipFile.OpenRead(...).ExtractToDirectory(...)`            |
| `.DLL`    | moves the dll into the folder it lived in before (via `GetDllTargetDir`), else `BepInEx/plugins` |
| `.7Z`     | `ManageArchive.Extract7zipTo(...)`                         |
| `.RAR`    | `ManageArchive.ExtractRarTo(...)`                          |
| other     | logs a warning, returns `false`                            |

On success it restores the preserved files and writes the new version into
`meta.ini`. On failure it removes a folder it created itself and returns
`false`.

### Backup / restore

* **`MakeBackup()`** moves the folder content into
  `ManageSettings.UpdatedModsOlderVersionsBuckupDirPath`, copies instead of
  moving the files that must survive (`RestorePathsList` + valid symlink
  targets from `RestorePathsListExtra`), and returns `false` when no usable
  backup could be produced. A folder with content whose backup is empty is
  treated as a failure, so a mod is never overwritten without a way back.
* **`RestoreBackup()`** moves the backup content back over the folder,
  replacing whatever the failed update left behind.
* **`RestorePathsList()`** — paths relative to the folder that must survive an
  update, e.g. `ModsBase` returns `{ "meta.ini" }`, `Mo` returns its ini and
  explorer++ files.
* **`RestorePathsListExtra()`** — absolute symlink paths that may point into the
  updated folder, e.g. the game's `DirLinkPaths`.

---

## 8. Built-in implementations

| Type                | Base                | Folder key      | `UpdateFiles()`                              |
|---------------------|---------------------|-----------------|----------------------------------------------|
| `Sources.Github`    | `UpdateSourceBase`  | —               | —                                            |
| `Targets.Mo`        | `UpdateTargetBase`  | absolute path   | runs the MO installer silently, `WaitForExit` + `ExitCode == 0` |
| `Targets.Mods.ModsList` | `ModsBase`      | folder name     | `PerformUpdate()`                            |
| `Targets.Mods.ModsMeta` | `ModsBase`      | mod path        | `PerformUpdate()`                            |

### `Mo`

* Only enabled for GitHub sources (`GetUpdateInfos` returns `null` otherwise).
* `PrepareUpdateRequest()` sets `Info.UpdateFileEndsWith = ".exe"`.
* `SetCurrentVersion()` reads `ModOrganizer.exe`'s `ProductVersion`, falling
  back to `FileVersion`.
* The installer is killed after 10 minutes. The preserved files are restored
  **only after** a zero exit code; on any failure the downloaded installer is
  deleted (it may be corrupted) and the existing installation is left untouched.

### `ModsList`

Reads the per-game `updateinfo.txt` through
`ManageUpdateMods.GetUpdateInfos(sourceId)`. The "enabled mods only" flag comes
from the static `ManageUpdateMods.UpdateOptions` field — see the note in the
class XML doc.

### `ModsMeta`

Reads the update record from each mod's `meta.ini`, first from the AIHelper
section, then from Mod Organizer's `General/notes`. The record is normalised
(`, ` → `,`) and the source id is `Regex.Escape`d before it is used as a
pattern.

---

## 9. `UpdateIo` — safe file-system helpers

Every method is **guaranteed not to throw**: the caller decides what a failure
means. Path comparisons are case-insensitive and separator-aware.

```csharp
namespace AIHelper.Manage.Update
{
    internal static class UpdateIo
    {
        internal static bool   TryDeleteDirectory(string path, bool recursive = false);
        internal static bool   TryDeleteFile(string path);
        internal static long   GetFileLengthOrZero(string path);
        internal static bool   IsUnderPath(string path, string rootPath);
        internal static string GetRelativeSubPath(string path, string rootPath);
        internal static string RemapPath(string path, string sourceRoot, string targetRoot);
        internal static bool   EnsureParentDirectory(string path);
    }
}
```

```csharp
// move a file out of the 2mo directory, keeping its sub-tree
if (UpdateIo.IsUnderPath(Info.UpdateFilePath, ManageSettings.Install2MoDirPath))
{
    var newPath = UpdateIo.RemapPath(
        Info.UpdateFilePath,
        ManageSettings.Install2MoDirPath,
        ManageSettings.ModsUpdateDirDownloadsPath);

    UpdateIo.EnsureParentDirectory(newPath);
    UpdateIo.TryDeleteFile(newPath);
    File.Move(Info.UpdateFilePath, newPath);
}
```

Use these instead of `new FileInfo(path).Length` (throws when the file
disappears), `Directory.Delete` inside a `catch` (throws from the handler) and
`string.Replace` path surgery (silently produces wrong paths).

---

## 10. `UpdateTools` — version helpers

```csharp
namespace AIHelper.Manage.Update
{
    internal static class UpdateTools
    {
        // "v13" -> "13", "1.0.0.0" -> "1", "1.0BETA" -> "1.0B"
        internal static string CleanVersion(string version);

        // in-place overload, kept for the existing call sites
        public static void CleanVersion(ref string version);

        // extension method on string
        public static bool IsNewerOf(this string candidateVersion, string installedVersion, bool needClean = true);
    }
}
```

```csharp
UpdateTools.CleanVersion(ref info.TargetLastVersion);
UpdateTools.CleanVersion(ref info.TargetCurrentVersion);

if (!info.TargetLastVersion.IsNewerOf(info.TargetCurrentVersion, false)) return;
```

* Both values are first converted from the Mod Organizer date format by
  `ManageModOrganizer.ConvertMODateVersion`, then cleaned unless
  `needClean == false` — pass `false` when both values are already clean, as
  `Updater` does.
* Each `.`/`,`-separated part is compared as a number first (so `10` > `2`) and
  as text otherwise.
* An empty candidate is never newer; an empty installed version is always
  older.

---

## 11. `WebClientEx` and `UserAgents`

```csharp
namespace AIHelper.Manage.Update.Sources
{
    internal sealed class WebClientEx : WebClient
    {
        internal const int DefaultRequestTimeoutMilliseconds    = 5000;
        internal const int DownloadRequestTimeoutMilliseconds   = 5 * 60 * 1000;

        internal WebClientEx(CookieContainer container);        // null -> a new container
        internal CookieContainer CookieContainer { get; set; }  // never null
        internal int RequestTimeoutMilliseconds { get; set; }   // defaults to 5000
    }
}
```

* **Caching is disabled** (`RequestCacheLevel.NoCacheNoStore`): answering the
  "latest release" page from the WinINet cache would silently hide new
  versions.
* **The timeout is adjustable** because a release page and a 300 MB archive
  need very different budgets. `DownloadFileTaskAsync` raises it to
  `DownloadRequestTimeoutMilliseconds` and restores the previous value.
* `UserAgents` is a `static class` of `const string` values
  (`ChromeIronWin7` is the one actually sent). Update the values if a request
  starts being rejected.

---

## 12. Lifecycle and threading

1. **One `UpdateInfo` per run**, one **source instance** and one **target
   instance** per run. The owner assigns `Info.Source`, `Info.Target`,
   `Info.SourceId` and `Info.TargetFolderPath` before each folder — a target
   must not cache them.
2. **Sources are disposable.** Dispose them when the run is over. `Updater`
   does it in a `finally`; ad-hoc callers must do it themselves.
3. **Everything runs on the UI thread.** `Update()` awaits internally and
   marshals back with `ConfigureAwait(true)`. Never block with `Thread.Sleep`;
   use `PauseAsync()`.
4. **WinForms controls are only touched on the UI thread.** The download
   progress form is updated through `BeginInvoke` when `InvokeRequired`.
5. **The progress form and the report are always released** — both are handled
   in `finally`/`using`.

---

## 13. Migration notes (what changed)

If you have code that touches the updater, these are the renames and signature
changes introduced by the refactoring:

| Before                                  | After                                              |
|-----------------------------------------|----------------------------------------------------|
| `MakeBuckup()`                          | `MakeBackup()`                                     |
| `RestoreBuckup()`                       | `RestoreBackup()`                                  |
| `RestoreModFromBuckup(...)`             | `RestoreModFromBackup(...)`                        |
| `Info.BuckupDirPath`                    | `Info.BackupDirPath`                               |
| `Info.LastErrorText` (public field)     | `Info.AppendError(...)` / `TakeAndClearError()` / `LastError` |
| `UpdateFilenameSubPathData`             | removed (dead)                                     |
| `source.Pause()`                        | `await source.PauseAsync()`                        |
| `UpdateSourceBase.OnlineTranslatorCookies` | removed                                         |
| `new UpdateInfo()` with no `Reset()`    | the constructor calls `Reset()`                    |
| `UserAgents.chromeWin10` (etc.)         | `UserAgents.ChromeWin10Latest` (PascalCase `const`) |
| `UpdateTargetBase.UpdateFiles()` without `PrepareUpdateRequest()` | override `PrepareUpdateRequest()` to constrain the request |

Unchanged on purpose (external call sites keep compiling):

* `Updater.Update(...)`, `Updater.UpdatedAny`, `Updater.UpdateOptions`
* `UpdateInfo.TargetFolderPath`, `UpdateInfo.TargetFolderUpdateInfo` object
  initializers
* `Mo(UpdateInfo)`, `ModsList(UpdateInfo)`, `ModsMeta(UpdateInfo)`
* `UpdateTools.CleanVersion(ref string)`, `UpdateTools.IsNewerOf(...)`
* `ManageUpdateMods.GetUpdateInfos(string sourceId)`

---

## 14. Minimal end-to-end example

```csharp
// update Mod Organizer and every mod, headless, from a background handler
internal static async Task RunUpdateAsync()
{
    var updater = new Updater(null);

    try
    {
        await updater.Update().ConfigureAwait(true);   // default sources + targets
    }
    catch (Exception ex)
    {
        LogManager.GetCurrentClassLogger().Error(ex, "Update pass failed.");
        return;
    }

    if (!updater.UpdatedAny) return;

    // data that must be rebuilt after something changed
    ManageModOrganizer.CleanMoFolder();
    ManageModOrganizer.CheckBaseGamesPy();
    ManageModOrganizer.RedefineGameMoData();
}
```
