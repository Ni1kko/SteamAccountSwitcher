using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Linq;
using SteamAccountSwitcher.Utils;

namespace SteamAccountSwitcher
{ 
    public partial class MainWindow : Window
    {
        private SteamAccount lastAcc = null;
        private Process Instance = null;
        public MainWindow()
        {
            InitializeComponent();

            void SetupWindow()
            {
                //Restore size
                Top = Properties.Settings.Default.Top;
                Left = Properties.Settings.Default.Left;
                Height = Properties.Settings.Default.Height;
                Width = Properties.Settings.Default.Width;
                if (Properties.Settings.Default.Maximized) WindowState = WindowState.Maximized;

                bool outOfBounds = (Left <= SystemParameters.VirtualScreenLeft - Width) ||(Top <= SystemParameters.VirtualScreenTop - Height) || (SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth <= Left) || (SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight <= Top);

                if (outOfBounds)
                {
                    Debug.WriteLine("Out of bounds window was reset to default offsets");
                    Left = 0;
                    Top = 0;
                    Width = 450;
                    Height = 400;
                }
            }

            SetupWindow();
            UpdateSteamLoginStatus();
            SasManager.Instance.InitializeAccountsFromFile();
            SasManager.Instance.SetSteamInstallDir(Properties.Settings.Default.steamDir);
         

            listBoxAccounts.ItemsSource = SasManager.Instance.AccountList;
            listBoxAccounts.Items.Refresh();
            if(listBoxAccounts.Items.Count > 0) lastAcc = (SteamAccount)listBoxAccounts.Items[0];

            Style itemContainerStyle = new Style(typeof(ListBoxItem));

            //take full width
            itemContainerStyle.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            listBoxAccounts.ItemContainerStyle = itemContainerStyle;
        }
        private void UpdateSteamLoginStatus()
        {
            SolidColorBrush BGColorBrush;
            statusBarLabel.Content = Misc.GetSteamServiceStatus("SessionsLogon", out BGColorBrush);
            statusbar.Background = BGColorBrush;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SasManager.Instance.SaveAccounts();

            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = Top;
                Properties.Settings.Default.Left = Left;
                Properties.Settings.Default.Height = Height;
                Properties.Settings.Default.Width = Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();
        }
        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            UpdateSteamLoginStatus();
            AccountWindow newAccWindow = new AccountWindow();
            newAccWindow.Owner = this;
            newAccWindow.ShowDialog();
            if (newAccWindow.Account != null) SasManager.Instance.AccountList.Add(newAccWindow.Account);
          
        }
        private void listBoxAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonEdit.IsEnabled = sender != null;
            listBoxAccounts.Items.Refresh();
        }
        private void listContextMenuRemove_Click(object sender, RoutedEventArgs e) => AskForDeletionOfAccount((SteamAccount)listBoxAccounts.SelectedItem);
        private void listContextMenuEdit_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxAccounts.SelectedItem == null) return;
            AccountWindow newAccWindow = new AccountWindow((SteamAccount)listBoxAccounts.SelectedItem);
            newAccWindow.Owner = this;
            newAccWindow.ShowDialog();
            listBoxAccounts.Items.Refresh();
        }
        private void buttonEdit_Click(object sender, RoutedEventArgs e) => listContextMenuEdit_Click(sender, e);
        private void steamAccount_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Did they really click?
            if (e.ClickCount < 1) return;

            //Update login server status bar
            UpdateSteamLoginStatus();

            //Double click only
            if (e.ClickCount < 2) return;

            //Selected account
            SteamAccount selectedAcc = (SteamAccount)listBoxAccounts.SelectedItem;
            
            //Update account background image
            if (lastAcc != null)
            {
                lastAcc.BackgroundImage = Misc.GetImage("account", "jpg");
                lastAcc = selectedAcc;
            }
            selectedAcc.BackgroundImage = Misc.GetImage("account-selected", "jpg");
            listBoxAccounts.Items.Refresh();
             
            //Launch steam
            Instance = SasManager.Instance.SteamInstallation.StartSteamAccount(selectedAcc, Instance, listBoxAccounts);
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            AskForDeletionOfAccount((SteamAccount)listBoxAccounts.SelectedItem);
        }
        private void AskForDeletionOfAccount(SteamAccount selectedAccount)
        {
            var result = MessageBox.Show("Are you sure you want to delete the account profile of " + selectedAccount.ToString() + "?","Deletion prompt",MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes) return; 
            SasManager.Instance.AccountList.Remove(selectedAccount);
            buttonEdit.IsEnabled = false; // Cannot edit deleted account
            listBoxAccounts.Items.Refresh();
        }
    }

    public sealed class SasManager
    {
        private UserSettings _globalSettings;
        private AccountLoader loader;

        private SasManager()
        {
            _globalSettings = new UserSettings(true);
            SteamInstallation = new Steam(GlobalSettings.SteamInstallDir); 
        }

        private static SasManager _instance = null;

        public static SasManager Instance
        {
            get
            {
                if (_instance == null) _instance = new SasManager();
                return _instance;
            }
        }

        public Steam SteamInstallation { get; private set; }

        public ObservableCollection<SteamAccount> AccountList { get; private set; } = new ObservableCollection<SteamAccount>();

        public UserSettings GlobalSettings
        {
            get => _globalSettings.Copy(); // Only give copies so no accidental global changes can be made, copy is non global
        }

        public void SetSteamInstallDir(string installDir)
        {
            if (string.IsNullOrEmpty(installDir) || string.IsNullOrWhiteSpace(installDir))
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Steam |steam.exe";
                dialog.InitialDirectory = @"C:\Program Files (x86)\Steam";
                dialog.Title = "Select your Steam Installation";
                installDir = (dialog.ShowDialog() == true) ? dialog.FileName : null;
                if (installDir == null)
                {
                    MessageBox.Show("You cannot use SteamAccountSwitcher without selecting your Steam.exe. Program will close now.", "Steam missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(1);
                }
            }

            if (installDir != null)
            {
                SteamInstallation = new Steam(installDir);
                _globalSettings.SteamInstallDir = installDir;
            }
        }

        public void InitializeAccountsFromFile()
        {
            loader = new AccountLoader();

            var accounts = loader.LoadAccounts();

            if (loader.AccountFileExists())
                AccountList = new ObservableCollection<SteamAccount>(accounts);
            else
                AccountList = new ObservableCollection<SteamAccount>();
        }
        public void SaveAccounts() => loader.SaveAccounts(AccountList.ToList());
    
        public void SetEncryption()
        {
            PasswordWindow passwordWindow = new PasswordWindow(true);
            passwordWindow.ShowDialog();
            var password = passwordWindow.Password;
            if (string.IsNullOrEmpty(password))
            {
                Debug.WriteLine("Will not change encryption to empty password");
                return;
            }

            loader.Password = password;
            loader.SaveAccounts(AccountList.ToList());
        }
    }
}