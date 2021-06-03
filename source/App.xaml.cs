using SteamAccountSwitcher.Utils;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace SteamAccountSwitcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
   [Guid("9D80EAEC-61BF-44AA-AF68-2AA0FD468234")]
    public partial class App : Application
    {
        public App() => AppDomain.CurrentDomain.AssemblyResolve += Misc.AssemblyFunctions.AssemblyResolver; 
    }
}
