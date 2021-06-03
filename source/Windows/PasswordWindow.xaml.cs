using SteamAccountSwitcher.Utils;
using System.Windows;
using System.Windows.Controls;

namespace SteamAccountSwitcher
{
    public partial class PasswordWindow : Window
    { 
        private bool SetNewPw { get; set; }
        public string Password { get; private set; }
         
        public PasswordWindow(bool setNewPw)
        { 
            SetNewPw = setNewPw;
            InitializeComponent();
            passwordBox.Focus();
            if (SetNewPw)
            {
                PwWindow.Title = "Set new password";
                PwWindow.Height = 140;
                repeatPasswordPanel.Visibility = Visibility.Visible;
                Image.Source = Misc.GetImage("lock");
            }
            else
            {
                PwWindow.Title = "Decrypt accounts with password";
                PwWindow.Height = 120;
                repeatPasswordPanel.Visibility = Visibility.Collapsed;
                Image.Source = Misc.GetImage("unlock");
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password == passwordBoxRepeat.Password || !SetNewPw)
            {
                Password = passwordBox.Password;
                Close();
            }
            else MessageBox.Show("Passwords do not match. Try again.", "Passwords not matching", MessageBoxButton.OK,MessageBoxImage.Exclamation);
        }
        private void ButtonCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}