using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LocalChat.Server
{
    public static class NetworkUtils
    {
        public async static Task<IPAddress> GetServerIPAddressAsync()
        {
            // Get all IP adresses from this computers host name
            IPHostEntry entry = await Dns.GetHostEntryAsync(Dns.GetHostName());
            foreach (IPAddress address in entry.AddressList)
            {
                // Return the first instance of an IP address that is not IPv6 (IPv4)
                if (!address.IsIPv6LinkLocal && !address.IsIPv6Multicast && !address.IsIPv6SiteLocal)
                    return address;
            }

            return IPAddress.None;
        }
    }
}
