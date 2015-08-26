using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Haven.Core
{
    /// <summary>
    /// Class that inherits WebClient to be able to hold cookies
    /// </summary>
    [System.ComponentModel.DesignerCategory("Code")]
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
