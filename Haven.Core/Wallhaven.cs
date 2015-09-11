using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

using System.IO;
using System.Net;
using System.Linq;
using System.Collections.Concurrent;

using HtmlAgilityPack;

namespace Haven.Core
{ 
    /// <summary>
    /// Class for qeueing and downloading wallpapers from Haven.
    /// </summary>
    public class Wallhaven
    {
        private bool _downloading;
        private bool _requireLogin;
        private bool _checkWallpaperBounds;
        private int _clientUpperBound;

        private List<Wallpaper> _wallpapers;
        private Queue<Wallpaper> _queue;
        private Stopwatch _stopwatch;
        private BlockingCollection<WebClient> _clientQueue;

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

        public bool IsDownloading { get { return _downloading; } }
        public int GetThumbsPerPage { get { return (int)Math.Round((double)(_wallpapers.Count / Pages)); } }
        public int GetDownloadCount { get { return _wallpapers.Count - _queue.Count; } }
        public int GetWallpaperCount { get { return _wallpapers.Count; } }
        public Wallpaper[] GetWallpapers { get { return _wallpapers.ToArray(); } }

        public delegate void DownloadCompletedHandler(object sender, HavenEventArgs e);
        public event DownloadCompletedHandler CompleteHandler;

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

            _clientUpperBound = 4;
            _clientQueue = new BlockingCollection<WebClient>(_clientUpperBound);

            for(int i = 0; i < _clientUpperBound; i++)
            {
                var client = new WebClient();
                client.Proxy = null;

                client.DownloadFileCompleted += DownloadFileCompleted;
                _clientQueue.Add(client);
            }
        }

        public Wallhaven(string url, int pages, string username, string password, bool login)
            : this(url, pages)
        {
            _requireLogin = login;

            Username = username;
            Password = password;
        }

        public Wallhaven(string YamlConfigPath)
            : this(Config.LoadYamlConfig(YamlConfigPath))
        { }

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

            _requireLogin = settings.UseAuth;
            _checkWallpaperBounds = settings.UseMin;

            if ((String.IsNullOrWhiteSpace(Username)
                || String.IsNullOrWhiteSpace(Password)) && _requireLogin)
            {
                Console.WriteLine("Username and/or password cannot be null.");
                Console.WriteLine("[[ User authentication disabled ]]\n");

                _requireLogin = false;
            }
        }

        public void StartDownload()
        {
            Console.WriteLine("Started up..");

            if (!Directory.Exists(Savepath))
            {
                Console.WriteLine("Creating directory: " + Savepath);
                Directory.CreateDirectory(Savepath);
            }

            Console.WriteLine("Queueing wallpapers...");

            _downloading = true;
            _stopwatch.Start();

            Console.WriteLine("Queueing {0} page(s)..\n", Pages);
            for(int i = 1; i <= Pages; i++)
            {
                Console.WriteLine(" Page {0}..", i);
                QueueDownload(i + PageOffset);
            }

            _queue = new Queue<Wallpaper>();
            _wallpapers.ForEach(x => _queue.Enqueue(x));

            Console.WriteLine("\nQueued {0} wallpaper(s).", _queue.Count);
            Console.WriteLine("Done queueing.");
            Console.WriteLine("\nStarting download...");

            try
            {
                while (_queue.Any())
                    DownloadSingleWallpaper();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while downloading:\n  {0}", ex.Message);
            }

            Console.WriteLine("Download completed.");

            _stopwatch.Stop();
            _downloading = false;

            DownloadTime = (int)_stopwatch.Elapsed.TotalSeconds;

            if (CompleteHandler != null)
            {
                var args = new HavenEventArgs(DownloadTime, GetDownloadCount);
                CompleteHandler(this, args);
            }
        }

        private void DownloadSingleWallpaper()
        {
            var client = _clientQueue.Take();
            var wallpaper = _queue.Dequeue();

            string path = Savepath + string.Format("wallpaper-{0}.{1}",
                wallpaper.Id, wallpaper.Extension == Extension.Jpg ? "jpg" : "png");

            Console.WriteLine("[ID: {0}] Downloading: {1}", wallpaper.Id, wallpaper.Name);

            client.DownloadFileAsync(new Uri(wallpaper.Url), path,
                new ClientDownloadArgs(wallpaper.Url, path, client));
        }

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

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
                throw e.Error;

            if (e.Cancelled)
                return;

            var args = (ClientDownloadArgs)e.UserState;
            _clientQueue.Add(args.Client);
        }

        private void QueueDownload(int page)
        {


            var url = String.Format(URL + "&page={0}", page);
            var result = String.Empty;

            using (var client = new CookieClient())
            {
                client.Proxy = null;

                if (_requireLogin)
                {
                    Console.WriteLine(String.Format("[Page: {0}] Authentication required, authenticating...", page));

                    var authUrl = @"http://alpha.Haven.cc/auth/login";
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

                if (!_checkWallpaperBounds)
                {
                    _wallpapers.Add(wallpaper);
                }
                else if (_checkWallpaperBounds)
                {
                    if (wallpaper.Width >= MinimumWidth && wallpaper.Height >= MinimumHeight)
                        _wallpapers.Add(wallpaper);
                }
            }
        }
    }

    public class HavenEventArgs : EventArgs
    {
        public double DownloadTime { get; private set; }
        public int DownloadCount { get; private set; }

        public HavenEventArgs(double time, int count)
        {
            this.DownloadTime = time;
            this.DownloadCount = count;
        }
    }

    public class ClientDownloadArgs
    {
        public string Url { get; private set; }
        public string Filename { get; private set; }
        public WebClient Client { get; private set; }

        public ClientDownloadArgs(string url, string filename, WebClient client)
        {
            this.Url = url;
            this.Filename = filename;
            this.Client = client;
        }
    }
}
