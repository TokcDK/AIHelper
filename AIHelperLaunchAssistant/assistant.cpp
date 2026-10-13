#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shellapi.h>

#include "ArgumentQuoting.h"
#include "CommandLine.h"
#include "Paths.h"
#include "ProcessLauncher.h"
#include "Text.h"
#include "ToolLog.h"

// aihelper-launch-assistant
//
// Starts a program - a game or a studio - and puts the application that asked for it back once that
// program has ended, so that the application does not have to sit in memory for the length of a
// whole game session.
//
// It is native and it has no window on purpose. It stays resident while the game runs, so what it
// costs while it does nothing is the whole point of it: a native process with no window, no message
// loop and no runtime is a megabyte of working set that the system is free to page out, against the
// tens of megabytes a managed one would hold. The wait for the game is a kernel wait, so it also
// costs no processor time.
//
// See light-assistant.md beside this file for the reasoning, and Shared\ArgumentQuoting.cs for the
// other half of the command line contract.

namespace Assistant
{
	namespace
	{
		/// <summary>Name of the application the tool ships with, which is found next to it.</summary>
		const wchar_t* const HostExecutableName = L"AI Helper.exe";

		/// <summary>How far up from the tool's own folder to look for it.</summary>
		const int HostSearchDepth = 4;

		/// <summary>Shows a line of text in a window of its own.</summary>
		/// <remarks>
		/// A window is raised here and nowhere else: --help and --version are only ever asked for by
		/// hand, and a windowed tool that answers nothing on the console has no other way to answer.
		/// </remarks>
		void ShowText(const std::wstring& text)
		{
			MessageBoxW(NULL, text.c_str(), L"aihelper-launch-assistant",
				MB_OK | MB_ICONINFORMATION | MB_SETFOREGROUND);
		}

		/// <summary>Waits for the application that asked for the launch to close.</summary>
		/// <remarks>
		/// The wait is what keeps the two apart. That application is a mod manager and is still writing
		/// its own files while it shuts down, and starting a game against a half-closed manager is how
		/// a launch ends up fighting with it over the same files.
		/// </remarks>
		void WaitForHost(const LaunchOptions& options)
		{
			if (options.HostProcessId == 0)
			{
				return;
			}

			if (!WaitForProcessExit(options.HostProcessId, options.HostWaitSeconds))
			{
				// Starting the program late is better than not starting it at all: the application may
				// be stuck rather than working.
				WriteLog(L"WARN ", L"Gave up waiting for process " + ToText(options.HostProcessId)
					+ L"; starting the program anyway.");
			}
		}

		/// <summary>Starts the application again, which is the last thing the tool does.</summary>
		bool StartHost(const LaunchOptions& options)
		{
			const std::wstring hostPath = options.HostPath.empty()
				? FindNearExecutable(HostExecutableName, HostSearchDepth)
				: options.HostPath;

			if (hostPath.empty())
			{
				WriteLog(L"ERROR", std::wstring(L"Could not find ") + HostExecutableName
					+ L" near the tool, so nothing was started again.");
				return false;
			}

			DWORD errorCode = ERROR_SUCCESS;
			const HANDLE host = StartProcess(hostPath, JoinArguments(options.HostArguments),
				DirectoryName(hostPath), errorCode);

			if (host == NULL)
			{
				WriteLog(L"ERROR", L"Could not start " + hostPath + L" again (Windows error "
					+ ToText(errorCode) + L").");
				return false;
			}

			// Closed straight away: the application keeps running, and only the handle this process
			// has no further use for goes away.
			CloseHandle(host);
			return true;
		}

		/// <summary>The whole sequence.</summary>
		int Run(const LaunchOptions& options)
		{
			if (!FileExists(options.TargetPath))
			{
				// Checked before the wait for the application, and on purpose: the application is still
				// running at this moment and is watching the tool, so refusing here is what lets it
				// start the program itself instead of the user being left with nothing running.
				WriteLog(L"ERROR", L"There is no such program: " + options.TargetPath);
				return ExitTargetNotStarted;
			}

			WaitForHost(options);

			// Worked out here rather than inherited: there is now a second process between the
			// application and the program being started, and an inherited directory would depend on
			// where that process happened to be started from.
			const std::wstring workingDirectory = options.WorkingDirectory.empty()
				? DirectoryName(options.TargetPath)
				: options.WorkingDirectory;

			DWORD errorCode = ERROR_SUCCESS;
			const HANDLE target = StartProcess(options.TargetPath, options.TargetArguments,
				workingDirectory, errorCode);

			if (target == NULL)
			{
				WriteLog(L"ERROR", L"Could not start " + options.TargetPath + L" (Windows error "
					+ ToText(errorCode) + L").");

				// The application is already gone by now, so leaving it closed would strand the user
				// with nothing running at all.
				StartHost(options);
				return ExitTargetNotStarted;
			}

			// A kernel wait, not a poll: the thread is asleep, nothing is looked at while it sleeps,
			// and this process can be paged out for the whole of the game session.
			WaitForSingleObject(target, INFINITE);
			CloseHandle(target);

			if (!options.RestartHost)
			{
				return ExitSuccess;
			}

			return StartHost(options) ? ExitSuccess : ExitHostNotStarted;
		}
	}
}

int WINAPI wWinMain(HINSTANCE instance, HINSTANCE previousInstance, PWSTR commandLine, int showCommand)
{
	UNREFERENCED_PARAMETER(instance);
	UNREFERENCED_PARAMETER(previousInstance);
	UNREFERENCED_PARAMETER(commandLine);
	UNREFERENCED_PARAMETER(showCommand);

	int argumentCount = 0;
	wchar_t** arguments = CommandLineToArgvW(GetCommandLineW(), &argumentCount);
	if (arguments == NULL)
	{
		Assistant::WriteLog(L"ERROR", L"The command line could not be read.");
		return Assistant::ExitUnexpected;
	}

	const Assistant::ParseResult parsed = Assistant::ParseCommandLine(argumentCount, arguments);
	LocalFree(arguments);

	switch (parsed.Outcome)
	{
	case Assistant::ParseShowHelp:
		Assistant::ShowText(Assistant::BuildUsageText());
		return Assistant::ExitSuccess;

	case Assistant::ParseShowVersion:
		Assistant::ShowText(Assistant::BuildVersionText());
		return Assistant::ExitSuccess;

	case Assistant::ParseFailed:
		// No window and no message box here, and that is the point: this path is reached while the
		// application that started the tool is still watching it, and a window raised over the game
		// to report a bad command line would be worse than the bad command line itself. The
		// application reads the exit code and starts the program itself.
		Assistant::WriteLog(L"ERROR", L"The command line was refused: " + parsed.Error);
		return Assistant::ExitBadCommandLine;

	default:
		return Assistant::Run(parsed.Options);
	}
}
