using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Controls;

namespace SteamAccountSwitcher.Utils
{
    public class Steam
    {
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        public string InstallLocation { get; set; }

        public Steam(string installLocation) => InstallLocation = installLocation;

        public string InstallDir => InstallLocation.Replace("Steam.exe", "").Replace("steam.exe", "");

        public bool IsSteamRunning()
        {
            var steamProcesses = Process.GetProcessesByName("steam");
            return steamProcesses.Length > 0;
        }
        
        private SteamAccount loggedInAcc = null;
        private ListView AccountslistBox = null;
        private void KillSteam(Process[] Processes)
        {
            if (Processes.Length > 0)
            {
                var Instance = Processes[0];
                if(Instance != null) Instance.Kill();
            }
        }
        public Process StartSteamAccount(SteamAccount acc, Process? Instance = null, ListView? listBoxAccounts = null)
        {
            AccountslistBox = listBoxAccounts;
            KillSteam(Instance != null ? new Process[] { Instance } : Process.GetProcessesByName("steam"));
            Thread.Sleep(2000);
            while (!IsSteamRunning())
            { 
                var SteamProcess = new Process();
                if (File.Exists(InstallLocation))
                {
                    SteamProcess.StartInfo = new ProcessStartInfo(InstallLocation, acc.StartParameters());
                    SteamProcess.Disposed += Terminated;
                    SteamProcess.Exited += Terminated;
                    void Terminated(object sender, EventArgs events)
                    {
                        if (loggedInAcc == null) return;
                        loggedInAcc.BackgroundImage = Misc.GetImage("account", "jpg");
                        if(AccountslistBox != null) AccountslistBox.Items.Refresh();
                        loggedInAcc = null;
                    }
                    SteamProcess.Start();
                    loggedInAcc = acc;
                    return SteamProcess;
                } 
            }
            return null;
        } 
    
    }
}