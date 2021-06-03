namespace SteamAccountSwitcher.Utils
{
    public sealed class UserSettings
    {
        private readonly bool _globalSettings;
        private string _steamInstallDir;
        private bool _autostart;
         
        public UserSettings() : this(false){}

        public UserSettings(bool global)
        {
            _globalSettings = global;
            if (_globalSettings)
            {
                _autostart = Properties.Settings.Default.autostart;
                _steamInstallDir = Properties.Settings.Default.steamDir;
            }
        }

        public string SteamInstallDir
        {
            get => _steamInstallDir;
            set
            {
                _steamInstallDir = value;
                if (_globalSettings) Properties.Settings.Default.steamDir = value;
            }
        }

        public bool Autostart
        {
            get => _autostart;
            set
            {
                _autostart = value;
                if (_globalSettings) Properties.Settings.Default.autostart = value;
            }
        }

        private bool Equals(UserSettings otherUserSettings) => _steamInstallDir == otherUserSettings._steamInstallDir && _autostart == otherUserSettings._autostart;
      
        public UserSettings Copy()
        {
            var copySettings = new UserSettings
            {
                Autostart = Autostart,
                SteamInstallDir = SteamInstallDir
            };
            return copySettings;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserSettings)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_steamInstallDir != null ? _steamInstallDir.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _autostart.GetHashCode();
                return hashCode;
            }
        }
    }
}