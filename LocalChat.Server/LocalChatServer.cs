using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalChat.Server
{
    internal class LocalChatServer
    {
        static IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        static List<TcpClient> clients = new List<TcpClient>();
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

            using TcpListener server = new TcpListener(ipAddress, port);
            try
            {
                server.Start();
                Console.WriteLine("> Created server at port: " + port);
                Console.WriteLine("> Waiting for clients");

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
                // Handle each client respectively
                TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine("Client connected!");
                clients.Add(client);
                _ = HandleClientAsync(client);
            }
        }

        async static Task HandleClientAsync(TcpClient client)
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[512];

                    // Read the data coming from each client in a asynchronous manner
                    int data = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                    if (data == 0)
                        break;

                    string response = Encoding.ASCII.GetString(buffer, 0, data);
                    byte[] responseData = Encoding.ASCII.GetBytes(response);

                    // Handles broadcasting the message the client sent to all clients
                    await BroadcastMessageAsync(responseData);
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
                client.Close();
                clients.Remove(client);
            }
        }

        async static Task BroadcastMessageAsync(byte[] response)
        {
            string responseData = Encoding.ASCII.GetString(response);
            Console.WriteLine("[CLIENT]: " + responseData);

            try
            {
                foreach (TcpClient c in clients)
                {
                    // Asyncronously write the clients message to all clients
                    await c.GetStream().WriteAsync(response);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error trying to broadcast message to clients: " + e.Message);
            }
        }
    }
}
