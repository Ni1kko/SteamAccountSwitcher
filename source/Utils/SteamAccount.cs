using System.Windows.Media;

namespace SteamAccountSwitcher.Utils
{
    public class SteamAccount
    {
        public string SteamId { get; set; }

        public string AccountName { get; set; }
        public ImageSource AccountImage { get; set; } = Misc.GetImage("key", "png");
        public ImageSource BackgroundImage { get; set; } = Misc.GetImage("account", "jpg");
        public string Password { get; set; }

        public SteamAccount(){}
        public SteamAccount(string accountName, string password)
        {
            AccountName = accountName;
            Password = password;
        }

        public string StartParameters() =>  $"-login {AccountName} {Password}"; 
        public override string ToString() => "(Username: " + AccountName + ")";
   
        protected bool Equals(SteamAccount other) => AccountName == other.AccountName && Password == other.Password && SteamId == other.SteamId;

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SteamAccount)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AccountName != null ? AccountName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AccountName != null ? AccountName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SteamId != null ? SteamId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}