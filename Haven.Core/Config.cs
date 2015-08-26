using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.Serialization;


namespace Haven.Core
{
    public static class Config
    { 
        public static Settings LoadYamlConfig(string path)
        {
            return new Deserializer().Deserialize<Settings>(File.OpenText(path));
        }
    }

    public struct Settings
    {
        [YamlMember(Alias = "save_location")]
        public string SaveLocation { get; set; }

        [YamlMember(Alias = "url")]
        public string Url { get; set; }

        [YamlMember(Alias = "pages")]
        public int Pages { get; set; }

        [YamlMember(Alias = "page_offset")]
        public int PageOffset { get; set; }

        [YamlMember(Alias = "use_min")]
        public bool UseMin { get; set; }

        [YamlMember(Alias = "min_height")]
        public int MinHeight { get; set; }

        [YamlMember(Alias = "min_width")]
        public int MinWidth { get; set; }

        [YamlMember(Alias = "use_auth")]
        public bool UseAuth { get; set; }

        [YamlMember(Alias = "username")]
        public string Username { get; set; }

        [YamlMember(Alias = "password")]
        public string Password { get; set; }
    }
}
