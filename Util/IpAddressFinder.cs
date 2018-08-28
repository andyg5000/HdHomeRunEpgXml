using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HdHomeRunEpgXml.Util
{
 public static    class IpAddressFinder
    {
        public static void PrintLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
            
                    Console.WriteLine("Internal Ip Address: " +  ip.ToString());
            
            }
            
        }
    }
}
