using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIHelper {
    public class BingImage {
        static public string FetchUrlAsync() {
            using (var httpClient = new System.Net.Http.HttpClient()) {
                string raw = (httpClient.GetStringAsync("https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US")).Result;
                string r1 = raw.Substring(raw.IndexOf("\"url\"") + 7);
                return "https://www.bing.com" + r1.Substring(0, r1.IndexOf(".jpg") + 4);
            }
        }
    }
}
