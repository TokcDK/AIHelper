using System;
using System.Diagnostics;
using System.Threading;

namespace AIHelperLaunchAssistant
{
    /// <summary>Waits for processes to end.</summary>
    /// <remarks>
    /// An interface rather than a static helper because this is the part of the sequence that costs
    /// real time and the part the notification area interrupts: a test has to be able to drive it
    /// without a program to wait for.
    /// </remarks>
    internal interface IProcessWaiter
    {
        /// <summary>Waits for <paramref name="process"/> to end.</summary>
        /// <param name="timeout">
        /// How long to wait; <see cref="TimeSpan.Zero"/> waits without a deadline.
        /// </param>
        /// <param name="takeRequest">
        /// Polled while waiting. It returns the user's request once one is made and clears it, so the
        /// request is taken here rather than read afterwards and there is no moment between the two in
        /// which it could be lost.
        /// </param>
        /// <param name="request">
        /// The request that ended the wait, or <c>null</c> when the wait ended for any other reason.
        /// </param>
        /// <returns><c>true</c> when the process ended; <c>false</c> when the wait was interrupted or ran out of time.</returns>
        bool WaitForExit(Process process, TimeSpan timeout, Func<UserRequest?> takeRequest, out UserRequest? request);

        /// <summary>Waits until nothing matching <paramref name="target"/> is running.</summary>
        bool WaitForExit(ProcessTarget target, TimeSpan timeout, Func<UserRequest?> takeRequest, out UserRequest? request);

        /// <summary>Whether something matching <paramref name="target"/> is running right now.</summary>
        bool IsRunning(ProcessTarget target);
    }

    internal sealed class ProcessWaiter : IProcessWaiter
    {
        /// <summary>
        /// How long one wait may last before the request and the deadline are looked at again. A
        /// blocking wait is used rather than a spin, so this costs one system call per interval.
        /// </summary>
        private const int PollIntervalMilliseconds = 250;

        public bool WaitForExit(Process process, TimeSpan timeout, Func<UserRequest?> takeRequest, out UserRequest? request)
        {
            request = null;

            if (process == null)
            {
                return true;
            }

            DateTime deadline = ResolveDeadline(timeout);

            while (true)
            {
                request = TakeRequest(takeRequest);
                if (request.HasValue)
                {
                    return false;
                }

                int wait = RemainingMilliseconds(deadline);
                if (wait == 0)
                {
                    return false;
                }

                bool exited;
                try
                {
                    exited = process.WaitForExit(wait);
                }
                catch (InvalidOperationException)
                {
                    // No process is associated with the object, so there is nothing left to wait for.
                    return true;
                }

                if (exited)
                {
                    return true;
                }
            }
        }

        public bool WaitForExit(ProcessTarget target, TimeSpan timeout, Func<UserRequest?> takeRequest, out UserRequest? request)
        {
            request = null;

            if (target == null)
            {
                return true;
            }

            DateTime deadline = ResolveDeadline(timeout);

            while (true)
            {
                request = TakeRequest(takeRequest);
                if (request.HasValue)
                {
                    return false;
                }

                if (!IsRunning(target))
                {
                    return true;
                }

                int wait = RemainingMilliseconds(deadline);
                if (wait == 0)
                {
                    return false;
                }

                Thread.Sleep(wait);
            }
        }

        public bool IsRunning(ProcessTarget target)
        {
            if (target == null)
            {
                return false;
            }

            if (target.HasProcessId)
            {
                try
                {
                    using (Process process = Process.GetProcessById(target.ProcessId))
                    {
                        return target.ProcessName.Length == 0
                            || string.Equals(process.ProcessName, target.ProcessName, StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch (ArgumentException)
                {
                    // Nothing is using that id, so the process has ended.
                    return false;
                }
            }

            if (target.ProcessName.Length == 0)
            {
                return false;
            }

            // Every handle the snapshot hands back has to be released again: this runs several times a
            // second for the length of a game session, and leaked handles would accumulate.
            Process[] processes = Process.GetProcessesByName(target.ProcessName);
            try
            {
                return processes.Length > 0;
            }
            finally
            {
                foreach (Process process in processes)
                {
                    process.Dispose();
                }
            }
        }

        private static UserRequest? TakeRequest(Func<UserRequest?> takeRequest)
        {
            return takeRequest == null ? null : takeRequest();
        }

        private static DateTime ResolveDeadline(TimeSpan timeout)
        {
            return timeout <= TimeSpan.Zero ? DateTime.MaxValue : DateTime.UtcNow + timeout;
        }

        /// <summary>
        /// Milliseconds left before the deadline, never more than one poll interval, or <c>0</c> once
        /// the deadline has passed. <c>0</c> is therefore also the "no time left" answer.
        /// </summary>
        private static int RemainingMilliseconds(DateTime deadline)
        {
            if (deadline == DateTime.MaxValue)
            {
                return PollIntervalMilliseconds;
            }

            double remaining = (deadline - DateTime.UtcNow).TotalMilliseconds;
            if (remaining <= 0)
            {
                return 0;
            }

            return (int)Math.Min(remaining, PollIntervalMilliseconds);
        }
    }
}
