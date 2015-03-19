using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MameRomsDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            CookieCollection cookies = GetAuthenticatedCookies();

            foreach (Cookie cookie in cookies)
                Console.WriteLine(cookie.ToString());

            Console.ReadKey();
        }

        private static CookieCollection GetAuthenticatedCookies()
        {
            UriBuilder uriBuilder = new UriBuilder(Properties.Settings.Default.SiteAuthPage);

            HttpWebRequest request = WebRequest.CreateHttp(uriBuilder.ToString());
            request.Method = "POST";

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[Properties.Settings.Default.SiteAuthUsernameParam] = Properties.Settings.Default.Username;
            query[Properties.Settings.Default.SiteAuthPasswordParam] = Properties.Settings.Default.Password;
            string data = query.ToString() + "&" + Properties.Settings.Default.SiteAuthAction;

            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] byte1 = encoding.GetBytes(data);
            // Set the content type of the data being posted.
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the content length of the string being posted.
            request.ContentLength = byte1.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(byte1, 0, byte1.Length);

            request.KeepAlive = true;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.89 Safari/537.36";

            request.AllowAutoRedirect = false;
            request.CookieContainer = new CookieContainer();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // Display the status.
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            //Console.WriteLine(responseFromServer);

            //Get the cookies
            CookieCollection cookies = response.Cookies;

            // Clean up the streams and the response.
            reader.Close();
            response.Close();

            return cookies;
        }
    }
}
