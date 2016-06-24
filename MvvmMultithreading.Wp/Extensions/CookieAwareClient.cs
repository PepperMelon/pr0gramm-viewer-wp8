using System;
using System.Diagnostics;
using System.Net;
using Pr0gramm.ViewModel;

public class CookieAwareClient : WebClient
{
    private readonly bool firstLogin;

    [System.Security.SecuritySafeCritical]
    public CookieAwareClient(bool firstLogin = false)
        : base()
    {
        this.firstLogin = firstLogin;
    }

    protected override WebRequest GetWebRequest(Uri address)
    {
        Debug.WriteLine("CookieAwareClient called: {0}", DateTime.Now);
        WebRequest request = base.GetWebRequest(address);
        if (request is HttpWebRequest)
        {
            var webRequest = (request as HttpWebRequest);

            var cookies = new CookieContainer();

            //if (request.Headers == null)
            //{
            //    request.Headers = new WebHeaderCollection();
            //}

            //webRequest.Accept = "application/json, text/javascript, */*; q=0.01";
            //webRequest.Headers["Accept-Encoding"] = "gzip, deflate";
            //webRequest.Headers["Accept-Language"] = "de,en-US;q=0.7,en;q=0.3";
            //webRequest.Headers["Cache-Control"] = "max-age=0";
            //webRequest.Headers["Connection"] = "keep-alive";
            //webRequest.Headers["Host"] = "pr0gramm.com";
            //webRequest.Headers["Referer"] = "http://pr0gramm.com/";
            //webRequest.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:32.0) Gecko/20100101 Firefox/36.0";
            //webRequest.Headers["X-Requested-With"] = "XMLHttpRequest";
            
            var loginDataMissing = string.IsNullOrEmpty(ViewModelLocator.Authentication.PP) ||
                                   string.IsNullOrEmpty(ViewModelLocator.Authentication.Me);

            if ((ViewModelLocator.Authentication.IsAuthenticated || firstLogin) && !loginDataMissing)
            {
                cookies.Add(address, new Cookie("pp", ViewModelLocator.Authentication.PP));
                cookies.Add(address, new Cookie("me", ViewModelLocator.Authentication.Me));
                request.Headers[HttpRequestHeader.IfModifiedSince] = DateTime.UtcNow.ToString("r");
            }
            else
            {
                cookies.Add(address, new Cookie("pp", "4f6c797a6597d32b2953ced9cc2b66b2"));
            }

            webRequest.CookieContainer = cookies;
        }

        return request;
    }

}