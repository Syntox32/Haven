using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using System.Net;
using HtmlAgilityPack;

namespace Haven
{
    public class Wallhaven
    {
        private const string _pagePostfix = "&page={0}";

        private Action _callback;
        private string _result;
        private bool _downloading;

        private List<Wallpaper> _wallpapers;
        private Queue<Wallpaper> _qeue;
        private Stopwatch _stopwatch;

        public int Pages { get; private set; }
        public int PageOffset { get; private set; }
        public int Errors { get; private set; }
        public string URL { get; private set; }
        public string Savepath { get; private set; }
        public double DownloadTime { get; private set; }

        public bool IsDownloading { get { return _downloading; } }
        public int GetThumbsPerPage { get { return (int)Math.Round((double)(_wallpapers.Count / Pages)); } }
        public int GetDownloadCount { get { return _wallpapers.Count - _qeue.Count; } }
        public int GetWallpaperCount { get { return _wallpapers.Count; } }
        public Wallpaper[] GetWallpapers { get { return _wallpapers.ToArray(); } }

        public Wallhaven(string url)
        {
            URL = url;

            if (!url.Contains("alpha."))
                Console.WriteLine("[[ Please beware this verison may be outdated ]]");

            _stopwatch = new Stopwatch();
            _wallpapers = new List<Wallpaper>();
            _downloading = false;
        }


        public void StartDownload(string savepath = "", int pages = 1, int offset = 0, Action downloadCompletedCallback = null)
        {
            Savepath = savepath;
            Pages = pages;
            PageOffset = 0;

            if (downloadCompletedCallback != null) {
                _callback = downloadCompletedCallback;
            }

            _downloading = true;
            _stopwatch.Start();

            if (pages > 1)
                for (int i = 1; i <= pages; i++)
                    QeueDownload(savepath, i + offset);
            else
                QeueDownload(savepath, pages + offset);

            _qeue = new Queue<Wallpaper>();
            foreach (var wall in _wallpapers)
                _qeue.Enqueue(wall);

            DownloadWallpapers();
        }

        private bool ShouldStop()
        {
            if (!_qeue.Any()) {
                _stopwatch.Stop();
                _downloading = false;

                DownloadTime = (int)_stopwatch.Elapsed.TotalSeconds;

                if (_callback != null)
                    _callback.Invoke();

                return true;
            }
            return false;
        }

        private async void DownloadWallpapers()
        {
            if (ShouldStop()) return;

            var wallpaper = _qeue.Dequeue();
            var uri = new Uri(wallpaper.Url);

            string path = Savepath + String.Format("wallpaper-{0}.{1}",
                wallpaper.Id, wallpaper.Extension == Extension.Jpg ? "jpg" : "png");

            Console.WriteLine("[ID: {0}] Downloading: {1}", wallpaper.Id, wallpaper.Name);

            using (var client = new WebClient()) {
                client.Proxy = null;

                try {
                    client.DownloadFileCompleted += DownloadFileCompleted;
                    await client.DownloadFileTaskAsync(uri, path);
                }
                catch (WebException ex) {
                    var response = (HttpWebResponse)ex.Response;
                    Console.WriteLine("[ID: {0}] Error: Download failed [Status: {1}]", 
                        wallpaper.Id, (int)response.StatusCode);

                    Errors++;
                }
            }
        }

        private static bool UrlExist(string url)
        {
            try {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "head";

                if (request == null) return false;

                using (var response = (HttpWebResponse)request.GetResponse())
                    return response.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException) {
                return false;
            }
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null) throw e.Error;
            if (e.Cancelled) return;

            DownloadWallpapers();
        }

        private void QeueDownload(string savepath, int page)
        {
            var url = String.Format(URL + _pagePostfix, page);

            using (var client = new WebClient()) {
                client.Proxy = null;
                _result = client.DownloadString(url);
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(_result);

            HtmlNodeCollection listItems = doc.DocumentNode.SelectNodes("//li/@id");
            foreach (HtmlNode node in listItems) {
                var id = Convert.ToInt32(node.Id.Substring(6, node.Id.Length - 6));
                var wallpaper = new Wallpaper(id);

                if (!UrlExist(wallpaper.Url))
                    wallpaper.Extension = Extension.Png;

                _wallpapers.Add(wallpaper);
            }
        }
    }

    public enum Extension { Jpg, Png }
    public enum Purity { SFW, Sketchy, NSFW }

    public class Wallpaper
    {
        private const string _downloadUrl = @"http://alpha.wallhaven.cc/wallpapers/full/wallhaven-{0}.{1}";

        public Extension Extension { get; set; }
        public Purity Purity { get { throw new NotImplementedException(); } }

        public int Id { get; private set; }
        public int Width { get { throw new NotImplementedException(); } }
        public int Heigth { get { throw new NotImplementedException(); } }

        public string Url { get { return String.Format(_downloadUrl, Id, Extension == Extension.Jpg ? "jpg" : "png"); } }
        public string Name { get { return String.Format("wallpaper-{0}", Id); } }

        public Wallpaper(int id)
        {
            this.Id = id;
            this.Extension = Extension.Jpg;
        }
    }
}