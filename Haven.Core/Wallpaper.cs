using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haven.Core
{
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
}
