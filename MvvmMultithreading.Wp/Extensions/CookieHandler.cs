using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Extensions
{
    using System.Net;

    /// <summary>
    /// Class for handling the cookies.
    /// </summary>
    public static class CookieHandler
    {
        /// <summary>
        /// Gets the cookies from the cookiecontainer.
        /// </summary>
        /// <returns>
        /// Cookies stored in the cookiecontainer.
        /// </returns>
        public static IEnumerable<Cookie> GetCookies(this Uri uri, CookieContainer cookieContainer)
        {
            var cookies = cookieContainer.GetCookies(uri);
            return cookies.Cast<Cookie>();
        }

        /// <summary>
        /// Gets specific cookie by key from the cookiecontainer.
        /// </summary>
        /// <returns>
        /// Value of cookie stored in the cookiecontainer.
        /// </returns>
        public static object GetCookieValue(this Uri uri, CookieContainer cookieContainer, string keyname)
        {
            var cookies = uri.GetCookies(cookieContainer);
            var cookie = cookies.FirstOrDefault(x => x.Name == keyname);
            if (cookie == null)
            {
                return null;
            }

            return cookie.Value;
        }
    }
}