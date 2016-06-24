using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Extensions
{
    using System.Net;
    using System.Security;

    public class ExtendedWebClient : WebClient
    {
        public CookieContainer CookieContainer { get; private set; }

        [SecuritySafeCritical]
        public ExtendedWebClient()
        {
            this.CookieContainer = new CookieContainer();
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = this.CookieContainer;
            }

            return request;

        }

    }
}