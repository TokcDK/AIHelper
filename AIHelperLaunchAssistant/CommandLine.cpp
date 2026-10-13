#include "CommandLine.h"

#include "ArgumentQuoting.h"
#include "Text.h"

namespace Assistant
{
	namespace
	{
		const wchar_t* const ToolName = L"aihelper-launch-assistant";
		const wchar_t* const ToolVersion = L"1.0";

		/// <summary>Everything after this argument belongs to the target program.</summary>
		const wchar_t* const ArgumentSeparator = L"--";

		/// <summary>
		/// Whether an argument is meant as an option rather than as a value.
		/// </summary>
		/// <remarks>
		/// A single "-" is a value, not an option: it is how a program is told to read its input, and
		/// refusing it here would make the tool unable to start something that wants it.
		/// </remarks>
		bool IsOption(const std::wstring& argument)
		{
			return argument.size() > 1 && argument[0] == L'-';
		}

		/// <summary>
		/// Splits "--name=value" into its two halves.
		/// </summary>
		/// <remarks>
		/// An argument that is not an option, or an option without an equals sign, comes back whole
		/// with no value of its own.
		/// </remarks>
		void SplitOption(const std::wstring& argument, std::wstring& name, bool& hasInlineValue, std::wstring& inlineValue)
		{
			name = argument;
			hasInlineValue = false;
			inlineValue.clear();

			if (!IsOption(argument))
			{
				return;
			}

			const size_t separator = argument.find(L'=');
			if (separator == std::wstring::npos || separator == 0)
			{
				return;
			}

			name = argument.substr(0, separator);
			inlineValue = argument.substr(separator + 1);
			hasInlineValue = true;
		}

		/// <summary>
		/// Reads the value of an option, from "--name=value" or from the argument after it, and
		/// refuses the line when there is none to read.
		/// </summary>
		bool ReadOptionValue(int argumentCount, wchar_t** arguments, int& index, bool hasInlineValue,
			const std::wstring& inlineValue, const std::wstring& optionName,
			std::wstring& value, ParseResult& result)
		{
			if (hasInlineValue)
			{
				value = inlineValue;
			}
			else if (index + 1 < argumentCount)
			{
				++index;
				value = arguments[index];
			}
			else
			{
				result.Error = L"Option " + optionName + L" needs a value.";
				return false;
			}

			if (value.empty())
			{
				result.Error = L"Option " + optionName + L" needs a value that is not empty.";
				return false;
			}

			return true;
		}

		/// <summary>Refuses a value given to an option that is a plain switch.</summary>
		bool RejectValue(bool hasInlineValue, const std::wstring& optionName, ParseResult& result)
		{
			if (!hasInlineValue)
			{
				return true;
			}

			result.Error = L"Option " + optionName + L" does not take a value.";
			return false;
		}
	}

