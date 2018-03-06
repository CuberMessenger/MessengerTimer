using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MessengerTimer.Models
{
    public class BingImage
    {
        static public async Task<string> FetchUrlAsync()
        {
            using (var httpClient = new HttpClient())
            {
                string raw = await httpClient.GetStringAsync("https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US");
                string r1 = raw.Substring(raw.IndexOf("\"url\"") + 7);
                return "https://www.bing.com" + r1.Substring(0, r1.IndexOf(".jpg") + 4);
            }
        }
    }
}
