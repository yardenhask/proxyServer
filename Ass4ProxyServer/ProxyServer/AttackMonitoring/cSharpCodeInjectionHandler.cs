using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyServer
{
    class cSharpCodeInjectionHandler
    {

        private List<string> listOfCSharpCommands = new List<string>{" goto ","Environment.Exit(0)", " return null ",
        " Thread.Sleep(", ".exe"};


        /// <summary>
        /// Checking for cSharpCodeInjection attack.
        /// </summary>
        /// <param name="message">The request message to be checked.</param>
        /// <returns>boolean value according to the attack possibility.</returns>
        public bool checkForInjectedCommands(string message)
        {
            int counter = 0;
            message = message.ToLower();
            foreach (string command in listOfCSharpCommands)
                if (message.Contains(command.ToLower()))
                    counter++;

            return counter >= 2;
        }
    }
}
