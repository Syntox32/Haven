using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using System.Net;
using HtmlAgilityPack;

namespace Haven
{
    /// <summary>
    /// Class for qeueing and downloading wallpapers from wallhaven.
    /// </summary>
    public class Wallhaven
    {
        private const string _pagePostfix = "&page={0}";

        private string _result;
        private bool _downloading;

        private List<Wallpaper> _wallpapers;
        private Queue<Wallpaper> _qeue;
        private Stopwatch _stopwatch;

        /// <summary>
        /// Gets the number of pages qeued for download.
        /// </summary>
        public int Pages { get; private set; }

        /// <summary>
        /// Gets the page offset.
        /// </summary>
        public int PageOffset { get; private set; }

        /// <summary>
        /// Gets the number of errors during the download process.
        /// </summary>
        public int Errors { get; private set; }

        /// <summary>
        /// Gets the request Url.
        /// </summary>
        public string URL { get; private set; }

        /// <summary>
        /// Gets the time it took to download all the qeued wallpapers.
        /// </summary>
        public double DownloadTime { get; private set; }

        /// <summary>
        /// Returns a boolean representing the download state.
        /// </summary>
        public bool IsDownloading { get { return _downloading; } }

        /// <summary>
        /// Gets the number of wallpapers per page.
        /// </summary>
        public int GetThumbsPerPage { get { return (int)Math.Round((double)(_wallpapers.Count / Pages)); } }

        /// <summary>
        /// Gets the number of downloaded wallpapers.
        /// </summary>
        public int GetDownloadCount { get { return _wallpapers.Count - _qeue.Count; } }

        /// <summary>
        /// Returns the wallpaper count.
        /// </summary>
        public int GetWallpaperCount { get { return _wallpapers.Count; } }

        /// <summary>
        /// Return all the wllpapers in an array.
        /// </summary>
        public Wallpaper[] GetWallpapers { get { return _wallpapers.ToArray(); } }

        /// <summary>
        /// Gets and sets the minimum wallpaper width.
        /// </summary>
        public int MinimumWidth { get; set; }

        /// <summary>
        /// Gets and sets the minimum wallpaper height.
        /// </summary>
        public int MinimumHeight { get; set; }

        /// <summary>
        /// Gets and sets the path where the wallpapers are saved.
        /// </summary>
        public string Savepath { get; set; }

        /// <summary>
        /// Gets and sets the download complete callback.
        /// </summary>
        public Action DownloadCompleteCallback { get; set; }

        /// <summary>
        /// Initalize a Wallhaven class with the requested Url.
        /// </summary>
        /// <param name="url">Request Url</param>
        public Wallhaven(string url)
        {
            URL = url;

            // Check to see if the site is still in alpha
            if (!url.Contains("alpha."))
                Console.WriteLine("[[ Please beware this verison may be outdated ]]");

            _stopwatch = new Stopwatch();
            _wallpapers = new List<Wallpaper>();
            _downloading = false;
        }

        /// <summary>
        /// Downloads wallpapers using the request Url.
        /// </summary>
        /// <param name="pages">Number of pages to download</param>
        /// <param name="offset">Page offset</param>
        public void StartDownload(int pages = 1, int offset = 0)
        {
            Pages = pages;
            PageOffset = 0;

            _downloading = true;
            _stopwatch.Start();

            if (pages > 1)
                for (int i = 1; i <= pages; i++)
                    QeueDownload(i + offset);
            else
                QeueDownload(pages + offset);

            _qeue = new Queue<Wallpaper>();
            foreach (var wall in _wallpapers)
                _qeue.Enqueue(wall);

            DownloadWallpapers();
        }

        /// <summary>
        /// Test to see if the qeue is empty, if it is the
        /// callback is invoked and DownloadTime is set.
        /// </summary>
        /// <returns></returns>
        private bool ShouldStop()
        {
            if (!_qeue.Any()) {
                _stopwatch.Stop();
                _downloading = false;

                DownloadTime = (int)_stopwatch.Elapsed.TotalSeconds;

                if (DownloadCompleteCallback != null)
                    DownloadCompleteCallback.Invoke();

                return true;
            }
            return false;
        }

        /// <summary>
        /// Asynchronously downloads a new wallpaper in the qeue for each call.
        /// If the qeue is empty the method aborts.
        /// </summary>
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

        /// <summary>
        /// Checks to see if a remote URL is valid/exists.
        /// </summary>
        /// <param name="url">Url to check</param>
        /// <returns>A bool representing the existence of the Url</returns>
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

        /// <summary>
        /// Initalizes a new download from the qeue after one was complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null) throw e.Error;
            if (e.Cancelled) return;

            DownloadWallpapers();
        }

        /// <summary>
        /// Qeues all the requested wallpapers for downloading.
        /// </summary>
        /// <param name="page">Page at the requested Url</param>
        private void QeueDownload(int page)
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

                var resNode = node.SelectSingleNode(String.Format("//*[@id=\"thumb-{0}\"]/div/span[1]", id));
                var resolution = resNode.InnerHtml.Trim().Split('x');

                int width = int.Parse(resolution[0].Trim());
                int height = int.Parse(resolution[1].Trim());

                var wallpaper = new Wallpaper(id, width, height);

                if (!UrlExist(wallpaper.Url))
                    wallpaper.Extension = Extension.Png;

                _wallpapers.Add(wallpaper);
            }
        }
    }

    public enum Extension { Jpg, Png }
    public enum Purity { SFW, Sketchy, NSFW }

    /// <summary>
    /// Holds the information of a wallpaper.
    /// </summary>
    public class Wallpaper
    {
        private const string _downloadUrl = @"http://alpha.wallhaven.cc/wallpapers/full/wallhaven-{0}.{1}";

        /// <summary>
        /// Returns what kind of extension the wallpaper has.
        /// </summary>
        public Extension Extension { get; set; }

        /// <summary>
        /// Gets the purity of the wallpaper.
        /// </summary>
        public Purity Purity { get { throw new NotImplementedException(); } }

        /// <summary>
        /// Returns the Id of the wallpaper.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Returns the wallpaper width.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Returns the wallpaper heigth.
        /// </summary>
        public int Heigth { get; private set; }

        /// <summary>
        /// Returns the download Url for the wallpaper.
        /// </summary>
        public string Url { get { return String.Format(_downloadUrl, Id, Extension == Extension.Jpg ? "jpg" : "png"); } }

        /// <summary>
        /// Returns a default name.
        /// </summary>
        public string Name { get { return String.Format("wallpaper-{0}", Id); } }

        /// <summary>
        /// Initialize a wallpaper
        /// </summary>
        /// <param name="id">Wallpaper Id</param>
        public Wallpaper(int id)
        {
            this.Id = id;
            this.Extension = Extension.Jpg;
        }

        /// <summary>
        /// Initialize a wallpaper
        /// </summary>
        /// <param name="id">Wallpaper Id</param>
        /// <param name="width">Width of the wallpaper</param>
        /// <param name="height">Height of the wallpaper</param>
        public Wallpaper(int id, int width, int height)
            : this(id)
        {
            this.Width = width;
            this.Heigth = height;
        }
    }
}