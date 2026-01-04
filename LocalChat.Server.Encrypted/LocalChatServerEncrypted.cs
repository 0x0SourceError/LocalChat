using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace LocalChat.Server.Encrypted
{
    internal class LocalChatServerEncrypted
    {
        static X509Certificate2? certificate;
        static List<SslStream> secureClients = new List<SslStream>();

        async static Task Main(string[] args)
        {
            int port;
            while (true)
            {
                Console.Write("Enter the port for the server: ");
                string? response = Console.ReadLine();

                bool validPort = int.TryParse(response, out port);
                if (!validPort || port <= 0)
                    Console.WriteLine("\nIncorrect value for port.\n");
                else
                    break;
            }

            Console.WriteLine("Reading \"server.json\" file");
            ServerDetails? serverDetails;
            try
            {
                string path = Directory.GetCurrentDirectory() + "/server.json";
                string contents = File.ReadAllText(path);
                serverDetails = JsonSerializer.Deserialize<ServerDetails>(contents);
            }
            catch (Exception)
            {
                Console.WriteLine("\nserver.json file is missing or does not have correct information");
                return;
            }

            // Use the certificate in the computer store along with the PFX file and export password
            try
            {
                certificate = new X509Certificate2(serverDetails.ServerPfx, serverDetails.ExportPassword, X509KeyStorageFlags.MachineKeySet);
            }
            catch (Exception)
            {
                Console.WriteLine("There was an error reading the .pfx file or password specified in \"server.json\"");
                return;
            }

            using TcpListener server = new TcpListener(IPAddress.Any, port);
            try
            {
                server.Start();
                IPAddress address = await SslNetworkUtils.GetServerIPAddressAsync();
                Console.WriteLine("Created server at IP: " + address.ToString() + " and Port: " + port);
                Console.WriteLine("Waiting for clients...");

                // Listen for clients in a asynchronous manner
                await HostSessionAsync(server);
            }
            catch (IOException e)
            {
                Console.WriteLine("Server error: " + e.Message);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Server error: " + e.Message);
            }
        }

        async static Task HostSessionAsync(TcpListener server)
        {
            while (true)
            {
                // When a client connects add them to a list of connected clients
                TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine("Client connected!");

                _ = HandleSecureClientAsync(client);
            }
        }

        async static Task HandleSecureClientAsync(TcpClient client)
        {
            SslStream clientStream = new SslStream(client.GetStream());
            secureClients.Add(clientStream);
            try
            {
                clientStream.AuthenticateAsServer(certificate, false, true);
                while (true)
                {
                    byte[] message = await SslNetworkUtils.ReadFrameAsync(clientStream);
                    await BroadcastSecureMessageAsync(message);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Client disconnected");
            }
            catch (SocketException e)
            {
                Console.WriteLine("Client error: " + e.Message);
            }
            finally
            {
                // Terminate the connection and remove the client from the list of clients
                clientStream.Close();
                secureClients.Remove(clientStream);
            }
        }


        async static Task BroadcastSecureMessageAsync(byte[] response)
        {
            string responseData = Encoding.ASCII.GetString(response);
            Console.WriteLine(responseData);

            try
            {
                foreach (SslStream ssl in secureClients)
                {
                    // Asyncronously write the clients message to all clients
                    await SslNetworkUtils.WriteFrameAsync(ssl, response);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error trying to broadcast message to clients: " + e.Message);
            }
        }
    }
}
