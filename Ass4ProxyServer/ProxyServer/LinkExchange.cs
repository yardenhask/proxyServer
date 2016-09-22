using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProxyServer
{
    class LinkExchange
    {
        string originalUrl = "";
        string twitterAddition = "";

        /// <summary>
        /// Check if the url ends with a file type ends.
        /// </summary>
        /// <param name="url">The url to be checked.</param>
        /// <returns>boolean according to the result</returns>
        public bool checkEndOfLinkContainsEndy(string url)
        {
            if (((url.EndsWith(".html") || url.EndsWith(".aspx") || url.EndsWith(".torrent") || url.EndsWith(".eml")
               || url.EndsWith(".json") || url.EndsWith(".vmsg") || url.EndsWith(".download") || url.EndsWith(".png") || url.EndsWith(".jsp") || url.EndsWith(".swf")
               || url.EndsWith(".opdownload") || url.EndsWith(".xap") || url.EndsWith(".website") || url.EndsWith(".htm") || url.EndsWith(".xml")
               || url.EndsWith(".js") || url.EndsWith(".asp") || url.EndsWith(".rdf") || url.EndsWith(".ucf") || url.EndsWith(".xsl")
               || url.EndsWith(".xsd") || url.EndsWith(".cgi") || url.EndsWith(".pfg") || url.EndsWith(".rss")
               || url.EndsWith(".css") || url.EndsWith(".xhtml") || url.EndsWith(".php") || url.EndsWith(".php5")
               || url.EndsWith(".page") || url.EndsWith(".php4") || url.EndsWith(".hdml") || url.EndsWith(".svg"))))
                return true;
            return false;
        }

        /// <summary>
        /// Check if the url contains a file type ends.
        /// </summary>
        /// <param name="url">The url to be checked.</param>
        /// <returns>boolean according to the result.</returns>
        public bool checkMiddleOfLinkContainsEndy(string url)
        {
            if (((url.Contains(".html") || url.Contains(".aspx") || url.Contains(".torrent") || url.Contains(".eml")
               || url.Contains(".json") || url.Contains(".vmsg") || url.Contains(".download") || url.Contains(".png") || url.Contains(".jsp") || url.Contains(".swf")
               || url.Contains(".opdownload") || url.Contains(".xap") || url.Contains(".website") || url.Contains(".htm") || url.Contains(".xml")
               || url.Contains(".js") || url.Contains(".asp") || url.Contains(".rdf") || url.Contains(".ucf") || url.Contains(".xsl")
               || url.Contains(".xsd") || url.Contains(".cgi") || url.Contains(".pfg") || url.Contains(".rss")
               || url.Contains(".css") || url.Contains(".xhtml") || url.Contains(".php") || url.Contains(".php5")
               || url.Contains(".page") || url.Contains(".php4") || url.Contains(".hdml") || url.Contains(".svg"))))
                return true;
            return false;
        }

        /// <summary>
        /// The function which takes the source code and switch the links.
        /// </summary>
        /// <param name="sourceCode">The original source code of the html.</param>
        /// <param name="url">The url to be checked.</param>
        /// <returns>The html source code after link switch.</returns>
        public string linkSwitch(string sourceCode, string url)
        {

            if (url.Contains(originalUrl) == false || originalUrl.Equals("") == true)
                originalUrl = url;

            if (checkEndOfLinkContainsEndy(url))
                url = originalUrl;


            string ans = "";
            sourceCode = sourceCode.ToLower();
            sourceCode = sourceCode.Replace('\'', '"');
            string[] linksParsing = sourceCode.Split(new string[] { "a href=" }, StringSplitOptions.None);
            ans = linksParsing[0];
            for (int i = 1; i < linksParsing.Length; i++)
            {

                string s = linksParsing[i].Substring(0, 6);
                if (ans.LastIndexOf('"') == (ans.Length - 1))
                    ans = ans.Substring(0, ans.Length - 1);
                if (!s.Contains("https"))
                {
                    string r = linksParsing[i].Substring(1, linksParsing[i].Length - 1);

                    ans += "a href=\"https://localhost/?url=" + r + "\"";
                }
                else
                {
                    ans += "a href=" + linksParsing[i];
                }
            }
            ans = switchImagesURL(ans, url);
            ans = switchLinkRel(ans, url);

            return ans;
        }

        /// <summary>
        /// The function which deals with relative links switch.
        /// </summary>
        /// <param name="sourceCode">The source code to be handled.</param>
        /// <param name="url">The url of the source code to be checked.</param>
        /// <returns>Source code after switch relative links.</returns>
        private string switchLinkRel(string sourceCode, string url)
        {
            string ans = "";
            string[] linksParsing = sourceCode.Split(new string[] { "href=" }, StringSplitOptions.None);
            ans = linksParsing[0];
            for (int i = 1; i < linksParsing.Length; i++)
            {
                if (linksParsing[i].Contains(".css") || linksParsing[i].Contains(".js"))
                {
                    ans += " href=" + linksParsing[i];
                    continue;
                }
                if (linksParsing[i][1] == '/' && linksParsing[i][2] != '/')
                {
                    ans += " href=\"" + url + linksParsing[i].Substring(1, linksParsing[i].Length - 1);
                }
                else
                {
                    ans += " href=" + linksParsing[i];
                }
            }





            twitterAddition = "";
            //   ans = ans.Replace("url(\"http" + twitterAddition + "://", "url(\"https://localhost/?url=");
            ans = ans.Replace("href=\"http" + twitterAddition + "://", "href=\"https://localhost/?url=");
            //    ans = ans.Replace("content=\"http" + twitterAddition + "://", "content=\"https://localhost/url=");
            //  ans = ans.Replace("src=\"http" + twitterAddition + "://", "src=\"https://localhost/?url=");
            //   ans = ans.Replace("baseurl\":\"http" + twitterAddition + "", "baseurl\":\"https://localhost/url=");
            //    ans = ans.Replace("host\":\"www", "host\":\"https://localhost/url=");
            ans = ans.Replace("action=\"http" + twitterAddition + "://", "action=\"https://localhost/url=");
            ans = ans.Replace("href=\"#", "href=\"https://localhost/?url=" + url + "#");

            //   ans = ans.Replace("localhost/?url=/", "localhost/?url=" + url + "/");
            // ans = ans.Replace("localhost:443/?url=/", "localhost/?url=" + url + "/");





            return ans;
        }

        /// <summary>
        /// Switch links of images.
        /// </summary>
        /// <param name="sourceCode">The source code to be handled.</param>
        /// <param name="url">The url of the source code to be handled.</param>
        /// <returns>The source code after switch the links of the images.</returns>
        private string switchImagesURL(string sourceCode, string url)
        {
            string ans = "";
            string[] linksParsing = sourceCode.Split(new string[] { "src=" }, StringSplitOptions.None);
            ans = linksParsing[0];
            for (int i = 1; i < linksParsing.Length; i++)
            {
                if (linksParsing[i][1] == '/' && linksParsing[i][2] != '/')
                {
                    ans += " src=\"" + url + linksParsing[i].Substring(1, linksParsing[i].Length - 1);
                }
                else if (linksParsing[i][1] == '/' && linksParsing[i][2] == '/')
                {
                    ans += " src=\"" + linksParsing[i].Substring(3, linksParsing[i].Length - 3);
                }
                else
                {
                    ans += " src=" + linksParsing[i];
                }
            }
            return ans;
        }

    }
}
