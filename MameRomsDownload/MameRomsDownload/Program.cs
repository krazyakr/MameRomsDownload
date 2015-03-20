using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MameRomsDownload
{
    class Program
    {
        static List<string> files2Download = new List<string>();
        static int maxFiles2Download = 5;

        static void Main(string[] args)
        {
            Console.WriteLine("Authenticating ... ");
            CookieCollection cookies = GetAuthenticatedCookies();
            Console.WriteLine("Done.");

            //foreach (Cookie cookie in cookies)
            //    Console.WriteLine(cookie.ToString());

            Console.WriteLine("Getting list of files ... ");
            string basePage = GetPage(Properties.Settings.Default.SiteRomsBasePath, cookies);

            ScrapePage(cookies, basePage);
            Console.WriteLine("Done.");

            Console.WriteLine(string.Format("Found {0} files to download.", files2Download.Count));

            Console.ReadKey();
        }

        private static void ScrapePage(CookieCollection cookies, string basePage)
        {
            if (files2Download.Count < maxFiles2Download)
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(basePage);

                ScrapeList(cookies, htmlDoc);

                ScrapeDownloadLink(htmlDoc);
            }
        }

        private static void ScrapeDownloadLink(HtmlDocument htmlDoc)
        {
            var ahrefs = htmlDoc.DocumentNode.Descendants("a").Where(x => x.InnerText.Contains("Download"));

            if (ahrefs.Count() == 1)
                files2Download.Add(ahrefs.First<HtmlNode>().Attributes["href"].Value);
        }

        private static void ScrapeList(CookieCollection cookies, HtmlDocument htmlDoc)
        {
            HtmlNode table = htmlDoc.GetElementbyId("dir_content");

            if (table != null)
            {
                foreach (HtmlNode row in table.ChildNodes)
                {
                    HtmlNode link = null;
                    var childs = row.Descendants("a");
                    if (childs.Count() > 0)
                    {
                        link = childs.First();
                        Uri url = new Uri(Properties.Settings.Default.SiteRomsBasePath);
                        string tmpPath = url.AbsoluteUri.Substring(0, url.AbsoluteUri.IndexOf("?"));
                        tmpPath = tmpPath.Substring(0, tmpPath.LastIndexOf("/") + 1);
                        string childPageLink = tmpPath + link.Attributes["href"].Value;

                        //Console.WriteLine(link.InnerText + " =>> " + childPageLink);

                        string page = GetPage(childPageLink, cookies);

                        ScrapePage(cookies, page);
                    }
                }
            }
        }

        private static string GetPage(string url, CookieCollection cookies)
        {
            Thread.Sleep(500);

            HttpWebRequest request = WebRequest.CreateHttp(url);

            request.KeepAlive = true;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.89 Safari/537.36";

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(cookies);

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

            string page = responseFromServer;

            // Clean up the streams and the response.
            reader.Close();
            response.Close();
            return page;
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
