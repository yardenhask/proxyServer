using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer
{
    class attacksMonitoringLogger
    {

        StreamWriter sw;
        public attacksMonitoringLogger()
        {
            FileStream fs;
            if (File.Exists("attacksLogger.txt") == true)
            {
                fs = new FileStream("attacksLogger.txt", FileMode.Append);
            }
            else
            {
                fs = new FileStream("attacksLogger.txt", FileMode.Create);
            }
            sw = new StreamWriter(fs);

        }


        public void writeToLogger(string data)
        {
            sw.Flush();
            sw.WriteLine("Potentially Attack Monitored on " + DateTime.Now.ToString());
            sw.WriteLine(data);
            sw.WriteLine();
        }
    }
}
