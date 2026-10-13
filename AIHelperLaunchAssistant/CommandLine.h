#pragma once

#include <string>
#include <vector>

// Turns a raw command line into a LaunchOptions, and describes the tool back to whoever asks.

namespace Assistant
{
	/// <summary>What the tool reports back to whatever started it.</summary>
	enum ExitCode
	{
		/// <summary>The whole sequence ran.</summary>
		ExitSuccess = 0,

		/// <summary>Something that is not a case of its own went wrong.</summary>
		ExitUnexpected = 1,

		/// <summary>The command line was refused, so nothing was started.</summary>
		ExitBadCommandLine = 2,

		/// <summary>The target program could not be started.</summary>
		ExitTargetNotStarted = 3,

		/// <summary>The host program could not be started again.</summary>
		ExitHostNotStarted = 4
	};

	/// <summary>Everything the tool was asked to do.</summary>
	struct LaunchOptions
	{
		/// <summary>Program to start again once the target has ended.</summary>
		std::wstring HostPath;

		/// <summary>Program to start once the host has ended.</summary>
		std::wstring TargetPath;

		/// <summary>Command line for the target, already formed.</summary>
		std::wstring TargetArguments;

		/// <summary>Directory to start the target in; empty means the target's own folder.</summary>
		std::wstring WorkingDirectory;

		/// <summary>Arguments for the host program, each quoted as the host's line is written.</summary>
		std::vector<std::wstring> HostArguments;

		/// <summary>Process to wait for before starting the target; 0 waits for nothing.</summary>
		unsigned long HostProcessId;

		/// <summary>How long to wait for it; 0 waits without a deadline.</summary>
		unsigned long HostWaitSeconds;

		/// <summary>Whether the host is to be started again once the target has ended.</summary>
		bool RestartHost;

		LaunchOptions()
			: HostProcessId(0)
			, HostWaitSeconds(0)
			, RestartHost(true)
		{
		}
	};

	/// <summary>What came of reading the command line.</summary>
	enum ParseOutcome
	{
		ParseSucceeded,
		ParseShowHelp,
		ParseShowVersion,
		ParseFailed
	};

	struct ParseResult
	{
		ParseOutcome Outcome;

		/// <summary>The options, when <see cref="Outcome"/> is ParseSucceeded.</summary>
		LaunchOptions Options;

		/// <summary>Why the line was refused, when <see cref="Outcome"/> is ParseFailed.</summary>
		std::wstring Error;

		ParseResult()
			: Outcome(ParseFailed)
		{
		}
	};

	/// <summary>
	/// Reads the command line, arguments 1..n-1 of <paramref name="arguments"/>.
	/// </summary>
	/// <remarks>
	/// Pure: it starts nothing and reads nothing from disk, which is what lets the whole contract
	/// with AI Helper be read in one place. Anything that can only be answered by looking at the
	/// machine - whether the target exists, where the host is - is left to the run itself.
	/// </remarks>
	ParseResult ParseCommandLine(int argumentCount, wchar_t** arguments);

	/// <summary>The text behind --help.</summary>
	std::wstring BuildUsageText();

	/// <summary>The text behind --version.</summary>
	std::wstring BuildVersionText();
}
