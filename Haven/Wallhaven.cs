using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using System.IO;
using System.Net;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Haven
{
    /// <summary>
    /// Struct to hold all the settings for the wallhaven class
    /// </summary>
    public struct Settings
    {
        public string SaveLocation { get; set; }
        public string Url { get; set; }
        public int Pages { get; set; }
        public int PageOffset { get; set; }
        public int MinHeight { get; set; }
        public int MinWidth { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Class for qeueing and downloading wallpapers from wallhaven.
    /// </summary>
    public class Wallhaven
    {
        private bool _downloading;
        private bool _requireLogin;

        private List<Wallpaper> _wallpapers;
        private Queue<Wallpaper> _qeue;
        private Stopwatch _stopwatch;

        public int Pages { get; private set; }
        public int PageOffset { get; private set; }
        public int Errors { get; private set; }
        public string URL { get; private set; }
        public double DownloadTime { get; private set; }

        public string Username { get; private set; }
        public string Password { get; private set; }
        public int MinimumWidth { get; set; }
        public int MinimumHeight { get; set; }
        public string Savepath { get; set; }
    
        public Action DownloadCompleteCallback { get; set; }

        public bool IsDownloading
        {
            get { return _downloading; }
        }

        public int GetThumbsPerPage
        {
            get { return (int)Math.Round((double)(_wallpapers.Count / Pages)); }
        }

        public int GetDownloadCount
        {
            get { return _wallpapers.Count - _qeue.Count; }
        }

        public int GetWallpaperCount
        {
            get { return _wallpapers.Count; }
        }

        public Wallpaper[] GetWallpapers
        {
            get { return _wallpapers.ToArray(); }

        }

        /// <summary>
        /// Initalize a Wallhaven class with the requested Url.
        /// </summary>
        public Wallhaven(string url, int pages)
        {
            URL = url;
            Pages = pages;
            PageOffset = 0;

            // Check to see if the site is still in alpha
            if (!url.Contains("alpha."))
                Console.WriteLine("[[ Please beware this verison may be outdated ]]");

            _stopwatch = new Stopwatch();
            _wallpapers = new List<Wallpaper>();
            _downloading = false;
            _requireLogin = false;
        }

        /// <summary>
        /// Initalize a Wallhaven class with the requested Url.
        /// Also set credentials used for authentication.
        /// </summary>
        public Wallhaven(string url, int pages, string username, string password, bool login)
            : this(url, pages)
        {
            _requireLogin = login;

            Username = username;
            Password = password;
        }

        /// <summary>
        /// Initalize a Wallhaven class with a settings config file
        /// </summary>
        public Wallhaven(string JSONconfigPath)
            : this(JsonConvert.DeserializeObject<Settings>(
                File.ReadAllText(JSONconfigPath)))
        { }

        /// <summary>
        /// Initalize the Wallhaven class with a Settings object
        /// </summary>
        public Wallhaven(Settings settings)
            : this(settings.Url, settings.Pages)
        {
            Savepath = settings.SaveLocation;
            MinimumWidth = settings.MinWidth;
            MinimumHeight = settings.MinHeight;

            Pages = settings.Pages;
            PageOffset = settings.PageOffset;

            Username = settings.Username;
            Password = settings.Password;

            if (!String.IsNullOrWhiteSpace(Username)
                && !String.IsNullOrWhiteSpace(Password))
                _requireLogin = true;
            else
                _requireLogin = false;
        }

        /// <summary>
        /// Downloads wallpapers using the request Url.
        /// </summary>
        public void StartDownload()
        {
            _downloading = true;
            _stopwatch.Start();

            if (Pages > 1)
                for (int i = 1; i <= Pages; i++)
                    QeueDownload(i + PageOffset);
            else
                QeueDownload(Pages + PageOffset);

            _qeue = new Queue<Wallpaper>();
            foreach (var wall in _wallpapers)
                _qeue.Enqueue(wall);

            DownloadWallpapers();
        }

        /// <summary>
        /// Test to see if the qeue is empty, if it is the
        /// callback is invoked and DownloadTime is set.
        /// </summary>
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

            using (var client = new WebClient())
            {
                client.Proxy = null;

                try
                {
                    client.DownloadFileCompleted += DownloadFileCompleted;
                    await client.DownloadFileTaskAsync(uri, path);
                }
                catch (WebException ex)
                {
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
        private static bool UrlExist(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "head";

                if (request == null)
                    return false;

                using (var response = (HttpWebResponse)request.GetResponse())
                    return response.StatusCode == HttpStatusCode.OK;
            }
            catch (WebException)
            {
                return false;
            }
        }

        /// <summary>
        /// Initalizes a new download from the qeue after one was complete.
        /// </summary>
        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
                throw e.Error;

            if (e.Cancelled)
                return;

            DownloadWallpapers();
        }

        /// <summary>
        /// Qeues all the requested wallpapers for downloading.
        /// </summary>
        private void QeueDownload(int page)
        {
            var url = String.Format(URL + "&page={0}", page);
            var result = String.Empty;

            using (var client = new CookieClient())
            {
                client.Proxy = null;

                if (_requireLogin)
                {
                    Console.WriteLine(String.Format("[Page: {0}] Authentication required, authenticating...", page));

                    var authUrl = @"http://alpha.wallhaven.cc/auth/login";
                    var loginPage = client.DownloadString(authUrl);
                    var document = new HtmlDocument();

                    document.LoadHtml(loginPage);

                    var tokenNode = document.DocumentNode.SelectSingleNode("//input[@type=\"hidden\" and @name=\"_token\"]");
                    var randomToken = tokenNode.Attributes["value"].Value;

                    var credentials = new NameValueCollection
                    {
                        { "_token", randomToken },
                        { "username", Username },
                        { "password", Password }
                    };

                    client.UploadValues(authUrl, "post", credentials);

                    Console.WriteLine(String.Format("[Page: {0}] Authentication successfull", page) + Environment.NewLine);
                }

                result = client.DownloadString(url);
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(result);

            HtmlNodeCollection listItems = doc.DocumentNode.SelectNodes("//section/ul/li/figure/a");

            foreach (HtmlNode node in listItems) 
            {
                var link = node.Attributes["href"].Value;
                var id = Convert.ToInt32(link.Split('/').Last());

                var resNode = node.SelectSingleNode(String.Format("//section/ul/li/figure/div/span", id));
                var resolution = resNode.InnerHtml.Trim().Split('x');

                int width = int.Parse(resolution[0].Trim());
                int height = int.Parse(resolution[1].Trim());

                var wallpaper = new Wallpaper(id, width, height);

                if (!UrlExist(wallpaper.Url))
                    wallpaper.Extension = Extension.Png;

                if (wallpaper.Width >= MinimumWidth 
                    && wallpaper.Height >= MinimumHeight)
                    _wallpapers.Add(wallpaper);
            }
        }
    }

    public enum Extension
    {
        Jpg,
        Png
    }

    public enum Purity
    {
        SFW,
        Sketchy,
        NSFW
    }

    /// <summary>
    /// Holds the information of a wallpaper.
    /// </summary>
    public class Wallpaper
    {
        private const string _downloadUrl = @"http://wallpapers.wallhaven.cc/wallpapers/full/wallhaven-{0}.{1}";

        public Extension Extension { get; set; }
        public int Id { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Purity Purity
        {
            get { throw new NotImplementedException(); }
        }

        public string Url
        {
            get { return String.Format(_downloadUrl, Id, Extension == Extension.Jpg ? "jpg" : "png"); }
        }

        public string Name
        {
            get { return String.Format("wallpaper-{0}", Id); }
        }

        public Wallpaper(int id)
        {
            this.Id = id;
            this.Extension = Extension.Jpg;
        }

        public Wallpaper(int id, int width, int height)
            : this(id)
        {
            this.Width = width;
            this.Height = height;
        }
    }

    /// <summary>
    /// Class that inherits WebClient to be able to hold cookies
    /// </summary>
    public class CookieClient : WebClient
    {
        private CookieContainer _cookie = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest req = (HttpWebRequest)base.GetWebRequest(address);
            req.ProtocolVersion = HttpVersion.Version10;

            if (req is HttpWebRequest)
                (req as HttpWebRequest).CookieContainer = _cookie;

            return req;
        }
    }
}