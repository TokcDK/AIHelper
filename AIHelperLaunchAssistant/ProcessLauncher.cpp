#include "ProcessLauncher.h"

#include "ArgumentQuoting.h"

#include <vector>

namespace Assistant
{
	HANDLE StartProcess(const std::wstring& executablePath,
		const std::wstring& arguments,
		const std::wstring& workingDirectory,
		DWORD& errorCode)
	{
		errorCode = ERROR_SUCCESS;

		std::wstring commandLine = QuoteArgument(executablePath);
		if (!arguments.empty())
		{
			commandLine += L' ';
			commandLine += arguments;
		}

		// CreateProcessW is allowed to write to the command line buffer, so it has to be a mutable
		// one of its own and it has to outlive the call.
		std::vector<wchar_t> mutableCommandLine(commandLine.begin(), commandLine.end());
		mutableCommandLine.push_back(L'\0');

		STARTUPINFOW startupInformation;
		ZeroMemory(&startupInformation, sizeof(startupInformation));
		startupInformation.cb = sizeof(startupInformation);

		PROCESS_INFORMATION processInformation;
		ZeroMemory(&processInformation, sizeof(processInformation));

		// No console window for the child. A program that has a console would otherwise be given one
		// of its own, which is a black window over the game; the flag is ignored for a program that
		// has no console, so it costs nothing to ask for.
		const DWORD creationFlags = CREATE_NO_WINDOW;

		const BOOL started = CreateProcessW(
			executablePath.c_str(),
			&mutableCommandLine[0],
			NULL,					// process attributes
			NULL,					// thread attributes
			FALSE,					// handles are not inherited
			creationFlags,
			NULL,					// environment: this process's own
			workingDirectory.empty() ? NULL : workingDirectory.c_str(),
			&startupInformation,
			&processInformation);

		if (started == FALSE)
		{
			errorCode = GetLastError();
			return NULL;
		}

		// The thread handle is of no use here; the process handle is what the caller waits on.
		CloseHandle(processInformation.hThread);

		return processInformation.hProcess;
	}

	bool WaitForProcessExit(unsigned long processId, unsigned long timeoutSeconds)
	{
		// SYNCHRONIZE is all that waiting needs, and it is the one right that is granted for a
		// process this tool has no other claim on, so no privilege is required to wait.
		const HANDLE process = OpenProcess(SYNCHRONIZE, FALSE, processId);
		if (process == NULL)
		{
			// Nothing is using that id any more, so there is nothing left to wait for.
			return true;
		}

		const DWORD timeout = timeoutSeconds == 0 ? INFINITE : timeoutSeconds * 1000ul;
		const DWORD waited = WaitForSingleObject(process, timeout);

		CloseHandle(process);

		return waited == WAIT_OBJECT_0;
	}
}
