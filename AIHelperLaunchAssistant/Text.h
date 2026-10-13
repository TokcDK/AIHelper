#pragma once

#include <string>

// The two things the rest of the tool would otherwise have to write out again: reading a number
// out of an argument and writing one into a message. Kept here rather than in a string utility of
// their own because a tool this size does not need one.

namespace Assistant
{
	/// <summary>
	/// Reads a run of decimal digits into <paramref name="value"/>.
	/// </summary>
	/// <returns>
	/// <c>false</c> when the text is empty, holds anything but digits, or does not fit, in which case
	/// <paramref name="value"/> is left alone. A number too large to hold is refused rather than
	/// wrapped, because a process id that does not fit is not a process id.
	/// </returns>
	inline bool TryParseUnsigned(const std::wstring& text, unsigned long& value)
	{
		if (text.empty())
		{
			return false;
		}

		unsigned long result = 0;
		for (size_t index = 0; index < text.size(); ++index)
		{
			const wchar_t character = text[index];
			if (character < L'0' || character > L'9')
			{
				return false;
			}

			const unsigned long digit = static_cast<unsigned long>(character - L'0');
			if (result > (0xFFFFFFFFul - digit) / 10ul)
			{
				return false;
			}

			result = result * 10ul + digit;
		}

		value = result;
		return true;
	}

	/// <summary>Writes an unsigned number as text, for the messages the tool leaves behind.</summary>
	inline std::wstring ToText(unsigned long value)
	{
		wchar_t buffer[16];
		int start = static_cast<int>(sizeof(buffer) / sizeof(buffer[0]));

		do
		{
			--start;
			buffer[start] = static_cast<wchar_t>(L'0' + (value % 10ul));
			value /= 10ul;
		}
		while (value > 0ul && start > 0);

		return std::wstring(buffer + start, static_cast<size_t>(static_cast<int>(sizeof(buffer) / sizeof(buffer[0])) - start));
	}
}
