#pragma once

#include <string>
#include <vector>

// Puts one argument back into the single string a Windows process is started with.
//
// This is the C++ half of the format described in Shared\ArgumentQuoting.cs, and it applies exactly
// the same rules: AI Helper writes the command line this tool is started with, this tool takes that
// line apart and writes the command line the target and the restarted application are started with,
// and the next process takes those apart in turn. Every one of those has to agree on the rules, so
// the two files have to be kept in step - the end-to-end harness is what proves they are.
//
// The rules are the ones CommandLineToArgvW - which is what each of those programs uses to take a
// line apart again - actually applies, and they are the reason this cannot simply be
// "\"" + value + "\"": a path ending in a backslash needs that backslash doubled, and a value
// containing a quote needs it escaped with backslashes of its own.

namespace Assistant
{
	/// <summary>
	/// Quotes an argument if it needs it, and returns it unchanged if it does not.
	/// </summary>
	std::wstring QuoteArgument(const std::wstring& argument);

	/// <summary>
	/// Joins arguments into one command line, quoting each of them so that the next process takes the
	/// line apart into exactly the same list.
	/// </summary>
	std::wstring JoinArguments(const std::vector<std::wstring>& arguments);
}
