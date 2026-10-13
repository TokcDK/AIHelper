#pragma once

#include <string>

// The one line the tool leaves behind when something went wrong.

namespace Assistant
{
	/// <summary>
	/// Appends a warning or an error to the log beside the tool.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Warnings and errors only. A run that does what it was asked writes nothing at all, which is the
	/// point of a tool whose whole reason to exist is to cost nothing while a game runs: the
	/// application's own log records the hand-off, and the exit code says how this half of it went.
	/// </para>
	/// <para>
	/// Every write is best effort. The tool must not fail because the folder it lives in happens to be
	/// read only, so a log that cannot be written is not written and nothing else changes.
	/// </para>
	/// </remarks>
	/// <param name="level">One of L"WARN " or L"ERROR", padded to the width of the longest.</param>
	/// <param name="message">What happened, in one line and without a trailing newline.</param>
	void WriteLog(const wchar_t* level, const std::wstring& message);
}
