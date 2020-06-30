using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using CameraAudioResumeFix.Properties;
using Microsoft.Win32.TaskScheduler;

namespace CameraAudioResumeFix
{
    internal class AudioManager
    {
        public const string RunArgument = "/run";
        public const string InstallArgument = "/install";
        public const string UninstallArgument = "/uninstall";

        private const string TaskName = "FixCameraAudioAfterSleep";

        public static void OpenAndClose()
        {
            mciSendString("open new Type waveaudio Alias recsound", "", 0, 0);
            mciSendString("record recsound", "", 0, 0);
            Thread.Sleep(1500);
            var recordPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "record.wav");
            mciSendString("save recsound " + recordPath, "", 0, 0);
            mciSendString("close recsound ", "", 0, 0);
        }

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

        internal static string ApplicationFullPath => Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// Get current state of scheduler task installed.
        /// </summary>
        /// <returns></returns>
        public static TaskInstallStatus GetTaskStatus()
        {
            using (TaskService ts = new TaskService())
            {
                var tasks = ts.RootFolder.GetTasks(new System.Text.RegularExpressions.Regex(TaskName));
                if (tasks.Count == 0) return TaskInstallStatus.NotInstalled;
                if (tasks.Count > 1) return TaskInstallStatus.InstallInvalid;
                var task = tasks[0];
                if (!task.IsActive) return TaskInstallStatus.InstallInvalid;
                if (task.Definition.Actions.Count != 1) return TaskInstallStatus.InstallInvalid;
                if (!(task.Definition.Actions[0] is ExecAction action)) return TaskInstallStatus.InstallInvalid;
                if (!ApplicationFullPath.Equals(action.Path, StringComparison.OrdinalIgnoreCase))
                    return TaskInstallStatus.InstallInvalid;
                if (!RunArgument.Equals(action.Arguments, StringComparison.OrdinalIgnoreCase))
                    return TaskInstallStatus.InstallInvalid;
                return TaskInstallStatus.Installed;
            }
        }

        /// <summary>
        /// Install to system Scheduler task to run audio fix on any user logon.
        /// </summary>
        public static void Install()
        {
            Uninstall();
            using (var ts = new TaskService())
            {
                var td = ts.NewTask();
                td.RegistrationInfo.Description = Resources.TaskDescription;
                var trigger = new EventTrigger
                {
                    Subscription =
                        @"<QueryList><Query Id=""0"" Path=""System""><Select Path=""System"">*[System[Provider[@Name='Microsoft-Windows-Power-Troubleshooter'] and (EventID=1)]]</Select></Query></QueryList>"
                };
                td.Triggers.Add(trigger);
                td.Actions.Add(new ExecAction(ApplicationFullPath, RunArgument));
                ts.RootFolder.RegisterTaskDefinition(TaskName, td);
            }
        }

        /// <summary>
        /// Remove any tasks from system scheduler.
        /// </summary>
        public static void Uninstall()
        {
            using (TaskService ts = new TaskService())
            {
                ts.RootFolder.DeleteTask(TaskName, false);
            }
        }
    }
}
