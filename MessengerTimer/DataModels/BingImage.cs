using System.Net.Http;
using System.Threading.Tasks;

namespace MessengerTimer.DataModels
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
