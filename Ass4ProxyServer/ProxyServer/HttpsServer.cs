using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

namespace ProxyServer
{
    class HttpsServer
    {
        public static Logger logger = new Logger();
        SslStream sslStream;
        Mutex m = new Mutex();
        int counter = 0;
        Stopwatch timer = new Stopwatch();
        bool inProcessing = false;


        static void Main(string[] args)
        {
            if (args.Length==1 && args[0].Equals("start"))
            {
                HttpsServer httpsServer = new HttpsServer(443, 1000, 1);
                httpsServer.Start();
            }
            

        }

        int m_port;
        int m_listeningInterval;
        volatile bool m_stop;
        HttpClient m_httpClient;
        TcpListener listener;
        bool isFirst = true;
        Thread t;


        static X509Certificate2 serverCertificate = null;
        public string currIP;

        public HttpsServer(int port, int listeningInterval, int numOfThreads)
        {

            m_port = port;
            m_httpClient = new HttpClient(this);
            ThreadPool.SetMaxThreads(numOfThreads, 0);
            m_listeningInterval = listeningInterval;
            m_stop = false;

        }


        /// <summary>
        /// Start the server operation.
        /// </summary>
        public void Start()
        {
            t = new Thread(() =>
              {


                  listener = new TcpListener(m_port);

                  listener.Start();
  
                  while (!m_stop)
                  {
                      if (timer.Elapsed.Milliseconds > 1000 && isFirst == false && inProcessing == false)
                          Thread.CurrentThread.Abort();


                      if (listener.Pending())
                      {


                          ThreadPool.QueueUserWorkItem((Object o) =>
                          {
                              m.WaitOne();
                              timer.Start();
                              TcpClient tcpClient = listener.AcceptTcpClient();
                              currIP = tcpClient.Client.RemoteEndPoint.ToString();

                              isFirst = false;
                              // if (isFirst)

                              counter++;
                              ProcessClient(tcpClient);

                              timer.Stop();
                              m.ReleaseMutex();

                              counter--;
                              //   isFirst = false;


                          });
                      }
                      else
                      {
                          Thread.Sleep(1000);
                      }
                  }
              });
            t.Start();



        }

        /// <summary>
        /// Handle a request which comes to the server.
        /// </summary>
        /// <param name="client">a TcpClient object of the request.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]

        void ProcessClient(TcpClient client)
        {


            inProcessing = true;

            sslStream = new SslStream(client.GetStream(), false);




            try
            {
                serverCertificate = new X509Certificate2(ProxyServer.Properties.Resources.CARoot, "pass");
        //        serverCertificate = new X509Certificate2("CARoot.pfx", "pass");


                sslStream.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls, false);


                // Set timeouts for the read and write to 5 seconds.
                sslStream.ReadTimeout = 10000000;
                sslStream.WriteTimeout = 10000000;

                // sslStream.Flush();
                // Read a message from the client.   
                string messageData = ReadMessage(sslStream);
                // Write a message to the client.


                if (messageData != null && !messageData.Equals(""))
                {
                    //        m_httpClient = new HttpClient(m_portClient, 1000, "127.0.0.1");

                    byte[] message = Encoding.UTF8.GetBytes(messageData);
                    //sslStream.Write(message); <<<<<<  writing to ssl stream - goinh back to chrome
                    // clientS.Write(messageData);
                    m_httpClient.sendMessage(messageData);
                    //      sw.Write(messageData);
                    m_httpClient.setAvilableRead();
                    if (m_httpClient.originalPath != null && !m_httpClient.originalPath.Equals(""))
                    {
                        logger.ConnectToServet(m_httpClient.originalPath, currIP);
                    }
                }

            }
            catch (Exception e)
            {


                sslStream.Close();
                client.Close();
                return;
            }
            finally
            {
                // The client stream will be closed with the sslStream
                // because we specified this behavior when creating
                // the sslStream.
                // sslStream.Close();
                //  client.Close();
                inProcessing = false;
            }

        }


        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Read and parse a request from the server.
        /// </summary>
        /// <param name="sslStream">The sslStream which serve to deliever the communication to the browser.</param>
        /// <returns>The string which contains the full request.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]



        static string ReadMessage(SslStream sslStream)
        {
            byte[] buffer = new byte[10000];
            StringBuilder messageData = new StringBuilder();
            Decoder decoder = Encoding.UTF8.GetDecoder();
            int bytes = -1;
            int length = 0;
            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length - 1);
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                if (messageData.ToString().Contains("\r\n\r\n"))
                    break;
            } while (bytes != 0);

            string ans = messageData.ToString();
            if (ans.Contains("Content-Length"))
            {
                string bodyText = "";
                string num = "";
                num = ans.Substring(ans.IndexOf("Content-Length:"), 22);
                num = num.Substring(num.IndexOf(":") + 1, 5);
                length = Convert.ToInt32(num);
                // length += 5;
                for (int i = 0; i < length; i++)
                {
                    StringBuilder messageBody = new StringBuilder();
                    byte[] body = new byte[1];
                    bytes = sslStream.Read(body, 0, 1);
                    char[] bChars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                    decoder.GetChars(body, 0, bytes, bChars, 0);
                    messageBody.Append(bChars);
                    bodyText += messageBody.ToString();
                }
                ans += bodyText;
            }
            return ans;



        }

        /// <summary>
        /// Adaption and links switch to the html source code.
        /// </summary>
        /// <param name="html">The source code which has to be adapted.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void getHtmlContentAfterAdaption(string html)
        {
            if (html == null || html.Equals(""))
            {
                sslStream.Close();
                return;
            }
            byte[] response = Encoding.UTF8.GetBytes(html);

            if (sslStream.CanWrite == false)
            {
                sslStream = new SslStream(listener.AcceptTcpClient().GetStream(), false);



                serverCertificate = new X509Certificate2("CARoot.pfx", "pass");


                sslStream.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls, false);



                // Set timeouts for the read and write to 5 seconds.
                sslStream.ReadTimeout = 5000000;
                sslStream.WriteTimeout = 5000000;

                //   sslStream.Flush();

            }
            //  Thread.Sleep(3000);
            sslStream.Write(response, 0, response.Length);
            sslStream.Close();

            return;

        }


    }
}
