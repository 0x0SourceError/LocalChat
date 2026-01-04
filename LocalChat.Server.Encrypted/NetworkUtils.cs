using System.Net;
using System.Net.Security;

namespace LocalChat.Server.Encrypted
{
    public static class SslNetworkUtils
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

        public async static Task<byte[]> ReadFrameAsync(SslStream ssl)
        {
            // SSL transmits data in frames, so read the entire message frame length
            byte[] lengthBuffer = await ReadMessageFrameLengthAsync(ssl, 4);

            // Convert the message frame length to an integer
            int length = BitConverter.ToInt32(lengthBuffer, 0);

            if (length < 0 || length > 1000000)
            {
                throw new InvalidOperationException("Message length invalid");
            }

            // Read the SSL stream data with the given message frame length
            return await ReadMessageFrameLengthAsync(ssl, length);
        }

        public async static Task WriteFrameAsync(SslStream ssl, byte[] data)
        {
            // Write the length of the message
            byte[] lengthBuffer = BitConverter.GetBytes(data.Length);
            await ssl.WriteAsync(lengthBuffer);

            // Write the data of the message
            await ssl.WriteAsync(data);
            await ssl.FlushAsync();
        }

        public async static Task<byte[]> ReadMessageFrameLengthAsync(SslStream ssl, int count)
        {
            byte[] lengthBuffer = new byte[count];
            int offset = 0;

            while (offset < count)
            {
                int read = await ssl.ReadAsync(lengthBuffer.AsMemory(offset, count - offset));

                if (read == 0)
                {
                    throw new IOException("Client disconnected");
                }

                offset += read;
            }

            return lengthBuffer;
        }
    }
}
