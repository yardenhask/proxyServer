using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer
{
    class BufferOverFlowHandler
    {

        private List<string> listOfCSharpCommands = new List<string> { "strcpy", "buffer", "*str", "exec(\"/bin/sh\"", "exec(\"/bin/bash\"", "NOP"
        ,"char *","EIP","EBP","NOP sled"};


        /// <summary>
        /// Checking for BufferOverflow attack.
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
