# todo.md — Code review of the updater function files

Scope: every source file under `AI Helper/Manage/Update/` (21 files, incl. the `Sort/` subtree).

| # | File |
|---|------|
| 1 | `UpdateTools.cs` |
| 2 | `UpdateInfo.cs` |
| 3 | `Updater.cs` |
| 4 | `Sources/UpdateSourceBase.cs` |
| 5 | `Sources/Github.cs` |
| 6 | `Sources/WebClientEx.cs` |
| 7 | `Sources/UserAgents.cs` |
| 8 | `Targets/UpdateTargetBase.cs` |
| 9 | `Targets/MO.cs` |
| 10 | `Targets/MOBaseGames.cs` |
| 11 | `Targets/Mods/ModsBase.cs` |
| 12 | `Targets/Mods/ModsList.cs` |
| 13 | `Targets/Mods/ModsMeta.cs` |
| 14 | `Targets/Mods/ModsMetaUrl/ModsMetaUrlBase.cs` |
| 15 | `Targets/Mods/ModsMetaUrl/XUA.cs` |
| 16 | `Sort/SideloaderPackBase.cs` |
| 17 | `Sort/SideloaderModPack.cs` |
| 18 | `Sort/SideloaderModPackCommunityUserdata.cs` |
| 19 | `Sort/UncensorSelectorBase.cs` |
| 20 | `Sort/UncensorSelectorFemale.cs` |
| 21 | `Sort/UncensorSelectorMale.cs` |

Target framework: **.NET Framework 4.7.2**, language level: **C# 7.3**
(`AI Helper.csproj`, `ToolsVersion 15.0`). No C# 8+ constructs are allowed
(no nullable reference types, no `using` declarations, no switch expressions,
no `??=`, no ranges/indices, no static local functions, no target-typed `new`).

---

## Severity legend

| Tag | Meaning |
|-----|---------|
| **P0** | Real bug that produces a wrong result, a crash, a resource leak or data loss |
| **P1** | Correctness/robustness risk, hidden coupling, inconsistent behaviour |
| **P2** | Refactoring / readability / maintainability / best-practice violation |
| **DEAD** | Dead, commented-out or unreachable code |

---

## 1. `UpdateTools.cs`

### UT-1 — **P0** — `string.Compare` result compared to `== 1`
```csharp
var compareResult = string.Compare(versionString1Parts[i], versionString2Parts[i]);
if (compareResult == 1)            // line 82
```
`string.Compare` returns `< 0`, `0` or `> 0` — **not** exactly `-1`/`0`/`1`
(for example comparing `"2"` with `"0"` returns `2`). The `== 1` test therefore
silently returns `false` for most positive results, so a genuinely newer
alphanumeric version (e.g. `1.2b` vs `1.2a`) is reported as *not newer* and the
update is skipped. Must be `> 0` / `< 0`.

### UT-2 — **P1** — `CleanVersion(ref string)` uses `ref` for a pure function
```csharp
public static void CleanVersion(ref string version)   // line 9
```
The method only maps one string to another; `ref` makes call sites noisy and
hides the data flow. Call sites: `Updater.cs:200-201`, `Github.cs:583-584`,
`Github.cs:675-679`, `InfoEditorForFlp.cs:387`.

### UT-3 — **P1** — `IsNewerOf` performs hidden side effects and double cleaning
`IsNewerOf` calls `ManageModOrganizer.ConvertMODateVersion` (line 51-52) and
`CleanVersion` (line 57-58) although the main caller `Updater.cs:200-201`
already cleaned both versions immediately before the call — the work is done
twice and the method is not a pure predicate.

### UT-4 — **P1** — `public static class` for an assembly-internal helper
`UpdateTools` is `public` while every other type in the namespace is
`internal`/implicitly internal. It unnecessarily widens the assembly surface.

### UT-5 — **P2** — magic replacement pairs
Lines 16-20 use five hard-coded `.Replace("ALPHA", "A")`… chains. Should be a
single `static readonly (string From, string To)[]` table so the suffix
vocabulary is data, not control flow.

### UT-6 — **P2** — missing/insufficient documentation
`IsNewerOf(versionNew, versionOld, needClean)` has an XML doc that does not
describe `needClean`; the parameter names `versionNew`/`versionOld` do not
convey "candidate" vs "installed".

### UT-7 — **DEAD** — commented-out prefix-stripping block, lines 24-31.

---

## 2. `UpdateInfo.cs`

### UI-1 — **P0** — the parameterised constructor skips `Reset()`
```csharp
public UpdateInfo() { Reset(); }                                  // line 61
public UpdateInfo(UpdateOptionsDialogForm updateOptions = null)    // line 66
{
    UpdateOptions = updateOptions;      // Reset() is NOT called
}
```
`UpdateInfo(null)` (used by `MainForm.cs:186` and `ModsMeta`) leaves
`TargetCurrentVersion`, `SourceLink`, `TargetLastVersion`, `UpdateFilePath`, …
as **`null`**. `Reset()` is the only thing that normalises them to `""`.
Any read before the first `info.Reset()` in `Updater` (or any new caller)
throws `NullReferenceException` — e.g. `UpdateTools.CleanVersion` dereferences
`version.Length` (`UpdateTools.cs:11`).

### UI-2 — **P0** — `Reset()` does not reset `BuckupDirPath`
`BuckupDirPath` is documented as a per-folder value (`/// buckup dir path for
old versions of mods`) but `Reset()` (lines 74-87) never clears it, while
`UpdateTargetBase.MakeBuckup` reads it through `Info.BuckupDirPath`. A folder
processed after a failure can therefore reuse a stale backup path.

### UI-3 — **P1** — `UpdateInfo` mixes public auto-properties with bare internal fields
`SourceId`, `TargetCurrentVersion`, `SourceLink`, `TargetLastVersion`,
`TargetFolderPath`, `TargetFolderUpdateInfo`, `NoRemoteFile`, `UpdateFilePath`,
`UpdateFileStartsWith`, `UpdateFileEndsWith`, `BuckupDirPath`, `Source`,
`Target`, `VersionFromFile` are public mutable fields, while `Excluded`,
`DownloadLink`, `LastErrorText`, `UpdateOptions` are properties. Every consumer
can mutate any field at any time, which is why `Updater` has to keep assigning
`info.Source`/`info.Target` inside nested loops.

### UI-4 — **P1** — `LastErrorText` leaks a mutable `StringBuilder`
```csharp
public StringBuilder LastErrorText { get; } = new StringBuilder();
```
Callers (`Updater.ErrorMessage`) mutate and `Clear()` it as a side effect.
A `TakeAndClearError()` method (or `string LastError` + `ClearError()`) would
make the ownership explicit.

### UI-5 — **P2** — inconsistent "empty" representation
`Reset()` sets most string fields to `""` but `TargetFolderPath` and
`TargetFolderUpdateInfo` to `null`. Consumers must handle both.

### UI-6 — **P2** — two constructors with an optional parameter
`UpdateInfo(UpdateOptionsDialogForm updateOptions = null)` makes
`new UpdateInfo()` ambiguous-looking next to the real parameterless ctor.
One ctor chaining to the other is enough.

### UI-7 — **DEAD** — commented-out members, lines 49-50 (`GetVersionFromLink`, `UrlSizeList`).

---

## 3. `Updater.cs`

