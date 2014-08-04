using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DictManiac.Sources
{
    class WebUtil
    {
        public static bool DownloadStringSynchronous(string url, out string result)
        {
            result = "";
            try
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add("User-Agent", "Chrome");
                result = webClient.DownloadString(url);
                return true;
            }
            catch// (Exception e)
            {
                //MyLogger.Attach(e.ToString());
                return false;
            }
        }

        public static bool DownloadBytesSynchronous(string url, out byte[] result)
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add("User-Agent", "Chrome");
                result = webClient.DownloadData(url);
                return true;
            }
            catch// (Exception e)
            {
                //MyLogger.Attach(e.ToString());
                result = null;
                return false;
            }
        }
    }
}