	ParseResult ParseCommandLine(int argumentCount, wchar_t** arguments)
	{
		ParseResult result;

		if (argumentCount <= 1)
		{
			result.Outcome = ParseShowHelp;
			return result;
		}

		LaunchOptions options;
		bool targetGiven = false;
		bool hostGiven = false;
		bool argumentsGiven = false;

		// The line is read in two halves. Options are recognised from the start until the first
		// argument that is not one; from there on every argument is a value. That is what lets the
		// target's own arguments - which may well look like options - be written after the program
		// they belong to without anything being read into them.
		bool readingOptions = true;

		std::vector<std::wstring> positional;
		std::vector<std::wstring> separated;

		for (int index = 1; index < argumentCount; ++index)
		{
			const std::wstring argument = arguments[index];

			if (readingOptions && argument == ArgumentSeparator)
			{
				for (++index; index < argumentCount; ++index)
				{
					separated.push_back(arguments[index]);
				}

				break;
			}

			if (!readingOptions || !IsOption(argument))
			{
				readingOptions = false;
				positional.push_back(argument);
				continue;
			}

			std::wstring name;
			bool hasInlineValue = false;
			std::wstring inlineValue;
			SplitOption(argument, name, hasInlineValue, inlineValue);

			std::wstring value;

			if (name == L"--help" || name == L"-h" || name == L"-?")
			{
				result.Outcome = ParseShowHelp;
				return result;
			}

			if (name == L"--version" || name == L"-v")
			{
				result.Outcome = ParseShowVersion;
				return result;
			}

			if (name == L"--target" || name == L"-t")
			{
				if (!ReadOptionValue(argumentCount, arguments, index, hasInlineValue, inlineValue, name, value, result))
				{
					return result;
				}

				options.TargetPath = value;
				targetGiven = true;
				continue;
			}

			if (name == L"--arguments" || name == L"-a")
			{
				if (!ReadOptionValue(argumentCount, arguments, index, hasInlineValue, inlineValue, name, value, result))
				{
					return result;
				}

				options.TargetArguments = value;
				argumentsGiven = true;
				continue;
			}

			if (name == L"--working-dir" || name == L"-d")
			{
				if (!ReadOptionValue(argumentCount, arguments, index, hasInlineValue, inlineValue, name, value, result))
				{
					return result;
				}

				options.WorkingDirectory = value;
				continue;
			}

			if (name == L"--host")
			{
				if (!ReadOptionValue(argumentCount, arguments, index, hasInlineValue, inlineValue, name, value, result))
				{
					return result;
				}

				options.HostPath = value;
				hostGiven = true;
				continue;
			}

			if (name == L"--host-arg")
			{
				if (!ReadOptionValue(argumentCount, arguments, index, hasInlineValue, inlineValue, name, value, result))
				{
					return result;
				}

				options.HostArguments.push_back(value);
				continue;
			}

			if (name == L"--host-pid")
			{
				if (!ReadOptionValue(argumentCount, arguments, index, hasInlineValue, inlineValue, name, value, result))
				{
					return result;
				}

				unsigned long processId = 0;
				if (!TryParseUnsigned(value, processId) || processId == 0)
				{
					result.Error = L"Option " + name + L" needs a process id, not \"" + value + L"\".";
					return result;
				}

				options.HostProcessId = processId;
				continue;
			}

			if (name == L"--host-wait")
			{
				if (!ReadOptionValue(argumentCount, arguments, index, hasInlineValue, inlineValue, name, value, result))
				{
					return result;
				}

				unsigned long seconds = 0;
				if (!TryParseUnsigned(value, seconds))
				{
					result.Error = L"Option " + name + L" needs a number of seconds, not \"" + value + L"\".";
					return result;
				}

				options.HostWaitSeconds = seconds;
				continue;
			}

			if (name == L"--no-restart")
			{
				if (!RejectValue(hasInlineValue, name, result))
				{
					return result;
				}

				options.RestartHost = false;
				continue;
			}

			result.Error = L"Unknown option \"" + argument + L"\".";
			return result;
		}

		// What the values mean depends on which of the two programs was already named: naming either
		// of them with an option takes it out of the sequence, and the documented form names the
		// application first and the program second.
		size_t firstTargetArgument = 0;

		if (targetGiven)
		{
			// The program was named with an option, so every value is one of its arguments.
		}
		else if (hostGiven)
		{
			// The application was named with an option, so the first value is the program.
			if (!positional.empty())
			{
				options.TargetPath = positional[0];
				targetGiven = true;
				firstTargetArgument = 1;
			}
		}
		else if (positional.size() >= 2)
		{
			options.HostPath = positional[0];
			options.TargetPath = positional[1];
			targetGiven = true;
			firstTargetArgument = 2;
		}
		else if (positional.size() == 1)
		{
			// A lone value is the program to start: the application is found next to the tool.
			options.TargetPath = positional[0];
			targetGiven = true;
			firstTargetArgument = 1;
		}

		// Values after "--" belong to the target, and the first of them names it when it has not been
		// named yet.
		size_t firstSeparatedArgument = 0;
		if (!targetGiven && !separated.empty())
		{
			options.TargetPath = separated[0];
			targetGiven = true;
			firstSeparatedArgument = 1;
		}

		std::vector<std::wstring> targetArguments;
		for (size_t index = firstTargetArgument; index < positional.size(); ++index)
		{
			targetArguments.push_back(positional[index]);
		}

		for (size_t index = firstSeparatedArgument; index < separated.size(); ++index)
		{
			targetArguments.push_back(separated[index]);
		}

		if (!targetArguments.empty())
		{
			if (argumentsGiven)
			{
				result.Error = L"Arguments for the program were given twice: with --arguments and after it.";
				return result;
			}

			// Re-quoted rather than concatenated: the arguments arrive already split, and joining them
			// back with plain spaces would lose the ones that contained a space.
			options.TargetArguments = JoinArguments(targetArguments);
		}

		if (!targetGiven || options.TargetPath.empty())
		{
			result.Error = L"No program was named. Name it with --target, after \""
				+ std::wstring(ArgumentSeparator) + L"\", or as the second value.";
			return result;
		}

		result.Outcome = ParseSucceeded;
		result.Options = options;
		return result;
	}

	std::wstring BuildVersionText()
	{
		return std::wstring(ToolName) + L" " + ToolVersion;
	}

	std::wstring BuildUsageText()
	{
		std::wstring text;

		text += BuildVersionText() + L"\r\n\r\n";

		text += L"Starts a program without the application that asked for it staying resident: waits for\r\n";
		text += L"that application to close, starts the program, waits for it to end and starts the\r\n";
		text += L"application again. Nothing is shown while it runs - there is no window and no icon in\r\n";
		text += L"the notification area, and the wait costs no processor time.\r\n\r\n";

		text += L"Usage:\r\n";
		text += std::wstring(L"  ") + ToolName + L" [options] <application> <program> [<program arguments>...]\r\n";
		text += std::wstring(L"  ") + ToolName + L" [options] -- <program> [<program arguments>...]\r\n\r\n";

		text += L"Options:\r\n";
		text += L"  -t, --target <path>      Program to start once the application has closed.\r\n";
		text += L"  -a, --arguments <text>   Command line for the program, as one already formed string.\r\n";
		text += L"  -d, --working-dir <dir>  Directory to start the program in (default: its own folder).\r\n";
		text += L"      --host <path>        Program to start again when the program ends\r\n";
		text += L"                           (default: AI Helper.exe, found next to this tool).\r\n";
		text += L"      --host-arg <value>   One argument for that program; may be repeated.\r\n";
		text += L"      --host-pid <pid>     Process to wait for (default: nothing is waited for).\r\n";
		text += L"      --host-wait <sec>    Stop waiting for it after this long (default: no deadline).\r\n";
		text += L"      --no-restart         Do not start the application again.\r\n";
		text += L"  -h, --help               Show this text.\r\n";
		text += L"  -v, --version            Show the version.\r\n\r\n";

		text += L"Exit codes: 0 done, 1 unexpected error, 2 bad command line, 3 program not started,\r\n";
		text += L"            4 application not started again.\r\n";

		return text;
	}
}
