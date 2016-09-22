using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ProxyServer
{
    class HttpClient
    {
        static Mutex mut = new Mutex();
        static Mutex mut2 = new Mutex();
        LinkExchange linkEx = new LinkExchange();
        TwitterAttacker ta = new TwitterAttacker();

        attacksMonitoringLogger attackLogger = new attacksMonitoringLogger();

        BufferOverFlowHandler bofH = new BufferOverFlowHandler();
        cSharpCodeInjectionHandler cSCIH = new cSharpCodeInjectionHandler();
        JavaScriptInjectionHandler jSIH = new JavaScriptInjectionHandler();
        SqlInjectionHandler sqlIH = new SqlInjectionHandler();
        XssHandler xssH = new XssHandler();
        CSSInjectionHandler cssH = new CSSInjectionHandler();

        public string originalPath = "";
        private bool avilable = false;
        StreamReader sr;
        StreamWriter sw;
        string msg;
        HttpsServer httpServer;
        public HttpClient( HttpsServer httpServer)
        {
            this.httpServer = httpServer;

            CommunicateWithServer();
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void getStreams(StreamReader sr, StreamWriter sw)
        {
            this.sr = sr;
            this.sw = sw;
        }

        /// <summary>
        /// The function which gets requests from the server.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]

        private void CommunicateWithServer()
        {
            new Thread(() =>
               {

                   while (true)
                   {

                       //  if (!sslStream.CanWrite)

                       if (avilable)
                       {
                           //string serverMsg = ReadMessage(sslStream);

                           httpServer.getHtmlContentAfterAdaption(writeToWeb(msg));

                           avilable = false;

                       }
                       else
                       {
                           Thread.Yield();
                       }
                   }

               }).Start();





        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        /// <summary>
        /// Get a message (request) from the server.
        /// </summary>
        /// <param name="m">The message</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void sendMessage(string m)
        {

            this.msg = m;


        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        static string ReadMessage(SslStream sslStream)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            byte[] buffer = new byte[65536];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;


            do
            {
                // Read the client's test message.
                bytes = sslStream.Read(buffer, 0, buffer.Length - 1);

                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF or an empty message.
                if (messageData.ToString().Contains('\n'))
                    break;

            } while (bytes != 0);
            return messageData.ToString();
        }

        /// <summary>
        /// Generating the url according to the rules of correct url.
        /// </summary>
        /// <param name="line">The initial url line before correction.</param>
        /// <returns></returns>
        public string urlGeneration(string line)
        {
            if (line.IndexOf("/") == 0)
            {
                return originalPath + line;
            }

            return line;

        }

        /// <summary>
        /// Write the content of the response to the browswer.
        /// </summary>
        /// <param name="serverMsg">The request message.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private string writeToWeb(string serverMsg)
        {



            string line = serverMsg;
            if (line == null || line.Equals(""))
                return "";
            line = line.Substring(0, line.IndexOf("1.1"));
            line = line.Substring(0, line.Length - 5);
            while (line.EndsWith(" "))
                line = line.Substring(0, line.Length - 1);
            line = line.ToLower();
            string url = "";
            sendCookiesToLooger(serverMsg);

            if (line == null || line.Equals(""))
                return "";

            if (line.Contains("url="))
                line = line.Substring(line.IndexOf("=") + 1);
            else
                line = line.Substring(4);


            //Get Request


            if (serverMsg.Substring(0, 5).ToLower().Contains("get"))
            {
                //original path 




                if (line.Contains("www.") && !linkEx.checkEndOfLinkContainsEndy(line))
                {
                    string word1 = line.Substring(line.IndexOf('.') + 1);
                    word1 = word1.Substring(0, word1.IndexOf('.'));

                    string word2 = "";
                    if (!originalPath.Equals(""))
                    {
                        word2 = originalPath.Substring(originalPath.IndexOf('.') + 1);
                        word2 = word2.Substring(0, word2.IndexOf('.'));
                    }
                    if ((originalPath == "" || !word1.Equals(word2)) && !linkEx.checkMiddleOfLinkContainsEndy(line))
                    {
                        if (line.Contains("http://"))
                        {
                            originalPath = line.Substring(line.IndexOf("http://") + 7);
                        }
                        else
                            originalPath = line;
                    }



                }

                else if ((line.Substring(line.IndexOf('.') + 1).Contains(originalPath.Substring(originalPath.IndexOf('.') + 1))) && !linkEx.checkMiddleOfLinkContainsEndy(line))
                {

                    if (line.Contains("http://"))
                    {
                        originalPath = line.Substring(line.IndexOf("http://") + 7);
                    }
                    else
                        originalPath = line;

                    if (originalPath.Substring(line.IndexOf('.') + 1).Contains('/'))
                    {
                        originalPath = originalPath.Substring(0, originalPath.IndexOf('/'));
                    }
                }


                string ans = "";
                if (line.Contains("twitter.com"))
                    ans = ta.handleGetForTwitter(line);
                else
                {
                    url = urlGeneration(line);

                    bool bof = bofH.checkForInjectedCommands(serverMsg);
                    bool cs = cSCIH.checkForInjectedCommands(serverMsg);
                    bool js = jSIH.checkForInjectedCommands(serverMsg);
                    bool sql = sqlIH.checkForInjectedCommands(serverMsg);
                    bool xss = xssH.checkForInjectedCommands(serverMsg);
                    bool css = cssH.checkForInjectedCommands(serverMsg);

                    if (bof)
                        attackLogger.writeToLogger("Potentially BufferOverflow detected on " + url);
                    if (cs)
                        attackLogger.writeToLogger("Potentially CSharp Injection detected on " + url);
                    if (js)
                        attackLogger.writeToLogger("Potentially Javascript Injection detected on " + url);
                    if (sql)
                        attackLogger.writeToLogger("Potentially sql Injection detected on " + url);
                    if (xss)
                        attackLogger.writeToLogger("Potentially Xss Attack detected on " + url);
                    if (css)
                        attackLogger.writeToLogger("Potentially CSS Injection detected on " + url);




                    ans = getRequest(url);
                }
                return ans;
            }
            //post request
            else if (serverMsg.Substring(0, 5).ToLower().Contains("post"))
            {

                url = urlGeneration(line);
                if (originalPath.Equals("www.twitter.com"))
                {
                    return ta.twitterPost("www.twitter.com", serverMsg, httpServer.currIP);
                }
                if (url.StartsWith("http") == false)
                    url = "http://" + url;
                string ans = postRequest(url, serverMsg);
                return ans;
            }

            //// errorrrrr
            if (line != null && line.Equals("") == false && line.Substring(0, 5).ToLower().Contains("get") == false && line.Substring(0, 5).ToLower().Contains("post") == false)
                return ("Illegal Response!");

            return "";

        }

        /// <summary>
        /// Send the cookies of a message to the logger.
        /// </summary>
        /// <param name="serverMsg">The request message.</param>
        private void sendCookiesToLooger(string serverMsg)
        {
            string[] cookie = serverMsg.Split(new string[] { "Cookie:" }, StringSplitOptions.None);
            string cd = cookie[1].Substring(0, cookie[1].IndexOf("\r\n\r\n"));
            HttpsServer.logger.writeCookies(originalPath, cd, httpServer.currIP);
        }

        /// <summary>
        /// Switch the links in the source code.
        /// </summary>
        /// <param name="sourceCode">The source code of the response.</param>
        /// <param name="url">The url of the http request.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public string changeLinks(string sourceCode, string url)
        {
            return linkEx.linkSwitch(sourceCode, url);
        }

        /// <summary>
        /// Get source code response according to the get request.
        /// </summary>
        /// <param name="url">The url of the request</param>
        /// <returns></returns>

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string getRequest(string url)
        {
            HttpWebRequest myRequest;
            bool wentTo = false;
            bool wentToSecend = false;
            url = url.Trim();

            if (url.StartsWith("http") == false && url.Contains("twitter") == false && url.StartsWith("//") == false)
                url = "http://" + url;
            else if (url.StartsWith("http") == false && url.Contains("twitter") == false && url.StartsWith("//") == true)
                url = "http:" + url;

            else if (url.StartsWith("https") == false && url.Contains("twitter") == true && url.StartsWith("//") == true)
                url = "https:" + url;

            else if (url.StartsWith("http") == false && url.Contains("twitter") == true && url.StartsWith("//") == false)
                url = "https://" + url;
        urlAgain:
            try
            {
                myRequest = (HttpWebRequest)WebRequest.Create(url);


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
            catch (Exception e)
            {
                if (!wentTo)
                {
                    url = url.Replace("www.", "");
                    wentTo = true;
                    goto urlAgain;

                }
                else if (!wentToSecend)
                {
                    url = url.Replace("http://", "");
                    wentToSecend = true;
                    goto urlAgain;
                }
            }
            return "";
        }

        /// <summary>
        /// Get source code response according to the post request.
        /// </summary>
        /// <param name="url">The url of the reqest</param>
        /// <param name="serverMsg">The full request</param>
        /// <returns></returns>
        public string postRequest(string url, string serverMsg)
        {
            if (serverMsg.Contains("application/x-www-form-urlencoded"))
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                ASCIIEncoding encoding = new ASCIIEncoding();
                string body = serverMsg.Substring(serverMsg.IndexOf("\r\n\r\n") + 4);


                byte[] data = Encoding.ASCII.GetBytes(body);
                request.Method = "POST";

                request.ContentType = "application/x-www-form-urlencoded";
                // request.CookieContainer = "247897859747575876; aeses=1; aelast=1466862543110; aestate=ab; aeb=true; _cb_ls=1; _cb=BTpqzMbWn1VDmMlcY; _chartbeat2=.1466862544444.1466862544444.1; _dy_ses_load_seq=11113%3A1469789072660; _dy_c_exps=; _dycst=dk.w.c.ws.frv2.ah.; _dy_geo=IL.AS.IL_05.IL_05_Tel%20Aviv; _dy_df_geo=Israel..Tel%20Aviv; _dy_toffset=-1; _dy_soct=12898.15511.1469789072; _dyus_8765945=34%7C360%7C0%7C3%7C8%7C0.0.1466859495876.1469789073827.2929577.0%7C210%7C31%7C6%7C116%7C4%7C0%7C0%7C0%7C0%7C0%7C0%7C4%7C0%7C0%7C0%7C0%7C7%7C4%7C7%7C0%7C0%7C0%7C0; _ga=GA1.1.1267733164.1466862541";
                request.ContentLength = data.Length;
                Stream stream = request.GetRequestStream();

                stream.Write(data, 0, data.Length);

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                return linkEx.linkSwitch(responseString, url);
            }

            else
            {


                var request = (HttpWebRequest)WebRequest.Create(url);
                string body = serverMsg.Substring(serverMsg.IndexOf("\r\n\r\n") + 4);

                byte[] data = Encoding.ASCII.GetBytes(body);
                request.Method = "POST";

                request.ContentType = "multipart/form-data";
                // request.CookieContainer = "247897859747575876; aeses=1; aelast=1466862543110; aestate=ab; aeb=true; _cb_ls=1; _cb=BTpqzMbWn1VDmMlcY; _chartbeat2=.1466862544444.1466862544444.1; _dy_ses_load_seq=11113%3A1469789072660; _dy_c_exps=; _dycst=dk.w.c.ws.frv2.ah.; _dy_geo=IL.AS.IL_05.IL_05_Tel%20Aviv; _dy_df_geo=Israel..Tel%20Aviv; _dy_toffset=-1; _dy_soct=12898.15511.1469789072; _dyus_8765945=34%7C360%7C0%7C3%7C8%7C0.0.1466859495876.1469789073827.2929577.0%7C210%7C31%7C6%7C116%7C4%7C0%7C0%7C0%7C0%7C0%7C0%7C4%7C0%7C0%7C0%7C0%7C7%7C4%7C7%7C0%7C0%7C0%7C0; _ga=GA1.1.1267733164.1466862541";
                request.ContentLength = data.Length;
                Stream stream = request.GetRequestStream();

                stream.Write(data, 0, data.Length);

                var response = (HttpWebResponse)request.GetResponse();


                return linkEx.linkSwitch(sr.ReadToEnd(), url);
            }


        }

        public static HttpWebResponse MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters)
        {
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;
            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);
            return PostForm(postUrl, userAgent, contentType, formData);
        }

        private static HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, byte[] formData)
        {
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;
            if (request == null)
            {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;

            // You could add authentication here as well if needed:
            // request.PreAuthenticate = true;
            // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

            // Send the form data to the request.
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            return request.GetResponse() as HttpWebResponse;
        }


        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;

            foreach (var param in postParameters)
            {
                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                // Skip it on the first parameter, add it to subsequent parameters.
                if (needsCLRF)
                    formDataStream.Write(Encoding.ASCII.GetBytes("\r\n"), 0, Encoding.ASCII.GetByteCount("\r\n"));

                needsCLRF = true;

                if (param.Value is FileParameter)
                {
                    FileParameter fileToUpload = (FileParameter)param.Value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        param.Key,
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(Encoding.ASCII.GetBytes(header), 0, Encoding.ASCII.GetByteCount(header));

                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(Encoding.ASCII.GetBytes(postData), 0, Encoding.ASCII.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(Encoding.ASCII.GetBytes(footer), 0, Encoding.ASCII.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }

        public class FileParameter
        {
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public FileParameter(byte[] file) : this(file, null) { }
            public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
            public FileParameter(byte[] file, string filename, string contenttype)
            {
                File = file;
                FileName = filename;
                ContentType = contenttype;
            }
        }

        /// <summary>
        /// Read the request message from the server.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void setAvilableRead()
        {

            //avilable = true;
            httpServer.getHtmlContentAfterAdaption(writeToWeb(msg));
            avilable = false;

        }
    }
}