### UP-1 — **P0** — `SaveCheckDateTimes` creates the file before the directory
```csharp
using (StreamWriter sw = new StreamWriter(ManageSettings.UpdateCheckDateTimesFilePath))  // line 311
{
    Directory.CreateDirectory(Path.GetDirectoryName(...));                               // line 313
```
The `StreamWriter` is constructed **first**, so on a fresh install (no
`Res\update\` directory) it throws `DirectoryNotFoundException` and the check
dates are never persisted — every start re-queries GitHub for every mod.
`Path.GetDirectoryName` can also return `null`, which makes
`Directory.CreateDirectory` throw `ArgumentNullException`.

### UP-2 — **P0** — `GetDateTimesFromFile` is not exception-safe
`StreamReader sr = new StreamReader(...)` (line 335) with a manual
`sr.Dispose()` at line 354: any exception inside the `while` loop leaks the
file handle and leaves the cache file locked. Must be `using`.

### UP-3 — **P0** — unguarded `DateTime.ParseExact` on external data
```csharp
ret[data[0]].Add(data[1], DateTime.ParseExact(data[2], "O", CultureInfo.InvariantCulture));  // line 351
```
A truncated/edited `updatecheckdatetimes.txt` throws `FormatException`, which
escapes `Update()` and aborts the whole update run. Needs `TryParseExact` +
skip (and ideally truncate/repair the file).

### UP-4 — **P0** — `oldCheckDateTime` is assigned the *new* value
```csharp
var oldCheckDateTime = DateTime.MinValue;                        // line 186
if (sourcePathDateCheckTime.ContainsKey(tFolderInfo.Key))
{
    oldCheckDateTime = checkDateTimeNow;                         // line 189  ← bug
    sourcePathDateCheckTime[tFolderInfo.Key] = checkDateTimeNow; // line 190
}
…
sourcePathDateCheckTime[tFolderInfo.Key] = oldCheckDateTime;     // line 254  ← restores the NEW value
```
The "restore old check date after a failed update" branch (lines 248-255) can
never restore the real previous timestamp; it either removes the key or writes
back `checkDateTimeNow`. Failed mods are therefore skipped for the whole
`updateCheckTimeout` window instead of being retried.

### UP-5 — **P0** — `source.GetFile()` is awaited without a guard
```csharp
bool getfileIsTrue = await source.GetFile().ConfigureAwait(true);   // line 208
```
`Github.GetFile()` re-throws every exception (`Github.cs:45`), so one network
error aborts the entire run (no report, no `SaveCheckDateTimes`, the progress
form is never disposed). Needs a per-target `try/catch` that records the error
in `info.LastErrorText` and continues.

### UP-6 — **P0** — `tInfoArray.Length == 0` never detects a malformed record
```csharp
if (tInfoArray.Length == 0) continue;   // line 172
```
`string.Split` never returns a zero-length array for a non-null input; a
malformed record yields `[""]` or a 1-2 element array and is then passed on to
`Github.InitializeGitHubRepositoryInfo`, which throws
`InvalidOperationException("Invalid GitHub repository configuration")`
(`Github.cs:420`) — again aborting the run. The guard must validate the
expected element count (≥ 3).

### UP-7 — **P0** — progress `Form`/`ProgressBar` are not protected by `try/finally`
`progressForm` is created at line 90 and disposed at line 294-295. Any
exception between them (see UP-5, UP-6) leaks the window and leaves a modal
form on screen. Also `Application.DoEvents()` is never called, so the progress
window may not paint at all while the synchronous parts of the loop run, and
`pBar.Dispose()` before `progressForm.Dispose()` disposes the child twice.

### UP-8 — **P1** — `_isHtmlReport` is always `true` → dead branches
Field declared at line 20 (`= true`), never written except
`_isHtmlReport = true` at line 408. Every `_isHtmlReport ? … : string.Empty`
ternary is dead code, including the whole non-HTML branch of `ShowReport`
(lines 416-423) and the `else` at line 397.

### UP-9 — **P1** — `sources` is a one-element `List<>` that is never disposed
```csharp
var sources = new List<UpdateSourceBase> { new Github(info) };   // line 43-46
```
`UpdateSourceBase` implements `IDisposable` (owns a `WebClient`) but
`Updater` never disposes it and does not implement `IDisposable` itself →
the `WebClient` (and its `Timer`) survives until GC. The list + `foreach` is
pure indirection for a single element.

### UP-10 — **P1** — 180-line nested loop with inlined report strings
`Update()` (lines 113-292) contains 5 nesting levels and three ~20-line inline
HTML concatenations (lines 220-240, 260-271, 278-285) that each repeat
`info.TargetFolderPath.Name`, the version tags and `info.SourceLink`
handling. This must be decomposed (see `technical_task.md`).

### UP-11 — **P1** — `Info.Excluded` / `Info.Source` / `Info.Target` used as global mutable state
`info.Source = source` (line 117), `info.Target = target` (line 136) and
`info.Excluded.Add(...)` (line 218) turn `UpdateInfo` into ambient state that
is read by targets/sources far away from the loop, which is what forces the
fragile ordering documented in §9 (MO-7).

### UP-12 — **P1** — `Path.Combine` with a possibly absolute key
```csharp
info.TargetFolderPath = new DirectoryInfo(Path.Combine(target.GetParentFolderPath(), tFolderInfo.Key));  // line 159
```
`Mo.GetUpdateInfos()` returns an **absolute** directory as the dictionary key
while `Mo.GetParentFolderPath()` returns `string.Empty`, so this only works by
accident of `Path.Combine("", absolute)`. Should branch on
`Path.IsPathRooted`.

### UP-13 — **P1** — `string.IsNullOrWhiteSpace` not used
`if (info.TargetLastVersion.Length == 0) continue;` (line 197) and
`version.Length == 0` in `UpdateTools` will throw if the value is `null`
(which is reachable through UI-1).

### UP-14 — **P2** — naming
`lastCheckDates` (line 30) is the only field without the `_` prefix;
`DateTimesFilePath` (line 333) is a PascalCase local.

### UP-15 — **P2** — magic numbers / inline ini plumbing
`int updateCheckTimeout = 10;` (line 59) is a magic default; the whole
"read-or-create the ini key" block (lines 58-72) belongs in a private helper
such as `GetUpdateCheckTimeoutMinutes()`.

### UP-16 — **P2** — `ErrorMessage(StringBuilder)` has a hidden side effect
It returns the text **and clears** the builder (lines 359-366). Rename to
`TakeAndClearError` (or return `string` from `UpdateInfo`).

### UP-17 — **P2** — `ShowReport` swallows failures to the console
```csharp
catch (Exception e) { Console.WriteLine(e.Message); }   // line 440-443
```
A WinForms app has no console; the report-open failure is lost. Use the
existing `_log` and also guard `File.ReadAllText` (line 409) and
`Directory.CreateDirectory(Path.GetDirectoryName(htmlfile))` (line 429,
`null` when `htmlfile` is a bare name).

### UP-18 — **P2** — `ShowReport` splits the report line and silently truncates it
```csharp
string[] lines = line.Split(new[] { ManageSettings.UpdateReport.InfoLinkPattern }, StringSplitOptions.None);
if (lines.Length == 2 && _isHtmlReport) { … } else { newReport.Add(lines[0]); }   // line 388-400
```
If a report line happens to contain the pattern twice, `lines.Length != 2` and
everything after the first segment is dropped. Use an explicit
`IndexOf` + `Substring` (or `Split` with a count) so the tail is preserved.

### UP-19 — **P2** — `UpdatedAny` is a bare internal field
`internal bool UpdatedAny;` (line 25) is read by
`ManageUpdateMods.UpdateByUpdater()` — make it a get-only property with a
private setter.

### UP-20 — **DEAD** — commented-out code
lines 49-50 (`new MOBaseGames(info)`), 76-88 (old progress form),
300 (`SaveSizes`), 368-377 (`SaveSizes` method), 424-460 (old report paths).

---

## 4. `Sources/UpdateSourceBase.cs`

### USB-1 — **P0** — `IsCancelDownload` is never reset
```csharp
private void CancelDownload(object sender, ElapsedEventArgs e)
{
    IsCancelDownload = true;      // line 64 — set once, never cleared
    …
}
protected virtual void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
{
    if (!IsCancelDownload) IsCompletedDownload = true;   // line 111
```
The first 10-second freeze permanently poisons the source: every later
download leaves `IsCompletedDownload == false`, so
`Github.DownloadTheFile` reports failure for the rest of the session.

### USB-2 — **P0** — `IsCompletedDownload` is never reset either
`IsCompletedDownload` (line 108) is only ever set to `true` (line 112) and is
read at `Github.cs:153`. Once one download succeeds, a subsequent failed or
cancelled download is still reported as completed as long as the file exists
and is non-empty — a partially written file can be accepted as valid.

### USB-3 — **P0** — `Timer` is created per download, dereferenced without a null check, and never stopped on cancel
```csharp
private void RestartTimer() { Timer.Stop(); Timer.Start(); }   // line 101-106
private void ReleaseTimer() { Timer.Stop(); Timer.Dispose(); } // line 116-121
```
Three distinct defects:
1. `Timer` is only created inside `InitDownloadTimeTimer()` (line 51), but
   `RestartTimer()` is called from `DownloadProgressChanged` (line 98), which is
   subscribed in the constructor (line 91). Any progress event outside a
   `DownloadFileTaskAsync` window throws `NullReferenceException`.
2. `ReleaseTimer()` disposes the timer without nulling it, so a later
   `DownloadProgressChanged` touches a disposed object
   (`ObjectDisposedException` from `Timer.Enabled`).
3. `CancelDownload` sets `IsCancelDownload` and calls `CancelAsync` but never
   stops the timer, so a genuinely stuck download appends
   "Download was freezed by some reason…" to `LastErrorText` **every 10
   seconds** until the completion event arrives.

### USB-4 — **P0** — `Timer` is never disposed by `Dispose()`
```csharp
public void Dispose() { if (WebClient != null) WebClient.Dispose(); }   // line 161-167
```
A pending `System.Timers.Timer` keeps the object rooted and fires
`CancelDownload` after the source is gone.

### USB-5 — **P1** — `Dispose()` does not follow the standard pattern
No `Dispose(bool disposing)`, no `GC.SuppressFinalize`, no idempotency guard.
`IDisposable` is implemented on an `abstract` class that also has a
parameterless ctor (line 75-77) which leaves `Info`/`WebClient` null.

### USB-6 — **P1** — `DownloadFileTaskAsync` catches only `WebException`
Lines 41-45. `UriFormatException`, `IOException`, `InvalidOperationException`
(WebClient busy), `NotSupportedException` all escape to `Github.GetFile`, which
re-throws (see GH-1). The method also returns `Task` rather than
`Task<bool>`, so success has to be inferred from the shared
`IsCompletedDownload` flag (see USB-2).

### USB-7 — **P1** — dead `OnlineTranslatorCookies` feature
```csharp
public CookieContainer OnlineTranslatorCookies { get; private set; }   // line 138
… new WebClientEx(OnlineTranslatorCookies ?? new CookieContainer())    // line 83
```
The property is never assigned, so `??` always wins; the "online translator"
cookies concept is dead.

### USB-8 — **P2** — `protected static Logger _log` is not `readonly`
Line 16 (`protected static Logger _log = …`) while every other class in the
project uses `static readonly`.

### USB-9 — **P2** — `DownloadFileFromTheLink` returns `null` by default
Line 150-153. A `virtual` method that returns `null` byte[] forces every caller
to null-check; prefer `abstract` or return an empty array + document.

### USB-10 — **P2** — `IsNotWorkingNow = false` redundant initializer, and
`Info`/`WebClient` are protected/internal mutable fields with no invariants.

---

## 5. `Sources/Github.cs`

### GH-1 — **P0** — `GetFile()` re-throws and kills the whole update run
```csharp
catch (Exception ex) { _log.Error(ex, …); throw; }   // line 42-46
```
Combined with UP-5 (no guard in `Updater`) this turns a transient DNS/TLS
error into an aborted update with a leaked progress window.

### GH-2 — **P0** — static WinForms controls updated from WebClient callbacks
```csharp
private static Form _dwnf;          // line 86
private static ProgressBar _dwnpb;  // line 87
protected override void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
{
    if (e.ProgressPercentage <= _dwnpb.Maximum) _dwnpb.Value = e.ProgressPercentage;   // line 55-58
}
```
`WebClient.DownloadFileTaskAsync` raises `DownloadProgressChanged` on a
**thread-pool** thread, so writing `_dwnpb.Value` without `Invoke`/`BeginInvoke`
is a cross-thread UI access (WinForms throws
`InvalidOperationException: Cross-thread operation not valid`).
Being `static` also means two `Github` instances share (and dispose) the same
controls.

### GH-3 — **P0** — progress UI is leaked when the download throws
```csharp
// Note: original behavior does NOT dispose the progress UI here (kept as-is).
return completed;   // line 166-167
```
On the normal path `DownloadFileCompleted` (line 63-68) already calls
`DisposeDownloadUi()`, so the happy path is clean. But if
`DownloadFileTaskAsync` (line 150) throws anything that is **not** a
`WebException` — or if it throws before the completion event fires — the
`await` unwinds straight out of `DownloadTheFile`, `CreateAndShowDownloadUi`
(line 129) is never paired with `DisposeDownloadUi`, and the topmost window
stays on screen forever (`_dwnf`/`_dwnpb` stay non-null, which then makes the
"Download UI is already active" guard at line 92 reject every later download
for the rest of the session). Needs a `try/finally`.

### GH-4 — **P0** — unescaped regex metacharacters in the file-name patterns
```csharp
var linkPattern = BuildFileLinkPattern(_gitOwner, _gitRepository, _gitLatestVersion, false);  // line 521
… Info.UpdateFileStartsWith + @"([^\""]*)" + Info.UpdateFileEndsWith …                          // line 552-554
```
`Info.UpdateFileStartsWith` / `UpdateFileEndsWith` come straight from user data
(`updateinfo.txt` / `meta.ini`). Prefixes such as `XUnity.AutoTranslator-BepIn-5x-`
happen to be safe, but `[Female]`, `Mod.Organizer-`, `(` `)` `+` `?` `.` `|`
are not — a bracket class silently matches the wrong characters and a lone `[`
makes `Regex.Match` throw. Every interpolated value must go through
`Regex.Escape`. Same problem in `SearchForFileLinkWithFlexibleOwner`
(line 535-537), `SearchInLast10Releases` (line 632-634, 649-651) and
`ExtractVersionFromFileName` (line 741-742).

### GH-5 — **P1** — no explicit guard for an empty resolved file name
```csharp
var updateFileName = !string.IsNullOrWhiteSpace(Info.UpdateFilePath) ? Path.GetFileName(Info.UpdateFilePath) : Path.GetFileName(Info.DownloadLink);  // line 104-106
if (string.IsNullOrWhiteSpace(updateFileName)) { updateFileName = SearchUpdateFilePath(…); }
```
Traced through: when both sources are empty `updateFileName` stays `""`,
`SearchUpdateFilePath` returns `""` without setting `Info.UpdateFilePath`,
`ShouldDownload("", "", …)` sees "file does not exist" and returns `true`, and
the run is then saved only by the separate `Info.DownloadLink` empty check at
line 140-146. So today the bad path is not reached, but the safety depends on a
check 40 lines further down and on `Info.UpdateFilePath` still being `""`.
Add an explicit `if (string.IsNullOrWhiteSpace(updateFileName)) { … return false; }`
so `Info.UpdateFilePath` can never be a directory.

### GH-6 — **P1** — `new FileInfo(...)` on a file that may have vanished
`new FileInfo(Info.UpdateFilePath).Length` (line 174) in the
`shouldDownload == false` branch, and `ShouldDownload` line 252 — both are
reached only after a `File.Exists` check, so the window is narrow, but a
concurrent KKManager/AV cleanup between the two checks throws
`FileNotFoundException` out of `GetFile`. Use a
`TryGetFileLength(path, out long length)` helper that treats "missing" as
length 0.

### GH-7 — **P0** — unreachable "current version is newer" early-out
```csharp
if (!updateFileMatch.Success && Info.VersionFromFile)      // line 378
{
    var searchResult = SearchInLast10Releases();
    …
    if (updateFileMatch.Success && !Info.VersionFromFile)  // line 386 — always false
    {
        if (IsCurrentVersionNewerThanRelease(updateFileMatch)) { Info.DownloadLink = ""; return _gitLatestVersion; }
    }
}
```
The inner condition contradicts the enclosing one, so
`IsCurrentVersionNewerThanRelease` (line 673) is dead in this path and a
downgrade can be performed when the installed version is already newer than
the last 10 releases.

### GH-8 — **P0** — `SearchInLast10Releases` overwrites `Info.SourceLink`
```csharp
Info.SourceLink = $"{GitHubBaseUrl}/{_gitOwner}/{_gitRepository}/releases";   // line 619
```
`Info.SourceLink` is used for the report link (`Updater.cs:239`) and in
`LogFileLinkNotFound` (line 774). After this call the report points at the
releases list instead of the release, and the next `Updater` iteration (and the
next `Github.GetLastVersion` call for another folder — `InitializeGitHubRepositoryInfo`
resets it, but the report is already built) shows the wrong link.

### GH-9 — **P0** — `SearchInLast10Releases` does not limit itself to 10 releases
```csharp
var assetsList = Regex.Matches(releasesListPage, AssetsPageRegexPattern);   // line 623
foreach (Match assetsMatch in assetsList) { … WebClient.DownloadString(assetsPageUrl); … }
```
Despite the name it downloads **every** assets page found on
`/releases` (GitHub renders ~10 per page, but the page can contain more, and
the regex also matches other links). Each iteration is a full HTTP request
with no delay, no cap and no rate-limit handling → slow and likely to trip
GitHub's abuse detection.

### GH-10 — **P0** — `ReleasesSearchResult.Found` is always `true`
```csharp
var result = new ReleasesSearchResult { Found = true };   // line 616
```
`Found` is never set to `false`, and the caller uses it as
`searchedInMultipleReleases` (line 383), which then selects the wrong
`$2`/`$3` capture group in `ExtractVersionFromFileName` (lines 753, 759) →
a wrong version string is compared and written to `meta.ini`.

### GH-11 — **P1** — `TryParseAndLoadAssetsPage` mutates the caller's array
```csharp
Info.TargetFolderUpdateInfo[0] = _gitOwner = currentOwnerName;   // line 474
Info.TargetFolderUpdateInfo[1] = _gitRepository = currentRepoName; // line 481
```
A method named "parse and load" also rewrites the update-info record (and
throws `NullReferenceException` if `TargetFolderUpdateInfo` is null).
The owner/repo "changed" detection should be returned to the caller, not
written back as a side effect.

### GH-12 — **P1** — `SearchUpdateFilePath` mixes static `Info` and parameter `info`
```csharp
private string SearchUpdateFilePath(string updateDownloadsDir, UpdateInfo info)
{
    … var candidateUpdateFileName = $"{Info.UpdateFileStartsWith}…";   // line 284 uses the field
    info.UpdateFilePath = Path.Combine(targetDir, candidateUpdateFileName);  // line 290 uses the parameter
```
Both refer to the same object but the reader cannot tell; the
`updateDownloadsDir` parameter is also redundant because the method already
re-derives the directories from `ManageSettings`.

### GH-13 — **P1** — `captureVersion` parameter is dead
`BuildFileLinkPattern(owner, repo, version, bool captureVersion)` is only ever
called with `false` (line 521), so the `captureVersion ? "([^/]+)" : version`
branch (line 551) is unreachable.

### GH-14 — **P1** — version extraction has hidden side effects
`IsCurrentVersionNewerThanRelease` (line 673-690) rewrites `_gitLatestVersion`
and `Info.TargetCurrentVersion` via `CleanVersion`, and
`TryFindUpdateFileLocally` (line 583-584) does the same — both are called from
conditions and leave the object mutated, so the values used afterwards depend
on which branch ran.

### GH-15 — **P1** — `Pause()` uses a fresh `Random` every call and blocks the UI
```csharp
int ms = (int)(new Random().NextDouble() * 500);   // line 794
Thread.Sleep(ms);                                  // line 796
```
`new Random()` inside a loop yields correlated seeds; and `Updater.Update` runs
on the UI thread, so `Thread.Sleep` freezes the window. Use one
`static readonly Random` guarded by a lock (or `Interlocked`-based jitter), and
await `Task.Delay` instead of blocking.

### GH-16 — **P1** — `CheckIfStopWork` is case-sensitive and duplicates `Info`
```csharp
var s = info.LastErrorText.ToString();
var b = s.Contains("(429) too many requests");   // line 784-785
```
GitHub returns `"API rate limit exceeded"` / `"(429)"` with varying casing;
`Contains` without `StringComparison` is culture-sensitive. Also the method
takes `info` while the field `Info` is the same object.

### GH-17 — **P1** — `_dwnpb.Maximum` guard is inverted/meaningless
```csharp
if (e.ProgressPercentage <= _dwnpb.Maximum) { _dwnpb.Value = e.ProgressPercentage; }   // line 55
```
`ProgressPercentage` is 0-100 and `Maximum` is 100, so the guard never fires;
the real risk is `_dwnpb` being null (see GH-2/GH-3) → `NullReferenceException`.

### GH-18 — **P2** — inconsistent array/field declarations
`_updateFilePrefixes` is `static readonly` (line 266) while
`_updateFileSuffixes` is an instance `readonly` (line 271) — both are constant
data and should be `static readonly`.

### GH-19 — **P2** — `ReleasesSearchResult` mutable struct with public fields
Lines 603-608. C# 7.3 supports value tuples and `readonly struct`; either would
express "three results, no identity" more clearly than a mutable struct.

### GH-20 — **P2** — `LogFileLinkNotFound` dereferences a possibly-null path
`Info.TargetFolderPath.Name` (line 773) without a null check.

### GH-21 — **P2** — noisy construction-time logging and swallowed stack traces
`_log.Info("Initialized Github update source for: …")` in the ctor (line 19);
`_log.Warn("Failed to check update. Error:\r\n" + ex)` (line 403) loses the
`Error`/`Debug` overloads NLog provides.

### GH-22 — **P2** — `MinValidLinkLength = 7` is undocumented magic
Line 333. It encodes `"href=".Length` — say so, or use
`HrefPrefix.Length`.

### GH-23 — **P2** — `DownloadFileFromTheLink` has no error handling
Line 800-804 (`WebClient.DownloadData`) — any HTTP error escapes.

---

## 6. `Sources/WebClientEx.cs`

### WC-1 — **P0** — `CacheIfAvailable` can serve stale GitHub pages
```csharp
CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);   // line 13
```
GitHub sends `Cache-Control` headers for release/asset pages, so a
`/releases/latest` response can be answered from the WinINet cache and the
updater will "see" an old version → updates silently missed. `NoCacheNoStore`
(or `BypassCache`) is required for a version checker.

### WC-2 — **P0** — a null `CookieContainer` is accepted
```csharp
public WebClientEx(CookieContainer container) { CookieContainer = container; }   // line 10-14
public CookieContainer CookieContainer { get; set; } = new CookieContainer();     // line 16
```
Passing `null` overwrites the initialised value; `ReadCookies` then throws
`ArgumentNullException` on `CookieContainer.Add`. The ctor should coalesce
(`CookieContainer = container ?? new CookieContainer();`) or throw.

### WC-3 — **P1** — a single hard-coded 5-second timeout for every request
```csharp
webRequest.Timeout = 5000; // 5 sec timeout   // line 25
```
`Timeout` is the response-header timeout, and the same 5 s is applied to the
release-page `DownloadString` **and** to the initial response of a
multi-hundred-MB mod download, so slow hosts fail deterministically. It must be
configurable, with a generous value for downloads and a short one for page
reads (`ReadWriteTimeout` defaults to 300 s and is left untouched).

### WC-4 — **P1** — `GetWebRequest` may return null
```csharp
WebRequest webRequest = base.GetWebRequest(address);
… webRequest.Timeout = 5000;   // line 25 — NRE when base returns null
```
`WebClient.GetWebRequest` returns `null` for schemes it does not handle.

### WC-5 — **P1** — `ReadCookies` assumes a non-null response
Lines 43-50; `base.GetWebResponse` can return `null`/throw, and
`response.Cookies` on a non-`HttpWebResponse` is guarded but the null response
is not.

### WC-6 — **P2** — visibility and documentation
The class is `public` in an otherwise `internal` namespace, has no XML summary,
and the only explanation is a bare StackOverflow URL comment (line 7).

---

## 7. `Sources/UserAgents.cs`

### UA-1 — **P2** — `public static readonly string` should be `const`
Lines 13-38: all values are compile-time literals. `public const string`
(or `internal static readonly`) avoids the static-initialisation cost and the
mutable-field smell; the class is `public` but only used inside the assembly.

### UA-2 — **P2** — duplicated user agent
`Chrome_Win7_Latest` (line 18) and `Chrome_Iron_Win7` (line 23) hold the exact
same string — one should alias the other.

### UA-3 — **P2** — naming convention violation
`Chrome_Win10_Latest`, `Firefox_Win10_Latest`, … use underscores; project
convention (and .NET) is PascalCase without separators.

### UA-4 — **P2** — hard-coded, outdated agent strings
All agents are from 2020 and every method body is a literal. Consider a
`const string Default = …` plus documented alternatives, and mark the file as
data.

### UA-5 — **P2** — meaningless doc comment
`/// My With Iron.` (line 21).

---

## 8. `Targets/UpdateTargetBase.cs`

### UTB-1 — **P0** — `PerformUpdate` reports success without doing anything
```csharp
if (ext == ".ZIP") { … }
else if (ext == ".DLL") { … }
else if (ext == ".7Z") { … }
else if (ext == ".RAR") { … }
// no else!
success = true;   // line 189
```
Any other extension (`.txt`, `.exe` for a mod, `.zip` with different casing is
handled, but e.g. `.bin`, `.json`, `.7z` uppercase is handled…) falls through
with `success = true`, so `RestoreSomeFiles` runs and
`meta.ini` `version` is written even though **no file was updated** — the mod
is marked as up to date and will never be retried.

### UTB-2 — **P0** — `Directory.Delete` can throw out of the catch block
```csharp
try
{
    …
    else
    {
        if (!updatingModDirPath.IsAnyFileExistsInTheDir()) Directory.Delete(updatingModDirPath);   // line 200
        return false;
    }
}
catch (Exception ex)
{
    _log.Warn(…);
    if (!updatingModDirPath.IsAnyFileExistsInTheDir()) Directory.Delete(updatingModDirPath);   // line 209
    return false;
}
```
`Directory.Delete(path)` (non-recursive) throws `IOException` when the
directory still contains sub-directories; line 209 then throws **from inside
the catch**, escaping `UpdateFiles` into `Updater`, which has no guard (UP-5).

### UTB-3 — **P0** — `File.Move` to a downloads directory that may not exist
```csharp
var newFilePathInDownloads = Info.UpdateFilePath.Replace(ManageSettings.Install2MoDirPath, ManageSettings.ModsUpdateDirDownloadsPath);
File.Move(Info.UpdateFilePath, newFilePathInDownloads);   // line 135
```
`File.Move` does not create the destination directory; and
`string.Replace` on paths silently produces a wrong path when
`Install2MoDirPath` appears in a sub-directory name. Use
`Path.Combine(ModsUpdateDirDownloadsPath, Path.GetFileName(...))` +
`Directory.CreateDirectory`.

### UTB-4 — **P0** — `RestoreList` can be null when `RestoreSomeFiles` runs
`RestoreList` (line 260) is assigned only inside `MakeBuckup()` (line 68).
`Mo.UpdateFiles` calls `RestoreSomeFiles(Info.BuckupDirPath, _moDirPath)`
(line 61) and `PerformUpdate` calls it at line 194 — both rely on
`MakeBuckup()` having run. `MakeBuckup` is `virtual`, and `Mo.MakeBuckup`
(line 94-98) calls `PreserveModOrganizerBasicGamePluginFiles()` **before**
`base.MakeBuckup()`, so any future override that returns early leaves
`RestoreList == null` → `NullReferenceException` at line 281/305/426.

### UTB-5 — **P1** — `MakeBuckup()` reads `Info.Target` instead of `this`
```csharp
foreach (var path in Info.Target.RestorePathsList())        // line 69
foreach (var path in Info.Target.RestorePathsListExtra())   // line 77
```
`Info.Target` is set by `Updater` (line 136) and happens to equal `this`, but
the indirection is unnecessary and NRE-prone; the methods are instance members
of the same object.

### UTB-6 — **P1** — `MakeBuckup()` success check is too weak
```csharp
MakeBuckup(BuckupDirPath, Info.TargetFolderPath.FullName);
return Directory.Exists(BuckupDirPath);   // line 94-96
```
`MakeBuckup(dir, dir)` creates `BuckupDirPath` unconditionally (line 373), so
the method returns `true` even when nothing was backed up. `Updater` then
proceeds to overwrite the mod with **no backup** (line 211).

### UTB-7 — **P1** — the prefix guard is unreadable and unexplained
Lines 115-127 compare
`Path.GetFileNameWithoutExtension(Info.UpdateFilePath)` with
`Info.UpdateFileStartsWith` **and** with
`Path.GetFileNameWithoutExtension(Info.UpdateFileStartsWith)` — applying
`GetFileNameWithoutExtension` to a *prefix* is meaningless. The intent (reject
a downloaded file that does not belong to the mod) should be expressed by one
named helper with a comment.

### UTB-8 — **P1** — `GetDllTargetDir` uses string replacement on paths
```csharp
return Path.GetDirectoryName(dll).Replace(Info.BuckupDirPath, updatingModDirPath);   // line 227
```
Case/format sensitive and wrong if `BuckupDirPath` appears twice; and
`Path.GetDirectoryName(dll)` may be null. Should compute the relative sub-path
and combine it.

### UTB-9 — **P1** — `RestoreSomeFiles` condition relies on `&&`/`||` precedence
Lines 301-306 mix `||` and `&&` in one expression without parentheses; the
reader must re-derive that `&&` binds tighter. Extract
`ShouldRestoreFile(file, ...)`. Also
`Directory.CreateDirectory(Path.GetDirectoryName(targetPath))` (line 284/311)
can receive `null`.

### UTB-10 — **P1** — `RestoreModFromBuckup` deletes target folders recursively
`if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);`
(line 336) — a restore is a destructive operation performed silently inside a
`catch`-per-item loop; it should at least be logged at `Info` level.

### UTB-11 — **P1** — `MakeBuckup(string, string)` moves the originals away
`folder.MoveTo(backupPath.FullName)` (line 400) and `file.MoveTo(backupPath)`
(line 432) physically remove the user's mod content before the new files are
extracted. If `UpdateFiles` then fails, `RestoreBuckup` must succeed or the mod
is lost. There is no verification step and `DirectoryInfo.MoveTo` throws if the
destination exists.

### UTB-12 — **P2** — empty, dead `UpdateFilenameSubPathData` initialiser
Lines 21-27: the dictionary is created with every entry commented out, so it is
always empty; the `TryGetValue` branch at line 163 is therefore dead. Either
restore the data or delete the field and the branch.

### UTB-13 — **P2** — typo in a log message
`"Update: Filed to move dir to bak dir. error:"` (line 406) → `Failed`.
The same "move file to bak dir" text is used for a *restore* operation
(line 292/320).

### UTB-14 — **P2** — `_log` is `protected static` (not `readonly`) and
`BuckupDirPath` (line 50) is declared in the middle of the class;
`RestorePathsList()`/`RestorePathsListExtra()` return
`new[] { string.Empty }` (a one-element array with an empty string) instead of
an empty array, forcing every caller to skip whitespace (lines 71, 79).

### UTB-15 — **P2** — hard-coded `"meta.ini"` and `"version"`/`"General"`
`Path.Combine(updatingModDirPath, "meta.ini")` (line 196) duplicates the same
literal from `ModsBase.SetCurrentVersion` (line 33).

---

## 9. `Targets/MO.cs`

### MO-1 — **P0** — `ProductVersion` can be null
```csharp
Info.TargetCurrentVersion = FileVersionInfo.GetVersionInfo(_moExePath).ProductVersion;   // line 85
```
`ProductVersion` is `string?`-like (null when the resource is absent), which
reproduces UI-1/UP-13: `UpdateTools.CleanVersion` dereferences
`version.Length` → `NullReferenceException`.

### MO-2 — **P2** — redundant `.\` prefix in the backup path list
```csharp
@".\ModOrganizer.ini",
@".\categories.dat",
@".\explorer++\Explorer++RU.dll",
@".\explorer++\config.xml",   // lines 18-21
```
Traced through: these strings are only ever consumed by
`UpdateTargetBase.MakeBuckup` via
`Path.GetFullPath(Path.Combine(Info.BuckupDirPath, path))` (line 73), so they
are resolved against the **backup directory** and the leading `.\` is harmless
(not CWD-relative). It is still redundant noise that reads like a CWD
dependency — drop the prefix.

### MO-3 — **P0** — `UpdateFiles` swallows the exception and can throw from the catch
```csharp
catch
{
    File.Delete(Info.UpdateFilePath);//maybe corrupted   // line 74
    return false;
}
```
Bare `catch` with no logging; and `File.Delete` on a locked/absent file throws
`IOException`/`DirectoryNotFoundException` from inside the catch, escaping to
`Updater`.

### MO-4 — **P0** — restore happens before the exit code is checked
```csharp
installer.Start();
installer.WaitForExit();
RestoreSomeFiles(Info.BuckupDirPath, _moDirPath);                       // line 61
ManageModOrganizer.RestoreMissingModOrganizerBasicGamePluginFiles();    // line 62
if (installer.ExitCode != 0) { File.Delete(Info.UpdateFilePath); }      // line 64-67
```
A failed installer still triggers the file restore and the plugin repair, and
the corrupt file is deleted only afterwards. The restore must be conditional on
`ExitCode == 0`.

### MO-5 — **P1** — no timeout on `WaitForExit()`
Line 59. A hung MO installer blocks the update thread forever with no
cancellation path.

### MO-6 — **P1** — `SetCurrentVersion` sets `Info.UpdateFileEndsWith` as a side effect
```csharp
internal override void SetCurrentVersion()
{
    Info.UpdateFileEndsWith = ".exe";   // line 81
```
A method whose name promises "read the current version" also mutates the
request that is sent to GitHub. This creates the undocumented ordering
requirement in `Updater.Update` (`SetCurrentVersion` at line 166 must run
before `GetLastVersion` at line 177). The value belongs in `GetUpdateInfos`.

### MO-7 — **P1** — `_moDirPath` is filled in by `GetUpdateInfos` and consumed later
`_moDirPath` is assigned inside `GetUpdateInfos()` (line 32) and read by
`UpdateFiles()` (line 56) and `SetCurrentVersion()` (line 82) — hidden temporal
coupling that breaks if the call order changes (e.g. a direct
`SetCurrentVersion()` call). Should be a property that reads
`ManageSettings.AppModOrganizerDirPath` on demand.

### MO-8 — **P1** — the dictionary key is an absolute path while other targets use names
`{ _moDirPath, "ModOrganizer2,modorganizer,Mod.Organizer-" }` (line 35) with
`GetParentFolderPath() => string.Empty` — this is what forces the fragile
`Path.Combine` in `Updater.cs:159` (UP-12).

### MO-9 — **P2** — naming/visibility
`Mo` vs `MO` (vs `MOBaseGames`, `ManageModOrganizer`) — inconsistent
capitalisation; `using System.Linq;` (line 4) is unused; the class has no XML
summary and no explicit `internal`.

---

## 10. `Targets/MOBaseGames.cs`

### MBG-1 — **DEAD (P0 for the build)** — the whole file is commented out
Lines 1-48 are entirely commented, yet the file is still in the compile list:
```xml
<Compile Include="Manage\Update\Targets\MOBaseGames.cs" />   <!-- AI Helper.csproj:326 -->
```
If it were ever uncommented it would not compile: it references `TBase`,
`updateInfo` (lower-case type name), `info.source.title`,
`ManageSettings.GetMOdirPath()`, `info.GetVersionFromLink` and
`info.SourceLink` — none of which exist any more. Either delete the file and
the `<Compile>` entry, or implement it against the current API.

---

## 11. `Targets/Mods/ModsBase.cs`

### MB-1 — **P0** — inverted condition drops every symlink path
```csharp
for (int i = 0; i < objectLinkPathsLength; i++)
{
    var linkPath = objectLinkPaths[i, 1];
    if (links.Contains(linkPath))      // line 61  ← should be !links.Contains
    {
        links.Add(linkPath);
    }
}
return links.ToArray();
```
The method is supposed to return the **distinct** set of link paths, but as
written it returns only the paths that occur **at least twice** (the first
occurrence is skipped because it is not yet in the set). For the normal case
(every path unique) the result is an **empty array**, so
`UpdateTargetBase.MakeBuckup` (line 77-92) never adds any symlink target to
`RestoreList` and symlinked mod content is not restored after an update.
Fix: `links.Add(linkPath);` (HashSet de-duplicates) — or invert the test.

### MB-2 — **P0** — `DirLinkPaths` may be null / ragged
`ManageSettings.Games.Game.DirLinkPaths` (line 52) is dereferenced without a
null check, and `objectLinkPaths.Length / 2` (line 54) assumes a perfectly
rectangular `[n, 2]` array; a jagged/`[n, 3]` array silently produces wrong
indices.

### MB-3 — **P1** — hard-coded `"meta.ini"` / `"version"` / `"General"`
`Path.Combine(Info.TargetFolderPath.FullName, "meta.ini")` (line 33) and
`ManageIni.GetIniValueIfExist(metaPath, "version", "General")` (line 36)
duplicate literals from `UpdateTargetBase` (line 196) — extract constants.

### MB-4 — **P1** — `UpdateFiles()` is a pass-through
`internal override bool UpdateFiles() => PerformUpdate();` (line 21-24) adds a
level of indirection with no behaviour; acceptable as a base-class contract,
but should be documented as such.

### MB-5 — **P2** — `SetCurrentVersion` leaves a stale version on a missing file
If `meta.ini` does not exist the method returns without touching
`Info.TargetCurrentVersion`, so the value from the previous folder (or `null`)
is used for comparison. `Reset()` mitigates this only because it sets `""`.

---

## 12. `Targets/Mods/ModsList.cs`

### ML-1 — **P2** — unused field with a static-path side effect
```csharp
readonly string _updateInfosFile = ManageSettings.UpdateInfosFilePath;   // line 13
```
Never read. It also evaluates a `ManageSettings` static at construction time
for no reason.

### ML-2 — **P1** — inconsistent "enabled mods only" behaviour vs `ModsMeta`
`GetUpdateInfos()` delegates to
`ManageUpdateMods.GetUpdateInfos(Info.SourceId)` (line 21), and that method
reads the **static** `ManageUpdateMods.UpdateOptions` field
(`ManageUpdateMods.cs:91`) instead of `Info.UpdateOptions`. `ModsMeta` uses
`Info.UpdateOptions.CheckEnabledModsOnlyCheckBox.Checked`
(`ModsMeta.cs:39`). The two targets therefore disagree whenever
`UpdateOptions` differs (e.g. the `MainForm` MO-only path passes `null`).

### ML-3 — **P2** — unused usings and missing docs
`using System;` (line 1) and `using System.IO;` (line 3) are unused; the class
has no summary even though its siblings do.

---

## 13. `Targets/Mods/ModsMeta.cs`

### MM-1 — **P1** — `ModInfo` lives in the wrong file and is only used by dead code
```csharp
internal class ModInfo { internal string Url; internal DirectoryInfo Moddir; }   // lines 9-13
```
The only consumers are `ModsMetaUrlBase`/`Xua`, which are themselves dead
(see MMU-1). `ModInfo` should live with them (or be deleted).

### MM-2 — **P1** — misleading tuple names
```csharp
foreach ((string s, string k) in new[] { (ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyUpdateName), ("General", "notes") })
{
    if (!ini.KeyExists(k, s)) continue;   // line 60-67
```
`s` holds the **section** and `k` the **key**, but `KeyExists` takes
`(key, section)` and `GetKey` takes `(section, key)`. The names invite exactly
the argument-order mistake the code currently avoids. Rename to
`(string section, string key)`.

### MM-3 — **P1** — unescaped and too-narrow regex
```csharp
var updateInfoMatch = Regex.Match(val, $"{Info.SourceId}::([^:]+)::");   // line 73
```
`Info.SourceId` is interpolated unescaped, and `[^:]+` cannot match a value
containing `:` (e.g. a future `https://…` payload). Use `Regex.Escape` and a
documented delimiter contract.

### MM-4 — **P2** — redundant `StartsWith` check
Lines 77-78: `updateInfoMatch.Value` is produced by a pattern that always
begins with `Info.SourceId`, so the `StartsWith` test can never be false.
(The case-insensitive comparison also contradicts the case-sensitive `Regex.Match`
on the previous line.)

### MM-5 — **P2** — `.Replace(", ", ",")` hides a format problem
Line 47. The parsing contract for the update record (`,` separated, no spaces)
should be enforced where the record is produced/validated, not patched here.

### MM-6 — **P2** — missing XML docs on `GetUpdateInfos`/`GetInfoFromMeta`, and
`infos.Add(...)` (line 47) will throw `ArgumentException` if
`EnumerateModNamesListFromActiveMoProfile` ever yields duplicates.

### MM-7 — **DEAD** — commented-out `_dbData`, `_dbDataParams`, `GetDb()`, lines 21-22, 32-37, 86-127.

---

## 14. `Targets/Mods/ModsMetaUrl/ModsMetaUrlBase.cs`

### MMU-1 — **DEAD (P0 for maintenance)** — the whole `ModsMetaUrl` subtree is unreachable
Nothing constructs `Xua`; the only reference is the commented-out block in
`ModsMeta.cs:32-37`. `ModsMetaUrlBase` and `Xua` are compiled but never
instantiated, so their bugs are latent.

### MMU-2 — **P2** — unused protected state
`Kk`, `Hs`, `Ai`, `Hs2` (lines 7-10) are never read; the only writer is a
commented-out `switch` (lines 16-30).

### MMU-3 — **P1** — `Xua.Check()` dereferences a possibly-null `Url`
`Modinfo.Url.Contains(...)` (`XUA.cs:17`) → `NullReferenceException` when the
meta record has no URL.

### MMU-4 — **P2** — hard-coded plugin paths in `Xua.StartsWith()`
`"BepInEx", "plugins", "XUnity.AutoTranslator", "XUnity.AutoTranslator.Plugin.Core.dll"`
(`XUA.cs:22-28`) duplicate knowledge that already lives in the installers.

### MMU-5 — **P2** — inconsistent member style
`Owner`/`Project` are properties while `StartsWith()`/`EndsWith()` are methods
returning the same kind of data; make them uniform (and prefer properties).
File name `XUA.cs` vs type `Xua` (`.editorconfig`/project convention).

---

## 15. `Sort/` subtree — `SideloaderPackBase.cs`, `SideloaderModPack.cs`, `SideloaderModPackCommunityUserdata.cs`, `UncensorSelectorBase.cs`, `UncensorSelectorFemale.cs`, `UncensorSelectorMale.cs`

### S-1 — **DEAD (P1)** — the entire subtree is never referenced
No file outside `Sort/` mentions `SideloaderPackBase` or any subclass. The
equivalent knowledge is hard-coded inside `ManageUpdateMods`:
`"Sideloader ModPack - Community UserData"` (`ManageUpdateMods.cs:283`),
`"Sideloader Modpack"` (`:361`), `"M"`/`"F"` suffixes (`:391-392`),
`"[Female]"` / `"[Penis]"` / `"[Balls]"` (`:458-459`). This is the clearest
candidate for "either wire it up or delete it".

### S-2 — **P1** — casing mismatch with the real directory names
`UncensorSelectorBase.DirName => "Sideloader ModPack - Uncensor Selector"`
(line 5) uses `ModPack` while `ManageUpdateMods` matches
`StartsWith("Sideloader Modpack")` (lower-case `p`). The two spellings cannot
both be right for the same on-disk folder.

### S-3 — **P1** — `UncensorSelectorMale` overrides nothing
`Sort/UncensorSelectorMale.cs` is an empty class body; the male filter
(`[Penis]`/`[Balls]`) lives in `ManageUpdateMods` instead — the abstraction is
half-implemented.

### S-4 — **P2** — expression-bodied properties allocate on every access
`internal virtual string[] ParentDirPath => new[] { "mods" };` (line 7) and
`FileNameFilter => new[] { "*.*" }` (line 9) build a new array per read;
`static readonly` backing fields are cheaper and make the values obviously
immutable.

### S-5 — **P2** — naming
`ParentDirPath` returns `string[]` (a list of segments) — the name reads like a
single path; `DirName`/`FileNameFilter` are fine.

---

## Cross-cutting summary

| ID | Severity | File | One-line |
|----|----------|------|----------|
| UT-1 | P0 | UpdateTools | `string.Compare(...) == 1` misses most "newer" results |
| UT-2 | P1 | UpdateTools | `ref string` for a pure transformation |
| UT-3 | P1 | UpdateTools | hidden side effects / double cleaning in `IsNewerOf` |
| UT-4 | P1 | UpdateTools | `public` helper class in an internal namespace |
| UT-5..7 | P2/DEAD | UpdateTools | magic pairs, docs, commented block |
| UI-1 | P0 | UpdateInfo | ctor without `Reset()` → null fields |
| UI-2 | P0 | UpdateInfo | `Reset()` misses `BuckupDirPath` |
| UI-3..7 | P1/P2/DEAD | UpdateInfo | mixed fields/properties, mutable builder, inconsistent empties, dead members |
| UP-1 | P0 | Updater | `StreamWriter` before `Directory.CreateDirectory` |
| UP-2 | P0 | Updater | `StreamReader` not disposed on exception |
| UP-3 | P0 | Updater | unguarded `DateTime.ParseExact` aborts the run |
| UP-4 | P0 | Updater | `oldCheckDateTime` gets the new value |
| UP-5 | P0 | Updater | `source.GetFile()` not guarded |
| UP-6 | P0 | Updater | `Length == 0` never detects a malformed record |
| UP-7 | P0 | Updater | progress form not in `try/finally` |
| UP-8..20 | P1/P2/DEAD | Updater | dead HTML flag, undisposed source, 180-line loop, naming, magic numbers, report handling |
| USB-1 | P0 | UpdateSourceBase | `IsCancelDownload` never reset → source poisoned |
| USB-2 | P0 | UpdateSourceBase | `IsCompletedDownload` never reset → false success |
| USB-3 | P0 | UpdateSourceBase | `Timer` NRE / `ObjectDisposedException` |
| USB-4 | P0 | UpdateSourceBase | `Timer` not disposed |
| USB-5..10 | P1/P2 | UpdateSourceBase | Dispose pattern, narrow catch, dead cookie feature, logging/naming |
| GH-1 | P0 | Github | `GetFile` re-throws → run aborted |
| GH-2 | P0 | Github | static UI controls touched from a pool thread |
| GH-3 | P0 | Github | progress form leaked when the download throws |
| GH-4 | P0 | Github | unescaped regex from user data |
| GH-5 | P1 | Github | no explicit guard for an empty resolved file name |
| GH-6 | P1 | Github | `FileInfo` on a file that may have vanished |
| GH-7 | P0 | Github | unreachable "already newer" early-out |
| GH-8 | P0 | Github | `SearchInLast10Releases` clobbers `Info.SourceLink` |
| GH-9 | P0 | Github | "last 10" loop is unbounded, no rate-limit handling |
| GH-10 | P0 | Github | `Found` always `true` → wrong capture group |
| GH-11..23 | P1/P2 | Github | parameter/field mix, dead param, hidden mutations, `Random`, case-sensitive 429, null path, logging |
| WC-1 | P0 | WebClientEx | `CacheIfAvailable` can serve stale release pages |
| WC-2 | P0 | WebClientEx | null `CookieContainer` accepted |
| WC-3 | P1 | WebClientEx | one hard-coded 5 s timeout for every request incl. downloads |
| WC-4..6 | P1/P2 | WebClientEx | null `WebRequest`, null response, visibility/docs |
| UA-1..5 | P2 | UserAgents | `const`, duplicated agent, naming, outdated data, doc typo |
| UTB-1 | P0 | UpdateTargetBase | unknown extension → `success = true` without any work |
| UTB-2 | P0 | UpdateTargetBase | `Directory.Delete` throws out of the catch |
| UTB-3 | P0 | UpdateTargetBase | `File.Move` into a non-existent directory |
| UTB-4 | P0 | UpdateTargetBase | `RestoreList` can be null |
| UTB-5..15 | P1/P2 | UpdateTargetBase | `Info.Target` self-reference, weak backup check, unreadable guard, path `Replace`, precedence, recursive delete, dead dictionary, typos |
| MO-1 | P0 | MO | `ProductVersion` null → NRE |
| MO-2 | P2 | MO | redundant `.\` prefix in the backup path list |
| MO-3 | P0 | MO | bare `catch` + throwing `File.Delete` |
| MO-4 | P0 | MO | restore before exit-code check |
| MO-5..9 | P1/P2 | MO | no installer timeout, side effect in `SetCurrentVersion`, `_moDirPath` coupling, absolute key, naming |
| MBG-1 | DEAD | MOBaseGames | fully commented file still in the compile list |
| MB-1 | P0 | ModsBase | inverted `Contains` → empty symlink restore list |
| MB-2 | P0 | ModsBase | null/ragged `DirLinkPaths` |
| MB-3..5 | P1/P2 | ModsBase | hard-coded meta literals, pass-through, stale version |
| ML-1..3 | P1/P2 | ModsList | unused field, inconsistent enabled-only filter, unused usings |
| MM-1..7 | P1/P2/DEAD | ModsMeta | misplaced `ModInfo`, misleading tuple names, unescaped regex, redundant check, hidden format patch, commented DB code |
| MMU-1..5 | DEAD/P1/P2 | ModsMetaUrl | unreachable subtree, unused flags, null `Url`, hard-coded paths, style |
| S-1..5 | DEAD/P1/P2 | Sort | unreachable subtree, casing mismatch, empty override, per-access allocations, naming |

**Totals:** 22 × P0, 37 × P1, 41 × P2, 9 × DEAD.

> Four findings were initially rated P0 and downgraded after closer tracing
> showed an existing guard further down the same code path: GH-5, GH-6, MO-2
> and WC-3. They are still fixed, but as robustness/consistency changes rather
> than behaviour corrections.

The concrete fix/refactor actions for every record above are listed in
`technical_task.md`.
