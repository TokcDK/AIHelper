#pragma once

#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#include <string>

// Where things are. The tool is deployed inside the application it belongs to, at
// <application>\RES\tools\aihelper-launch-assistant, so both the application and the folder the
// tool's own log goes in are found from the tool's location and are not passed in.

namespace Assistant
{
	/// <summary>Full path of the running executable, or an empty string if it cannot be read.</summary>
	inline std::wstring GetExecutablePath()
	{
		std::wstring buffer(MAX_PATH, L'\0');

		for (;;)
		{
			const DWORD length = GetModuleFileNameW(NULL, &buffer[0], static_cast<DWORD>(buffer.size()));
			if (length == 0)
			{
				return std::wstring();
			}

			if (static_cast<size_t>(length) < buffer.size())
			{
				buffer.resize(length);
				return buffer;
			}

			// The answer filled the buffer, so it was cut short; ask again with twice the room.
			buffer.resize(buffer.size() * 2);
		}
	}

	/// <summary>
	/// Directory part of a path, without the separator that ended it, or an empty string when there
	/// is none.
	/// </summary>
	inline std::wstring DirectoryName(const std::wstring& path)
	{
		const size_t separator = path.find_last_of(L"\\/");
		if (separator == std::wstring::npos)
		{
			return std::wstring();
		}

		// "C:\folder" has "C:" above it, not the empty name before the colon.
		if (separator == 2 && path.size() > 2 && path[1] == L':')
		{
			return path.substr(0, 2);
		}

		return path.substr(0, separator);
	}

	/// <summary>File name part of a path, which is the whole path when it has no separator.</summary>
	inline std::wstring FileName(const std::wstring& path)
	{
		const size_t separator = path.find_last_of(L"\\/");
		return separator == std::wstring::npos ? path : path.substr(separator + 1);
	}

	/// <summary>Joins a directory and a name with exactly one separator between them.</summary>
	inline std::wstring Combine(const std::wstring& directory, const std::wstring& name)
	{
		if (directory.empty())
		{
			return name;
		}

		const wchar_t last = directory[directory.size() - 1];
		if (last == L'\\' || last == L'/')
		{
			return directory + name;
		}

		return directory + L"\\" + name;
	}

	/// <summary>Whether a file - not a directory - is there.</summary>
	inline bool FileExists(const std::wstring& path)
	{
		if (path.empty())
		{
			return false;
		}

		const DWORD attributes = GetFileAttributesW(path.c_str());
		return attributes != INVALID_FILE_ATTRIBUTES && (attributes & FILE_ATTRIBUTE_DIRECTORY) == 0;
	}

	/// <summary>
	/// Looks for a file in the folder of the running executable and in up to
	/// <paramref name="maxDepth"/> folders above it.
	/// </summary>
	/// <returns>Its full path, or an empty string when it is not there.</returns>
	inline std::wstring FindNearExecutable(const std::wstring& fileName, int maxDepth)
	{
		std::wstring directory = DirectoryName(GetExecutablePath());

		for (int depth = 0; depth < maxDepth && !directory.empty(); ++depth)
		{
			const std::wstring candidate = Combine(directory, fileName);
			if (FileExists(candidate))
			{
				return candidate;
			}

			const std::wstring parent = DirectoryName(directory);
			if (parent == directory)
			{
				// At a drive root, or at a name with nothing above it: the walk is over.
				break;
			}

			directory = parent;
		}

		return std::wstring();
	}
}
