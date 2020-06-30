using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using CameraAudioResumeFix.Properties;

namespace CameraAudioResumeFix
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            UpdateStatus();
            ThreadPool.QueueUserWorkItem(a => AudioManager.OpenAndClose());

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (HasAdministrativeRight)
                AudioManager.Install();
            else
                RunAsAdmin(AudioManager.InstallArgument);
            UpdateStatus();
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            if (HasAdministrativeRight)
                AudioManager.Uninstall();
            else
                RunAsAdmin(AudioManager.UninstallArgument);
            UpdateStatus();
        }
        
        private void UpdateStatus()
        {
            switch (AudioManager.GetTaskStatus())
            {
                case TaskInstallStatus.NotInstalled:
                    lblStatus.Text = Resources.TaskStatus_NotInstalled;
                    lblStatus.ForeColor = Color.Blue;
                    btnInstall.Text = Resources.Button_Install;
                    break;
                case TaskInstallStatus.Installed:
                    lblStatus.Text = Resources.TaskStatus_Installed;
                    lblStatus.ForeColor = Color.Green;
                    btnInstall.Text = Resources.Button_Reinstall;
                    break;
                case TaskInstallStatus.InstallInvalid:
                    lblStatus.Text = Resources.TaskStatus_Invalid;
                    lblStatus.ForeColor = Color.DarkRed;
                    btnInstall.Text = Resources.Button_Reinstall;
                    break;
            }
        }

        private void RunAsAdmin(string arguments)
        {
            if (HasAdministrativeRight) return;
            var processInfo = new ProcessStartInfo
            {
                Verb = "runas", FileName = Assembly.GetExecutingAssembly().Location,
                Arguments = arguments
            };

            try
            {
                var proc = Process.Start(processInfo);
                proc?.WaitForExit();
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.Install_Failed);
            }
        }

        private static bool HasAdministrativeRight
        {
            get
            {
                var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                bool hasAdministrativeRight = principal.IsInRole(WindowsBuiltInRole.Administrator);
                return hasAdministrativeRight;
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(a => AudioManager.OpenAndClose());
        }
    }
}
