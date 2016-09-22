using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer
{
    class CSSInjectionHandler
    {


        private List<string> listOfCSharpCommands = new List<string> { "<style>", "</style>", "src=", "div{", "body{", "html{"
        ,"visible:","position:absolute"};


        /// <summary>
        /// Checking for CSSInjection attack.
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
