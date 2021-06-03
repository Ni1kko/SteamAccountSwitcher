using Newtonsoft.Json;
using SteamAccountSwitcher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows;

namespace SteamAccountSwitcher
{
    public partial class AccountWindow : Window
    {
        public SteamAccount Account { get; private set; }
        public AccountWindow() => InitializeComponent();
        public AccountWindow(SteamAccount accToEdit)
        {
            InitializeComponent();
            Title = "Edit Account";
            Account = accToEdit ?? throw new ArgumentNullException();
            textBoxUsername.Text = Account.AccountName;
            textBoxPassword.Password = Account.Password;
            textBoxUsername.IsEnabled = string.IsNullOrEmpty(Account.Password);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;
            if (Account == null) Account = new SteamAccount(textBoxUsername.Text, textBoxPassword.Password);
            else
            {
                Account.AccountName = textBoxUsername.Text;
                Account.Password = textBoxPassword.Password;
            };
            Close();
        }
        private bool ValidateInput()
        {
            string errorstring = "";
            if (string.IsNullOrEmpty(textBoxUsername.Text)) errorstring += "Username cannot be empty!\n";
            if (string.IsNullOrEmpty(textBoxPassword.Password)) errorstring += "Password cannot be empty!\n";
            if (errorstring == "") return true;
            MessageBox.Show(errorstring, "Validation problem", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Account = null;
            Close();
        }
    }

    public sealed class AccountLoader
    {
        private readonly string _directory;

        public AccountLoader() => _directory = AppDomain.CurrentDomain.BaseDirectory;

        public string Password { get; set; }

        public string AccountsFilePath => Path.Combine(_directory, "accounts.ini");

        public List<SteamAccount> LoadAccounts()
        {
            bool retry = true;
            while (retry)
            {
                string encryptionKey;
                if (!string.IsNullOrEmpty(Password))
                {
                    encryptionKey = Password;
                }
                else
                {
                    Password = AskForPassword(!AccountFileExists());
                    encryptionKey = Password;
                }

                try
                {
                    string encrypted = File.ReadAllText(this._directory + "accounts.ini");
                    string decrypted = EncryptionHelper.Decrypt(encrypted, encryptionKey);
                    List<SteamAccount> accountList = JsonConvert.DeserializeObject<List<SteamAccount>>(decrypted);
                    return accountList;
                }
                catch (CryptographicException e)
                {
                    MessageBox.Show("Try entering the password again.", "Could not decrypt");
                    Password = null;
                }
                catch (JsonException e)
                {
                    MessageBox.Show(e.Message, "Fatal Error when reading accounts file!");
                    retry = false;
                }
                catch (Exception e)
                {
                    retry = false;
                }
            }

            return null;
        }

        private string AskForPassword(bool isNew = false)
        {
            PasswordWindow passwordWindow = new PasswordWindow(isNew);
            passwordWindow.ShowDialog();
            if (passwordWindow.Password == null)
            {
                Environment.Exit(1);
            }

            return passwordWindow.Password;
        }

        public void SaveAccounts(List<SteamAccount> list)
        {
            string encryptionKey = Password;

            string output = JsonConvert.SerializeObject(list, Formatting.None);
            string encrypted = EncryptionHelper.Encrypt(output, encryptionKey);

            File.WriteAllText(AccountsFilePath, encrypted);
        }

        public bool AccountFileExists() =>File.Exists(AccountsFilePath);
    }
}