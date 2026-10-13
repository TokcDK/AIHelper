#include "ArgumentQuoting.h"

namespace Assistant
{
	namespace
	{
		/// <summary>Whether a value has anything in it that a command line would lose unquoted.</summary>
		bool NeedsQuotes(const std::wstring& argument)
		{
			return argument.empty()
				|| argument.find_first_of(L" \t\"") != std::wstring::npos;
		}
	}

	std::wstring QuoteArgument(const std::wstring& argument)
	{
		if (argument.empty())
		{
			// An empty argument still has to reach the next process as an argument, and a bare
			// nothing between two spaces does not.
			return L"\"\"";
		}

		if (!NeedsQuotes(argument))
		{
			return argument;
		}

		std::wstring quoted;
		quoted.reserve(argument.size() + 2);
		quoted += L'"';

		size_t backslashes = 0;
		for (size_t index = 0; index < argument.size(); ++index)
		{
			const wchar_t character = argument[index];

			if (character == L'\\')
			{
				// A run of backslashes is only doubled in front of a quote, so it is counted here and
				// written out once the character after it is known.
				++backslashes;
				continue;
			}

			if (character == L'"')
			{
				// A quote is escaped by the backslashes before it, doubled, plus the escape itself.
				quoted.append(backslashes * 2 + 1, L'\\');
				quoted += L'"';
				backslashes = 0;
				continue;
			}

			if (backslashes > 0)
			{
				quoted.append(backslashes, L'\\');
				backslashes = 0;
			}

			quoted += character;
		}

		// A run of backslashes just before the closing quote has to be doubled as well.
		quoted.append(backslashes * 2, L'\\');
		quoted += L'"';

		return quoted;
	}

	std::wstring JoinArguments(const std::vector<std::wstring>& arguments)
	{
		std::wstring joined;

		for (size_t index = 0; index < arguments.size(); ++index)
		{
			if (index > 0)
			{
				joined += L' ';
			}

			joined += QuoteArgument(arguments[index]);
		}

		return joined;
	}
}
