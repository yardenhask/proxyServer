using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer
{
    class JavaScriptInjectionHandler
    {
        private List<string> listOfCSharpCommands = new List<string>{" var "," return ", "confirm(","prompt(",
        " alert(", ".exe"};


        /// <summary>
        /// Checking for JavsScriptInjection attack.
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
