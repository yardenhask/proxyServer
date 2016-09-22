using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer
{
    class XssHandler
    {

        private List<string> jsCmd = new List<string>{"<script>", "<script>",  "<?php>",
         "?>", "src=", "/../", "../", "javascript:alert(", "javascript:prompt(", "javascript:confirm(",
         "href=javascript"};


        /// <summary>
        /// Checking for Xss attack.
        /// </summary>
        /// <param name="message">The request message to be checked.</param>
        /// <returns>boolean value according to the attack possibility.</returns>
        public bool checkForInjectedCommands(string message)
        {
            int counter = 0;
            message = message.ToLower();
            foreach (string command in jsCmd)
                if (message.Contains(command.ToLower()))
                    counter++;

            return counter >= 2;
        }
    }
}
