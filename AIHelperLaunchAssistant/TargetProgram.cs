using System;
using System.IO;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// The program the assistant has to start once the host application has closed.
    /// </summary>
    /// <remarks>
    /// Immutable: it is built once from the command line and only read afterwards.
    /// </remarks>
    internal sealed class TargetProgram
    {
        /// <param name="path">Full or relative path of the program to start.</param>
        /// <param name="arguments">Command line for the program; empty when it takes none.</param>
        /// <param name="workingDirectory">
        /// Directory to start the program in, or <c>null</c> to start it in its own folder.
        /// </param>
        internal TargetProgram(string path, string arguments, string workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("The target program path is required.", nameof(path));
            }

            Path = path;
            Arguments = arguments ?? string.Empty;
            WorkingDirectory = workingDirectory;
        }

        /// <summary>Path of the program to start, exactly as it was given on the command line.</summary>
        internal string Path { get; }

        /// <summary>Command line handed to the program; empty when it takes none.</summary>
        internal string Arguments { get; }

        /// <summary>Directory to start the program in, or <c>null</c> for its own folder.</summary>
        internal string WorkingDirectory { get; }

        /// <summary>File name of the target, for messages and for the notification area tooltip.</summary>
        internal string DisplayName
        {
            get
            {
                string name = System.IO.Path.GetFileName(Path);
                return string.IsNullOrEmpty(name) ? Path : name;
            }
        }
    }
}
