using System;
using System.Net;

namespace FlickrNet.WebClientEx
{
    /// <summary>
    /// WebClient s moznosti nastavedni poctu soubeznych spojeni, Expect100Continue atd - vyrazne zrychly paralelni beh nad flickrem
    /// 
    /// viz: Configuring WebClient Timeout and ConnectionLimit, viz http://www.tomdupont.net/2012/05/configuring-webclient-timeout-and.html
    /// </summary>
    public class WebClientEx : WebClient
    {
        public int? Timeout { get; set; }

        public int? ConnectionLimit { get; set; }

        public bool? Expect100Continue { get; set; }


        protected override WebRequest GetWebRequest(Uri address)
        {
            var baseRequest = base.GetWebRequest(address);
            var webRequest = baseRequest as HttpWebRequest;
            if (webRequest == null)
            {
                return baseRequest;
            }

            if (Timeout.HasValue)
            {
                webRequest.Timeout = Timeout.Value;
            }
            if (ConnectionLimit.HasValue)
            {
                webRequest.ServicePoint.ConnectionLimit = ConnectionLimit.Value;
            }
            if (Expect100Continue.HasValue)
            {
                webRequest.ServicePoint.Expect100Continue = Expect100Continue.Value;
            }
            return webRequest;
        }
    }

}
