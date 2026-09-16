# technical_task.md — Fix & refactor plan for the updater function files

Derived from every record in `todo.md`. One work package (WP) per file; each WP
lists the `todo.md` IDs it closes, the concrete change, and acceptance criteria.

---

## 1. Constraints and confirmed decisions

### 1.1 Language / platform constraints (hard)

* Target framework **.NET Framework 4.7.2**, language level **C# 7.3**
  (`AI Helper.csproj`, `ToolsVersion 15.0`, `TargetFrameworkVersion v4.7.2`).
* **Allowed:** expression-bodied members, `out var`, `is`-patterns, local
  functions (non-static), tuples + deconstruction, `readonly struct`, `in`
  parameters, `default` literal, throw expressions, `nameof`, string
  interpolation, `using static`.
* **Forbidden (C# 8+):** nullable reference types, `using` declarations,
  switch expressions, `??=`, ranges/indices (`..`, `^`), static local
  functions, target-typed `new()`, `IAsyncEnumerable`, `await using`,
  records, init-only setters, top-level statements.
* Keep WinForms + `System.Net.WebClient` + NLog + `INIFileMan` — no new
  dependencies, no new packages.

### 1.2 Confirmed scope decisions (user answers)

| # | Decision |
|---|----------|
| D1 | **Fix all P0 findings.** Behaviour changes are allowed for P0 fixes and must be recorded in §6. P1/P2 findings are refactored **only where the behaviour stays identical**. |
| D2 | **Delete dead files and remove them from the `.csproj`.** |
| D3 | **Safe subset of renames:** rename only identifiers fully contained in `Manage/Update/`. Keep `UpdateTools.CleanVersion` / `UpdateTools.IsNewerOf` signatures so `Manage/Functions/InfoEditorForFlp.cs:387` still compiles; keep `ManageUpdateMods.GetUpdateInfos(string)` unchanged. |
| D4 | **Fix correctness, keep the UI model.** Marshal WinForms updates with `Invoke`/`BeginInvoke`, replace blocking `Thread.Sleep` with an awaitable delay, keep the same `Form`-based progress windows and the same call flow. |

---

## 2. File-level plan

### 2.1 Deletions (D2)

| File | Reason (`todo.md`) |
|------|--------------------|
| `Targets/MOBaseGames.cs` | MBG-1 — fully commented, would not compile, still in `<Compile>` |
| `Sort/SideloaderPackBase.cs` | S-1 — never referenced |
| `Sort/SideloaderModPack.cs` | S-1 |
| `Sort/SideloaderModPackCommunityUserdata.cs` | S-1 |
| `Sort/UncensorSelectorBase.cs` | S-1, S-2, S-3 |
| `Sort/UncensorSelectorFemale.cs` | S-1 |
| `Sort/UncensorSelectorMale.cs` | S-1, S-3 |
| `Targets/Mods/ModsMetaUrl/ModsMetaUrlBase.cs` | MMU-1, MMU-2 |
| `Targets/Mods/ModsMetaUrl/XUA.cs` | MMU-1, MMU-3, MMU-4 |

Also delete the now-empty `Sort/` and `Targets/Mods/ModsMetaUrl/` directories,
remove the 9 matching `<Compile Include=…>` entries from `AI Helper.csproj`
(lines 316-328) and the `<None>`/folder leftovers if any.
`ModInfo` (declared in `ModsMeta.cs`, used only by the deleted subtree) is
removed as well → closes MM-1.

### 2.2 New file

| File | Purpose |
|------|---------|
| `UpdateIo.cs` | Small internal static helper shared by `Updater`, `Github`, `UpdateTargetBase` and `Mo`: safe delete (`TryDeleteDirectory`/`TryDeleteFile`), `GetFileLengthOrZero`, and `IsUnderPath` (case-insensitive, separator-aware containment test replacing the `string.Replace`-based path surgery). Closes UTB-2, UTB-3, UTB-8, MO-3, GH-6. |

### 2.3 Modified files

`UpdateTools.cs`, `UpdateInfo.cs`, `Updater.cs`, `Sources/UpdateSourceBase.cs`,
`Sources/Github.cs`, `Sources/WebClientEx.cs`, `Sources/UserAgents.cs`,
`Targets/UpdateTargetBase.cs`, `Targets/MO.cs`, `Targets/Mods/ModsBase.cs`,
`Targets/Mods/ModsList.cs`, `Targets/Mods/ModsMeta.cs`.

---

## 3. Work packages

### WP-1 — `UpdateTools.cs`

| todo.md | Action |
|---------|--------|
| UT-1 | Change `if (compareResult == 1)` → `if (compareResult > 0)` and `else if (compareResult < 0)` stays. Add a comment explaining that `string.Compare` returns an arbitrary signed value, not ±1. |
| UT-2 | **Keep the `ref` signature** (D3) but add a returning overload `CleanVersion(string)`; `CleanVersion(ref string)` becomes `version = CleanVersion(version);`. Existing callers keep compiling unchanged. |
| UT-3 | Extract `NormalizeForComparison(ref string a, ref string b)` used by `IsNewerOf`; document that `needClean` only controls the *second* normalisation and that `ConvertMODateVersion` is always applied. Behaviour unchanged. |
| UT-4 | `public static class UpdateTools` → `internal static class UpdateTools`. |
| UT-5 | Replace the five `.Replace(...)` calls with a `static readonly (string From, string To)[] VersionSuffixReplacements` table applied in a loop. Same input→output mapping. |
| UT-6 | Add XML docs for `needClean`, rename parameters to `candidateVersion` / `installedVersion`, document the return contract. |
| UT-7 | Delete the commented-out `foreach (var prefix in …)` block. |

Acceptance: `UpdateTools.CleanVersion(ref x)` and `IsNewerOf(a, b, false)` keep
their exact signatures; `InfoEditorForFlp.cs` compiles untouched;
`"1.2b".IsNewerOf("1.2a")` now returns `true`.

### WP-2 — `UpdateInfo.cs`

| todo.md | Action |
|---------|--------|
| UI-1 | Make `UpdateInfo()` the single initialising ctor and have the other chain: `public UpdateInfo(UpdateOptionsDialogForm updateOptions = null) : this() { UpdateOptions = updateOptions; }`. Remove the duplicate `UpdateOptions` assignment ambiguity. |
| UI-2 | Add `BuckupDirPath = ""` to `Reset()`. |
| UI-3 | Convert the bare fields to `internal` auto-properties with `internal set` where the setter is needed outside the class, and keep `Excluded`/`Report` as read-only collections. Keep the names identical so no other file changes. |
| UI-4 | Replace `public StringBuilder LastErrorText { get; }` with a private `StringBuilder _lastErrorText`, plus `internal void AppendError(string text)` and `internal string TakeAndClearError()`. Update `Updater`, `Github` and `UpdateSourceBase` call sites (all inside the Update folder). |
| UI-5 | Normalise `Reset()` to use `string.Empty` for all strings; document that `TargetFolderPath`/`TargetFolderUpdateInfo` are `null` until set. |
| UI-6 | Keep one public ctor with the optional parameter; delete the redundant explicit parameterless one. |
| UI-7 | Delete the commented-out members. |

Acceptance: `new UpdateInfo(null).TargetCurrentVersion` is `""` and
`CleanVersion` on it no longer throws; `new UpdateInfo(null).BuckupDirPath` is `""`.

### WP-3 — `Updater.cs`

| todo.md | Action |
|---------|--------|
| UP-1 | `SaveCheckDateTimes`: resolve the directory first, guard `Path.GetDirectoryName` against `null`, then open the writer inside `using`. |
| UP-2 | `GetDateTimesFromFile`: wrap `StreamReader` in `using`. |
| UP-3 | Use `DateTime.TryParseExact(..., "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)` and `continue` on failure; log a `Warn` once per bad line. |
| UP-4 | Capture the previous value **before** overwriting: `var hadOld = sourcePathDateCheckTime.TryGetValue(key, out var oldValue); sourcePathDateCheckTime[key] = checkDateTimeNow;` and restore `oldValue` (or remove the key) on failure. |
| UP-5 | Wrap `await source.GetFile()` in `try/catch`; on failure record the error via `info.AppendError(ex.Message)`, set `getFileIsTrue = false`, and let the existing failure path build the report line. |
| UP-6 | Replace `tInfoArray.Length == 0` with `tInfoArray.Length < MinUpdateInfoParts` (3) and log the malformed record; also guard the `Remove(...)` slicing against short/`::`-less values. |
| UP-7 | Put the whole loop body in `try { … } finally { DisposeSources(sources); pBar.Dispose(); progressForm.Dispose(); SaveCheckDateTimes(); }`. |
| UP-8 | Delete `_isHtmlReport` and every `… ? X : string.Empty` ternary around it (the value is always `true`); delete the now-unreachable non-HTML branch of `ShowReport`. |
| UP-9 | Dispose every `UpdateSourceBase` in the `finally` (closes the `WebClient`/`Timer` leak). Keep the one-element `sources` list because `Github` is the only source and the loop shape documents the extension point. |
| UP-10 | Decompose `Update()` into `UpdateSingleFolderAsync(...)`, `GetUpdateInfoArray(...)`, `BuildTargetFolderPath(...)`, `GetSourceCheckDates(...)`, `IsCheckTimeElapsed(...)`, `RestoreCheckDateTime(...)`, `WriteVersionStamp(...)` and the three report builders `BuildSuccessReportLine` / `BuildNoRemoteFileReportLine` / `BuildFailedReportLine` + `BuildInfoLinkSuffix`. The `Update()` body becomes the loop skeleton only. |
| UP-11 | Reduce ambient mutation: pass `source`/`target` as parameters to `UpdateSingleFolderAsync`; `info.Source`/`info.Target` are still assigned because sources/targets read them. |
| UP-12 | `BuildTargetFolderPath`: `Path.IsPathRooted(key) ? new DirectoryInfo(key) : new DirectoryInfo(Path.Combine(target.GetParentFolderPath(), key))`. |
| UP-13 | Replace `.Length == 0` with `string.IsNullOrWhiteSpace(...)` for `TargetLastVersion` / `TargetCurrentVersion`. |
| UP-14 | Rename `lastCheckDates` → `_lastCheckDates` (initialise at declaration) and `DateTimesFilePath` → `dateTimesFilePath`. |
| UP-15 | Extract `private static int GetUpdateCheckTimeoutMinutes()` with `const int DefaultUpdateCheckTimeoutMinutes = 10`; keep the same ini read-or-create semantics. |
| UP-16 | Rename `ErrorMessage` → `TakeAndClearError`; the side effect now lives on `UpdateInfo` (WP-2, UI-4). |
| UP-17 | Replace `Console.WriteLine(e.Message)` with `_log.Warn(ex, …)`; guard `File.ReadAllText` and the `Path.GetDirectoryName` null case in `ShowReport`. |
| UP-18 | Replace the `Split` + `lines.Length == 2` logic with `IndexOf(InfoLinkPattern)` + `Substring` so a line that contains the pattern more than once keeps its tail. |
| UP-19 | `internal bool UpdatedAny { get; private set; }`. |
| UP-20 | Delete all commented-out code (old progress form, `SaveSizes`, old report branches, `new MOBaseGames(info)`). |

Acceptance: `Updater.Update()` keeps the same public shape
(`internal async Task Update(List<UpdateTargetBase> targetList = null)`),
`UpdatedAny` stays readable from `ManageUpdateMods`, the report HTML for
success/warning/error is byte-identical to before, and no window survives a
thrown exception.

### WP-4 — `Sources/UpdateSourceBase.cs`

| todo.md | Action |
|---------|--------|
| USB-1 | Reset `IsCancelDownload = false` at the start of every download (`InitDownloadTimeTimer`). |
| USB-2 | Reset `IsCompletedDownload = false` at the start of every download; make the download method return `Task<bool>` so success is explicit. |
| USB-3 | Store the timer in a `System.Timers.Timer _downloadWatchdog` field; guard `RestartTimer()` with a null/`_watchdogStopped` check; stop and null the timer in `ReleaseTimer()` and inside `CancelDownload` before `CancelAsync` so the watchdog fires once. |
| USB-4 | Dispose `_downloadWatchdog` in `Dispose()`. |
| USB-5 | Implement the standard pattern: `public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }` + `protected virtual void Dispose(bool disposing)`, idempotent via a `_disposed` flag. |
| USB-6 | `DownloadFileTaskAsync` becomes `protected async Task<bool> DownloadFileTaskAsync(Uri uri, string updateFilePath)`: catch `Exception` (not just `WebException`), log via `_log.Warn(ex, …)`, append the message, return `false`. Keep the `WebException`-specific message wording. |
| USB-7 | Delete the dead `OnlineTranslatorCookies` property and pass `new CookieContainer()` explicitly (closes the `??` dead branch). |
| USB-8 | `protected static readonly Logger _log`. |
| USB-9 | `DownloadFileFromTheLink` becomes `abstract` **or** returns `Array.Empty<byte>()`; keep it `virtual` returning an empty array and document it, because only `Github` overrides it. |
| USB-10 | Remove the redundant `= false` initialiser; document `Info`/`WebClient` invariants in the class summary. |

Acceptance: a cancelled download no longer poisons later downloads; a failed
download no longer reports `true`; `Dispose()` is idempotent and releases the
timer.

### WP-5 — `Sources/Github.cs`

| todo.md | Action |
|---------|--------|
| GH-1 | `GetFile()` returns `false` instead of re-throwing; log with `_log.Error(ex, …)` and append the message. |
| GH-2 | Add `private static void SetDownloadProgress(int percentage)` that reads `_dwnpb` into a local, checks `null`/`IsDisposed`, and marshals with `BeginInvoke` when `InvokeRequired`; use it in `DownloadProgressChanged`. Same for the window title if it is ever updated. |
| GH-3 | Wrap the download section of `DownloadTheFile` in `try/finally { DisposeDownloadUi(); }`. |
| GH-4 | `Regex.Escape(Info.UpdateFileStartsWith)` / `Regex.Escape(Info.UpdateFileEndsWith)` in **all** patterns built from user data (`BuildFileLinkPattern`, `SearchForFileLinkWithFlexibleOwner`, both `SearchInLast10Releases` patterns, `ExtractVersionFromFileName`). Group numbering stays the same. |
| GH-5 | Add an explicit early `if (string.IsNullOrWhiteSpace(updateFileName)) { log; DisposeDownloadUi(); return false; }`. |
| GH-6 | Use `UpdateIo.GetFileLengthOrZero(path)` in `ShouldDownload` and in the "already exists" branch. |
| GH-7 | Delete the contradictory `&& !Info.VersionFromFile` so `IsCurrentVersionNewerThanRelease` actually runs on the multi-release path, as the comment intends. |
| GH-8 | Stop writing to `Info.SourceLink` inside `SearchInLast10Releases`; keep the releases-list URL in a local and pass it where needed. |
| GH-9 | Cap the loop at `MaxReleasesToSearch = 10` and call `Pause()` between requests. |
| GH-10 | Replace the always-true `Found` flag with an explicit `Found` that is set only when a match is returned, and use a named local instead of the misleading `searchedInMultipleReleases`. |
| GH-11 | `TryParseAndLoadAssetsPage` no longer writes into `Info.TargetFolderUpdateInfo`; it returns the (possibly changed) owner/repo and the caller applies them. Null-guard the array. |
| GH-12 | `SearchUpdateFilePath` drops the redundant `updateDownloadsDir` parameter, uses one `UpdateInfo` reference (the parameter) consistently and is renamed `FindExistingUpdateFile`. |
| GH-13 | Delete the dead `captureVersion` parameter from `BuildFileLinkPattern`. |
| GH-14 | Remove the hidden `CleanVersion` mutations from `IsCurrentVersionNewerThanRelease` and `TryFindUpdateFileLocally` — clean into locals instead of the shared fields. |
| GH-15 | Replace `new Random()` per call with a `static readonly Random` + `lock`, and replace `Thread.Sleep(ms)` with an awaited `Task.Delay` so the UI thread is not blocked (D4). `Pause()` stays a `void`-returning virtual on the base (signature unchanged) and delegates to an async helper where possible; if the signature must stay synchronous, use a bounded `Thread.Sleep` only when the caller is not on the UI thread. |
| GH-16 | `CheckIfStopWork` uses `IndexOf("(429)", StringComparison.OrdinalIgnoreCase) >= 0` **or** `IndexOf("too many requests", StringComparison.OrdinalIgnoreCase) >= 0`; keep using the `info` parameter. |
| GH-17 | Remove the meaningless `ProgressPercentage <= Maximum` guard (subsumed by GH-2's null guard). |
| GH-18 | Make `_updateFilePrefixes` and `_updateFileSuffixes` both `static readonly`. |
| GH-19 | Replace `ReleasesSearchResult` with a `readonly struct` that has a constructor, or return `(Match Match, string PageContent, bool Found)`. |
| GH-20 | Null-guard `Info.TargetFolderPath` in `LogFileLinkNotFound`. |
| GH-21 | Use NLog's `(Exception, string)` overloads and drop the construction-time `Info` log line. |
| GH-22 | Replace `MinValidLinkLength = 7` with `HrefPrefix.Length` (keep the constant name for readability if preferred). |
| GH-23 | Wrap `DownloadFileFromTheLink`'s `DownloadData` in `try/catch` returning an empty array on failure. |

Acceptance: a transient HTTP failure no longer aborts the run; the progress
window never throws a cross-thread exception and is always disposed; file-name
prefixes containing `[`, `]`, `.`, `+`, `(` are matched literally;
`Info.SourceLink` still points at the release after a multi-release search.

### WP-6 — `Sources/WebClientEx.cs`

| todo.md | Action |
|---------|--------|
| WC-1 | `CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)` so release pages are never served from the WinINet cache. |
| WC-2 | Ctor coalesces: `CookieContainer = container ?? new CookieContainer();`. |
| WC-3 | Add `internal int RequestTimeoutMilliseconds { get; set; } = DefaultRequestTimeoutMilliseconds` (`DefaultRequestTimeoutMilliseconds = 5000`) and use it in `GetWebRequest`; `UpdateSourceBase` raises it to `DownloadRequestTimeoutMilliseconds = 300000` for the duration of a file download and restores the default afterwards (requires the `WebClient` field in `UpdateSourceBase` to be typed `WebClientEx` — contained in the Update folder). |
| WC-4 | Null-guard the `WebRequest` returned by `base.GetWebRequest` before setting `Timeout`. |
| WC-5 | Null-guard the `WebResponse` in both `GetWebResponse` overrides before `ReadCookies`. |
| WC-6 | Add an XML summary explaining the class purpose and the cookie/threading contract; reduce visibility to `internal sealed`. |

Acceptance: release pages are always fetched from the network; a download is
not aborted after 5 s; `WebClientEx(null)` no longer produces a latent NRE.

### WP-7 — `Sources/UserAgents.cs`

| todo.md | Action |
|---------|--------|
| UA-1 | `public static readonly string` → `public const string` for every literal. |
| UA-2 | `Chrome_Win7_Latest` and `Chrome_Iron_Win7` → one const with the other as an alias const (`Chrome_Iron_Win7 = Chrome_Win7_Latest;`). |
| UA-3 | Rename to PascalCase without underscores (`ChromeWin10Latest`, `ChromeWin7Latest`, `ChromeIronWin7`, `FirefoxWin10Latest`, `EdgeWin10Latest`, `OperaMini`) — the class is referenced only from `UpdateSourceBase`, which is inside the Update folder (D3). Keep the old names as `[Obsolete]`-free aliases only if needed; otherwise update the single call site. |
| UA-4 | Keep the values (behaviour), add a file-level summary explaining they are a snapshot and where to update them. |
| UA-5 | Delete the meaningless `/// My With Iron.` doc. |

Acceptance: `UpdateSourceBase` still sets the same UA header value
(`Chrome_Iron_Win7` == `Chrome_Win7_Latest` == the same string as today).

### WP-8 — `Targets/UpdateTargetBase.cs`

| todo.md | Action |
|---------|--------|
| UTB-1 | Add an explicit `else { _log.Warn("Unsupported update file extension …"); success = false; }` so an unknown extension fails the update instead of marking the mod up to date. |
| UTB-2 | Replace both `Directory.Delete(updatingModDirPath)` calls with `UpdateIo.TryDeleteDirectory(updatingModDirPath, recursive: true)` (never throws). |
| UTB-3 | Replace the `string.Replace` + `File.Move` with `Directory.CreateDirectory(ManageSettings.ModsUpdateDirDownloadsPath)` + `Path.Combine(ManageSettings.ModsUpdateDirDownloadsPath, Path.GetFileName(Info.UpdateFilePath))` and guard with `UpdateIo.IsUnderPath`. |
| UTB-4 | Initialise `protected readonly HashSet<string> RestoreList = new HashSet<string>();` at declaration (no longer only inside `MakeBuckup`). |
| UTB-5 | Use `RestorePathsList()` / `RestorePathsListExtra()` directly instead of `Info.Target.…`. |
| UTB-6 | `MakeBuckup()` verifies the copy actually happened: return `false` when the source folder has content but the backup directory is empty (`!BuckupDirPath.IsAnyFileExistsInTheDir() && Info.TargetFolderPath.IsAnyFileExistsInTheDir()`). |
| UTB-7 | Extract `IsUpdateFileForThisTarget(...)` with an explanatory comment; same predicate, one named place. |
| UTB-8 | `GetDllTargetDir` computes the sub-path with `Path.GetRelativePath`-equivalent logic (substring after the backup root + `Path.Combine`) instead of `string.Replace`; null-guard `Path.GetDirectoryName`. |
| UTB-9 | Extract `ShouldRestoreFile(FileInfo file, string oldModBuckupDirPath)` with explicit parentheses so the `&&`/`||` precedence is readable; guard `Path.GetDirectoryName` before `Directory.CreateDirectory`. |
| UTB-10 | Log `RestoreModFromBuckup` deletions at `Info` level and use `UpdateIo.TryDeleteDirectory`. |
| UTB-11 | Add a `_log.Info` before the destructive `MoveTo` calls so a crash mid-update is diagnosable; guard `DirectoryInfo.MoveTo` against an existing destination. |
| UTB-12 | Delete the always-empty `UpdateFilenameSubPathData` field and the dead `TryGetValue` branch (or keep the field and document it as the extension point — chosen: **delete**, the branch becomes the `GetDllTargetDir` path). |
| UTB-13 | Fix the `"Filed"` → `"Failed"` typo and use the correct wording for the restore vs backup paths. |
| UTB-14 | `protected static readonly Logger _log`; move `BuckupDirPath` next to the other fields; return `new string[0]` (or `Array.Empty<string>()`) instead of `new[] { string.Empty }` from the two `RestorePathsList*` defaults and drop the now-unneeded whitespace skips (keep them defensively). |
| UTB-15 | Add `internal const string MetaIniFileName = "meta.ini";` and `internal const string MetaIniVersionSection = "General";` / `MetaIniVersionKey = "version";` and use them here and in `ModsBase`. |
| Naming (D3) | Rename `Buckup` → `Backup` in `BuckupDirPath`, `MakeBuckup`, `RestoreBuckup` (all confined to `UpdateTargetBase` and `MO`, both inside the Update folder). |

Acceptance: an update archive with an unsupported extension fails and is
reported; a partially-empty backup no longer reports success; no
`Directory.Delete` can escape `UpdateFiles`.

### WP-9 — `Targets/MO.cs`

| todo.md | Action |
|---------|--------|
| MO-1 | `Info.TargetCurrentVersion = FileVersionInfo.GetVersionInfo(_moExePath).ProductVersion ?? "";` and fall back to `FileVersion` when `ProductVersion` is empty. |
| MO-2 | Drop the redundant `.\` prefix from `_baseFilesToBackup`. |
| MO-3 | Replace the bare `catch` with `catch (Exception ex)` + `_log.Error(ex, …)`; guard `UpdateIo.TryDeleteFile(Info.UpdateFilePath)`. |
| MO-4 | Move `RestoreSomeFiles` + `RestoreMissingModOrganizerBasicGamePluginFiles()` **after** the `ExitCode` check and only run them when `ExitCode == 0`; delete the corrupt file first when the exit code is non-zero. |
| MO-5 | `installer.WaitForExit(InstallerTimeoutMilliseconds)` with `const int InstallerTimeoutMilliseconds = 10 * 60 * 1000`; treat a timeout as failure and kill the process. |
| MO-6 | Move `Info.UpdateFileEndsWith = ".exe"` out of `SetCurrentVersion()` into `GetUpdateInfos()` (where the rest of the request is built), removing the hidden ordering dependency on `Updater`. |
| MO-7 | Turn `_moDirPath` into `private static string MoDirPath => ManageSettings.AppModOrganizerDirPath;` (computed on demand); delete the field. |
| MO-8 | Keep the absolute key (behaviour) but document it; `Updater.BuildTargetFolderPath` now handles rooted keys explicitly (UP-12). |
| MO-9 | Delete the unused `using System.Linq;`, add a class summary, add explicit `internal`. Keep the type name `Mo` (D3 — it is referenced from `MainForm.cs`). |

Acceptance: a failed MO installer leaves the existing installation untouched;
a hung installer no longer blocks forever; `SetCurrentVersion` has no side
effects.

### WP-10 — `Targets/Mods/ModsBase.cs`

| todo.md | Action |
|---------|--------|
| MB-1 | Replace the inverted condition with `links.Add(linkPath)` (HashSet de-duplicates); skip empty entries. |
| MB-2 | Null-guard `ManageSettings.Games.Game.DirLinkPaths`; compute the row count as `objectLinkPaths.GetLength(0)` and the column count as `GetLength(1)` with a `>= 2` check instead of `Length / 2`. |
| MB-3 | Use `UpdateTargetBase.MetaIniFileName` / `MetaIniVersionSection` / `MetaIniVersionKey`. |
| MB-4 | Add a one-line summary explaining `UpdateFiles()` is the `UpdateTargetBase` contract implemented by `PerformUpdate()`. |
| MB-5 | When `meta.ini` is missing, explicitly set `Info.TargetCurrentVersion = ""` so no stale version is compared. |

Acceptance: `RestorePathsListExtra()` now returns the distinct link paths
(previously an empty array for unique paths); a mod without `meta.ini` compares
against an empty current version.

### WP-11 — `Targets/Mods/ModsList.cs`

| todo.md | Action |
|---------|--------|
| ML-1 | Delete the unused `_updateInfosFile` field. |
| ML-2 | **No behaviour change** (D1): keep `ManageUpdateMods.GetUpdateInfos(Info.SourceId)` and add a comment pointing at the `UpdateOptions` discrepancy with `ModsMeta`, so it can be resolved later. |
| ML-3 | Remove the unused `using System;` / `using System.IO;`; add a class summary. |

### WP-12 — `Targets/Mods/ModsMeta.cs`

| todo.md | Action |
|---------|--------|
| MM-1 | Delete `ModInfo` (only consumer is the deleted `ModsMetaUrl` subtree). |
| MM-2 | Rename the deconstructed tuple to `(string section, string key)`. |
| MM-3 | `Regex.Escape(Info.SourceId)` and replace `([^:]+)` with `([^\n\r]+?)` bounded by the trailing `::` (documented delimiter contract) so values containing `:` are handled. |
| MM-4 | Delete the redundant `StartsWith` check. |
| MM-5 | Move the `", "` → `","` normalisation into a named `NormalizeUpdateRecord` with a comment referencing the producer format. |
| MM-6 | Add XML docs; use indexer assignment (`infos[modPath] = …`) instead of `Add` so a duplicate name cannot throw. |
| MM-7 | Delete the commented-out `_dbData` / `_dbDataParams` / `GetDb()` block. |

Acceptance: a `meta.ini` record such as
`updgit::Owner,Repo,Prefix::TRUE` and one containing a `:` in the value both
parse; a duplicate mod name no longer throws.

---

## 4. Build order

1. Delete the dead files and fix `AI Helper.csproj`.
2. Add `UpdateIo.cs` (+ csproj entry).
3. WP-7 (`UserAgents`) → WP-6 (`WebClientEx`) → WP-4 (`UpdateSourceBase`) →
   WP-5 (`Github`) — leaf-to-root so the WebClient/UA call sites compile.
4. WP-1 (`UpdateTools`) → WP-2 (`UpdateInfo`) → WP-3 (`Updater`).
5. WP-8 (`UpdateTargetBase`) → WP-9 (`MO`) → WP-10 (`ModsBase`) →
   WP-11 (`ModsList`) → WP-12 (`ModsMeta`).
6. Verify `Manage/Functions/InfoEditorForFlp.cs` and `Forms/MainForm.cs`
   still compile (they are the two external call sites).

---

## 5. Acceptance criteria for the whole task

* `todo.md` P0 records: all closed (22 → 0 open).
* `todo.md` P1/P2 records: closed or explicitly re-scoped in §7.
* `todo.md` DEAD records: all closed by deletion.
* No C# 8+ syntax anywhere in the changed files (verify by inspection; the
  project has no `LangVersion` override so MSBuild 15 defaults to 7.3).
* `Updater.Update()` keeps its signature and its observable effects:
  the same report HTML, the same `updatecheckdatetimes.txt` format
  (`source|path|ISO8601`), the same `meta.ini` version write, the same
  `UpdatedAny` semantics.
* `usage.md` documents the post-change API.
* Re-review pass recorded in §8 of this file (or in `usage.md`'s appendix).

---

## 6. Behaviour-change register (D1 — every observable change)

| ID | Before | After | Why |
|----|--------|-------|-----|
| UT-1 | `1.2b` vs `1.2a` → "not newer" | "newer" | `string.Compare` sign was misread |
| UI-1 | `new UpdateInfo(opts)` → `TargetCurrentVersion == null` | `== ""` | NRE source |
| UI-2 | stale `BuckupDirPath` carried over | cleared per folder | wrong backup dir |
| UP-3 | corrupt `updatecheckdatetimes.txt` aborted the run | bad lines are skipped | resilience |
| UP-4 | a failed mod was skipped for the whole timeout window | retried on the next run | restore logic was inverted |
| UP-5 | one HTTP error aborted the entire run | that folder is reported as failed, the run continues | resilience |
| UP-6 | malformed update record threw `InvalidOperationException` | skipped with a warning | resilience |
| UTB-1 | unknown extension → `success = true`, `meta.ini` written | fails, reported, retried | silent data loss |
| UTB-3 | `File.Move` into a missing directory threw | directory created | crash |
| UTB-6 | empty backup still reported success | reports failure | silent data loss |
| USB-1/2 | first cancel poisoned the source; a stale success was reused | flags reset per download | wrong results |
| GH-1 | exception aborted the run | returns `false` | resilience |
| GH-4 | `[Female]` etc. matched as a regex class | matched literally | wrong file chosen |
| GH-7 | "already newer" early-out was dead | it runs | unwanted downgrade |
| GH-8 | report link pointed at the releases list | points at the release | wrong report |
| GH-9 | unbounded release search | capped at 10 with pauses | rate limiting |
| GH-10 | wrong capture group → wrong version written to `meta.ini` | correct group | wrong version |
| WC-1 | release pages could be served from cache | always fetched | missed updates |
| MB-1 | symlink restore list was empty | distinct link paths | symlinked content was lost |
| MB-5 | a missing `meta.ini` reused the previous version | empty version | wrong comparison |
| MO-1 | `null` `ProductVersion` → NRE | `""` / `FileVersion` fallback | crash |
| MO-4 | a failed installer still restored/repair files | restore only on success | corrupted install |
| MO-6 | `SetCurrentVersion` mutated the request | mutation moved to `GetUpdateInfos` | hidden ordering |
| UP-8 | dead non-HTML report branch | removed | identical output (`_isHtmlReport` was always `true`) |
| UP-18 | a report line with 2+ link patterns lost its tail | tail preserved | only reachable with a malformed line |

Everything else is a behaviour-preserving refactor.

---

## 7. Explicitly out of scope

| todo.md | Reason |
|---------|--------|
| ML-2 | Fixing it requires changing `ManageUpdateMods.GetUpdateInfos` (outside the Update folder) or duplicating its body; recorded as a documented inconsistency instead (D3). |
| UP-11 (full) | Removing the `Info.Source` / `Info.Target` ambient state entirely means changing the `UpdateSourceBase` / `UpdateTargetBase` contracts; only the parameter passing inside `Updater` is cleaned up. |
| USB-10 / GH-19 style-only changes | Kept minimal to limit churn. |
| `ManageUpdateMods.cs`, `InfoEditorForFlp.cs`, `MainForm.cs` | Outside the requested scope; only verified to still compile. |

---

## 8. Re-review checklist (used by the final pass)

- [x] Every P0 ID in `todo.md` has a matching change in the source.
- [x] Every §6 row is observable in the diff.
- [x] No `?`/`:` leftover from `_isHtmlReport`.
- [x] No `Console.WriteLine` left in the Update folder.
- [x] No `Thread.Sleep` reachable from the UI thread without a comment.
- [x] No `catch { }` / bare `catch` left.
- [x] No `Regex` pattern built from user data without `Regex.Escape`.
- [x] No `new FileInfo(...).Length` without a guard.
- [x] No `Directory.Delete` without a guard.
- [x] `Buckup` no longer appears in the Update folder.
- [x] `Sort/` and `ModsMetaUrl/` no longer exist and are gone from the csproj.

### 8.1 Re-review result

Method: inspection + `grep` over `Manage/Update/**` (`.cs` only for the
syntax checks; `.md` hits are the documents themselves). A real compile was
**not possible** in this environment — see §8.2.

| # | Check | Result |
|---|-------|--------|
| 1 | P0 closure (22 records) | **Closed.** Each P0 verified in the source: UT-1 `>0/<0`; UI-1 ctor→`Reset()`; UI-2 `BackupDirPath` cleared; UP-1 `CreateDirectory` before `StreamWriter`; UP-2 `using` on `StreamReader`; UP-3 `TryParseExact`+skip; UP-4 `TryGetValue` before overwrite; UP-5 `try/catch` around `GetFile()`; UP-6 `Length < MinUpdateInfoParts`; UP-7 `try/finally` + `Dispose`; USB-1/2 flags reset in `StartDownloadWatchdog`; USB-3 null-guard + `ObjectDisposedException`; USB-4 `StopDownloadWatchdog` disposes; GH-1 returns `false`; GH-2 `BeginInvoke`; GH-3 `DisposeDownloadUi()` in catch; GH-4 `Regex.Escape`; GH-7 `IsCurrentVersionNewerThanRelease` now called (line 519); GH-8 `SourceLink` not touched, page returned; GH-9 `MaxReleasesToSearch` cap; GH-10 `Found` = match success; WC-1 `NoCacheNoStore`; WC-2 `?? new CookieContainer()`; UTB-1 `else` returns `false`; UTB-2 `TryDeleteDirectory`; UTB-3 `CreateDirectory` + `EnsureParentDirectory`; UTB-4 `RestoreList` initialised at declaration; MO-1 `ProductVersion ?? FileVersion`; MO-3 `catch (Exception)` + `TryDeleteFile`; MO-4 restore after `ExitCode == 0`; MB-1 `links.Add`; MB-2 null/ragged guard. |
| 2 | §6 rows observable | **Yes.** All 25 rows trace to a concrete change in the listed file. |
| 3 | `_isHtmlReport` leftovers | **None** in `.cs` (only in the two documents). |
| 4 | `Console.WriteLine` | **None** in `.cs`; all logging goes through `_log`. |
| 5 | `Thread.Sleep` | **None** in `.cs`; replaced by `PauseAsync()` / `Task.Delay`. |
| 6 | bare `catch` | **None** in `.cs`; every handler names the exception and logs. |
| 7 | unescaped regex | **Clean.** Every pattern built from a record/file name is `Regex.Escape`d (Github 700-703, 720-723, 747, 805-807, 831-833, 924-926; ModsMeta 75). The two unescaped patterns (`AssetsPageRegexPattern`, `VersionTagRegexPattern`) are `const` literals, not data. |
| 8 | `new FileInfo(...).Length` | **None** in `.cs`; `UpdateIo.GetFileLengthOrZero` is used instead. |
| 9 | `Directory.Delete` / `File.Delete` | **Only inside `UpdateIo`**, wrapped in `try/catch` by design. No unguarded call remains. |
| 10 | `Buckup` | **One hit**: `ManageSettings.UpdatedModsOlderVersionsBuckupDirPath` in `UpdateTargetBase.cs:108`. This is an **external settings property name** outside the Update folder and outside D3 scope; the `Info.BuckupDirPath` / `MakeBuckup` / `RestoreBuckup` members were all renamed. Accepted as a false positive. |
| 11 | dead dirs + csproj | `Sort/` and `Targets/Mods/ModsMetaUrl/` no longer exist; the csproj has no reference to `SideloaderPackBase`, `SideloaderModPack`, `UncensorSelector*`, `MOBaseGames`, `ModsMetaUrl` or `XUA`; `Manage\Update\UpdateIo.cs` is registered (line 321). |
| 12 | C# 8+ syntax | **None.** Scans for `??=`, `using var`, `await using`, `is not`, `switch { }`, `new()`, ranges/indices, `IAsyncEnumerable`, `record`, `init;`, `with { }`, nullable reference types and static local functions returned only comment/XML-doc text. |
| 13 | `usage.md` | Written; documents the post-change API. |

### 8.2 Known limitations of this verification

* **No real build.** `SharpConfig`'s `<HintPath>` points to
  `K:\0 Repositories\[TokcDK]\SharpConfig\Src\bin\Release\net461\SharpConfig.dll`,
  which is missing on this machine, and both `MSBuild.exe` and `csc.exe` are
  blocked by the sandbox policy. The verification is therefore by inspection
  and `grep`, not by compiling.
* **External call sites were verified by inspection only**: `MainForm.cs:186`,
  `ManageUpdateMods.cs:112-115`, `InfoEditorForFlp.cs:364-378/442`. Their
  signatures still match (`Updater.Update`, `UpdatedAny`, `new Mo(new UpdateInfo(null))`,
  `UpdateInfo` object initialisers, `Github.GetFile()`, `ModsMeta.UpdateFiles()`,
  `UpdateTools.CleanVersion(ref string)`, `IsNewerOf`).
* **Two records remain open by design** (see §7): ML-2 (documented
  inconsistency in `ModsList`) and UP-11 (ambient `Info.Source`/`Info.Target`
  kept to avoid changing the base-class contracts).
