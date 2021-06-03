using System;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging; 

namespace SteamAccountSwitcher.Utils
{
    public static class Misc
    {
        public static Embedded AssemblyFunctions = new Embedded(); 
        public static ImageSource GetImage(string name, string ext = "png") => new BitmapImage(new Uri($"../images/{name}.{ext}", UriKind.Relative));
        public static ImageSource GetIcon(string name, string ext = "png") => GetImage($"icons/{name}", ext);
        public static SolidColorBrush BrushColorFromRgb(byte r = 0, byte g = 0, byte b = 0) => new SolidColorBrush(Color.FromRgb(r, g, b));

        private static JObject GetAPIData(this string url)
        { 
            var wc = new WebClient() { Encoding = Encoding.UTF8 };
            var obj = JObject.Parse(wc.DownloadString(url));
            wc.Dispose();
            return obj; 
        }

        public static string GetSteamServiceStatus(string service, out SolidColorBrush solidColorBrush)
        {
            solidColorBrush = BrushColorFromRgb(146, 247, 181);
            try
            {  
                if ("https://mgpmn3kqj8.execute-api.us-west-2.amazonaws.com/prod/".GetAPIData()["services"][service].ToString().ToLower() == "normal") return "Steam Servers Online.";
                else throw new Exception("Steam servers are currently experiencing issues!");
            }
            catch (Exception ex)
            {
                solidColorBrush = BrushColorFromRgb(250, 165, 165);
                return ex.Message;
            }
        }
    }
}
