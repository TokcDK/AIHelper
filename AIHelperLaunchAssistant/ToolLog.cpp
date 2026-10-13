#include "ToolLog.h"

#include "Paths.h"

#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#include <stdio.h>
#include <vector>

namespace Assistant
{
	namespace
	{
		const wchar_t* const LogFileName = L"aihelper-launch-assistant.log";

		/// <summary>
		/// Size at which the log starts over, so that a folder used for years does not grow forever.
		/// </summary>
		const long long MaximumLengthBytes = 64ll * 1024ll;

		/// <summary>Now, as the log writes it.</summary>
		std::wstring TimeStamp()
		{
			SYSTEMTIME now;
			GetLocalTime(&now);

			wchar_t buffer[32];
			swprintf_s(buffer, L"%04u-%02u-%02u %02u:%02u:%02u",
				static_cast<unsigned int>(now.wYear),
				static_cast<unsigned int>(now.wMonth),
				static_cast<unsigned int>(now.wDay),
				static_cast<unsigned int>(now.wHour),
				static_cast<unsigned int>(now.wMinute),
				static_cast<unsigned int>(now.wSecond));

			return std::wstring(buffer);
		}

		/// <summary>Starts the log over when it has grown past its limit.</summary>
		void RestartIfTooLong(const std::wstring& path)
		{
			const HANDLE file = CreateFileW(path.c_str(), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE,
				NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
			if (file == INVALID_HANDLE_VALUE)
			{
				return;
			}

			LARGE_INTEGER length;
			length.QuadPart = 0;

			const bool tooLong = GetFileSizeEx(file, &length) != FALSE && length.QuadPart > MaximumLengthBytes;

			CloseHandle(file);

			if (tooLong)
			{
				DeleteFileW(path.c_str());
			}
		}

		/// <summary>Writes text as UTF-8, with a byte order mark when it starts the file.</summary>
		void AppendUtf8(HANDLE file, const std::wstring& text, bool startsTheFile)
		{
			const int length = static_cast<int>(text.size());

			const int byteCount = WideCharToMultiByte(CP_UTF8, 0, text.c_str(), length, NULL, 0, NULL, NULL);
			if (byteCount <= 0)
			{
				return;
			}

			std::vector<char> bytes(static_cast<size_t>(byteCount));
			WideCharToMultiByte(CP_UTF8, 0, text.c_str(), length, &bytes[0], byteCount, NULL, NULL);

			DWORD written = 0;

			if (startsTheFile)
			{
				// The mark is written once, when the file is made: a log opened by hand is read far
				// more often than it is read by a program, and it is not always opened as UTF-8.
				WriteFile(file, "\xEF\xBB\xBF", 3, &written, NULL);
			}

			WriteFile(file, &bytes[0], static_cast<DWORD>(bytes.size()), &written, NULL);
		}
	}

	void WriteLog(const wchar_t* level, const std::wstring& message)
	{
		const std::wstring directory = DirectoryName(GetExecutablePath());
		if (directory.empty())
		{
			// A bare file name has no folder to put a log in.
			return;
		}

		const std::wstring path = Combine(directory, LogFileName);

		RestartIfTooLong(path);

		// Opened for appending and shared, so that a run overlapping another cannot be what keeps the
		// tool from starting a game.
		const HANDLE file = CreateFileW(path.c_str(), FILE_APPEND_DATA, FILE_SHARE_READ | FILE_SHARE_WRITE,
			NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
		if (file == INVALID_HANDLE_VALUE)
		{
			return;
		}

		LARGE_INTEGER length;
		length.QuadPart = 0;
		const bool startsTheFile = GetFileSizeEx(file, &length) != FALSE && length.QuadPart == 0;

		AppendUtf8(file, TimeStamp() + L" " + level + L" " + message + L"\r\n", startsTheFile);

		CloseHandle(file);
	}
}
