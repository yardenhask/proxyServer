using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer
{
    class SqlInjectionHandler
    {

        private List<string> blackList = new List<string>{" select "," where ", " delete ",
        " insert ", " update ", "1=1","0=0", "delete * from", "* from"};


        /// <summary>
        /// Checking for SqlInjection attack.
        /// </summary>
        /// <param name="message">The request message to be checked.</param>
        /// <returns>boolean value according to the attack possibility.</returns>
        public bool checkForInjectedCommands(string message)
        {
            int counter = 0;
            message = message.ToLower();
            foreach (string command in blackList)
                if (message.Contains(command.ToLower()))
                    counter++;

            return counter >= 2;
        }
    }
}
