using System;
using System.Windows.Forms;

namespace CameraAudioResumeFix
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(params string[] args)
        {
            if (args != null && args.Length > 0)
            {
                ProcessArguments(args);
                Application.Exit();
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void ProcessArguments(string[] args)
        {
            foreach (var arg in args)
            {
                switch (arg.ToLowerInvariant())
                {
                    case AudioManager.RunArgument:
                        AudioManager.OpenAndClose();
                        break;
                    case AudioManager.InstallArgument:
                        AudioManager.Install();
                        break;
                    case AudioManager.UninstallArgument:
                        AudioManager.Uninstall();
                        break;
                }
            }
        }
    }
}
