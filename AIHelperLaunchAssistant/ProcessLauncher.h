#pragma once

#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#include <string>

// Starting a program and waiting for one to end, which is the whole of what this tool does.

namespace Assistant
{
	/// <summary>
	/// Starts a program and hands back the handle of the process.
	/// </summary>
	/// <remarks>
	/// The program is named by its full path as well as being the first word of the command line, so
	/// that what is started is exactly the file the caller meant and not whatever the search path
	/// would have found for that name.
	/// </remarks>
	/// <param name="executablePath">Full path of the program to start.</param>
	/// <param name="arguments">
	/// Its command line, already formed and without the program's own name in it.
	/// </param>
	/// <param name="workingDirectory">
	/// Directory to start it in, or an empty string to let it inherit this process's own.
	/// </param>
	/// <param name="errorCode">The Windows error of the failure, when there was one.</param>
	/// <returns>
	/// The handle of the started process, which the caller waits on or closes, or <c>NULL</c> when
	/// nothing was started.
	/// </returns>
	HANDLE StartProcess(const std::wstring& executablePath,
		const std::wstring& arguments,
		const std::wstring& workingDirectory,
		DWORD& errorCode);

	/// <summary>
	/// Waits for a process to end.
	/// </summary>
	/// <remarks>
	/// A kernel wait, not a poll: the thread is asleep for the whole of the wait, costs no processor
	/// time, and the process can be paged out while it waits.
	/// </remarks>
	/// <param name="processId">Process to wait for.</param>
	/// <param name="timeoutSeconds">How long to wait; 0 waits without a deadline.</param>
	/// <returns>
	/// <c>true</c> when the process has ended, or when there is no such process to wait for - either
	/// way there is nothing left to wait for. <c>false</c> when the deadline passed with the process
	/// still running.
	/// </returns>
	bool WaitForProcessExit(unsigned long processId, unsigned long timeoutSeconds);
}
