using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace MessengerTimer.DataModels
{
    public class BingImage
    {
        private static AppSettings appSettings = new AppSettings();

        private static async Task<string> FetchUrlAsync()
        {
            using (var httpClient = new HttpClient())
            {
                string raw = await httpClient.GetStringAsync(new Uri("https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US"));
                string r1 = raw.Substring(raw.IndexOf("\"url\"") + 7);
                return "https://www.bing.com" + r1.Substring(0, r1.IndexOf(".jpg") + 4);
            }
        }

        public static async Task<SoftwareBitmapSource> GetImageAsync()
        {
            DateTime now = DateTime.Now;

            var folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("images_cache", CreationCollisionOption.OpenIfExists);

            if (now.Subtract(appSettings.LastUpdateImageTime) > TimeSpan.FromDays(1))
            {
                string link;
                try
                {
                    link = await FetchUrlAsync();

                }
                catch (Exception)
                { return null; }

                try
                {
                    IInputStream inputStream = await GetStreamAsync(link);

                    var sb = await GetBitmapFromStreamAsync(inputStream);

                    await WriteToFileAsync(folder, sb, "Cache.jpg");

                    return await ConvertSoftwareBitmap(sb);
                }
                catch (Exception e)
                {
                    return null;
                }

            }
            else
            {
                StorageFile file = await folder.CreateFileAsync("Cache.jpg", CreationCollisionOption.OpenIfExists);

                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var sb = await GetBitmapFromStreamAsync(stream);

                    return await ConvertSoftwareBitmap(sb);
                }
            }
        }

        private async static Task<SoftwareBitmapSource> ConvertSoftwareBitmap(SoftwareBitmap sb)
        {
            appSettings.LastUpdateImageTime = DateTime.Now;
            var sbs = new SoftwareBitmapSource();
            await sbs.SetBitmapAsync(sb);
            return sbs;
        }

        private static async Task<SoftwareBitmap> GetBitmapFromStreamAsync(IRandomAccessStream stream)
        {
            IRandomAccessStream memStream = new InMemoryRandomAccessStream();
            await RandomAccessStream.CopyAsync(stream, memStream);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(memStream);
            return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        }

        private static async Task<SoftwareBitmap> GetBitmapFromStreamAsync(IInputStream stream)
        {
            IRandomAccessStream memStream = new InMemoryRandomAccessStream();
            await RandomAccessStream.CopyAsync(stream, memStream);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(memStream);
            return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        }

        private static async Task WriteToFileAsync(StorageFolder folder, SoftwareBitmap sb, string fileName)
        {

            if (sb != null)
            {
                // save image file to cache
                StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                    encoder.SetSoftwareBitmap(sb);
                    await encoder.FlushAsync();
                }
            }
        }


        private static async Task<IInputStream> GetStreamAsync(string url)
        {

            var httpClient = new HttpClient();
            var response = await httpClient.GetInputStreamAsync(new Uri(url));
            return response;
        }

    }
}
