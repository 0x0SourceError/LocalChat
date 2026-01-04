using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using LocalChat.Server.Encrypted;

namespace LocalChat.Encrypted
{
    public partial class frmLocalChat : Form
    {
        TcpClient? client;
        SslStream secureClientStream;
        string connectToServerPrompt = "Connect to server (IP:port):";
        string usernameDisplay = "Username: ";
        static string username = Environment.MachineName;

        public frmLocalChat()
        {
            InitializeComponent();
        }

        bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;

            rtbConsole.Text += "\nCertificate error: " + errors;
            rtbConsole.Text += $"\n\n{connectToServerPrompt} ";
            return false;
        }

        async Task ConnectToServer(string response)
        {
            object[] data = ValidateData(response);
            if (data.Length < 2)
            {
                rtbConsole.Text += "asdasdasdasd";
                return;
            }

            IPAddress ipAddress = (IPAddress)data[0];
            int port = (int)data[1];

            try
            {
                // Attempt to connect to the server
                client = new TcpClient();
                rtbConsole.Text += "\nAttempting to connect to server";
                btnSend.Enabled = false;

                // Wait only 2 seconds before displaying a message to the user if timed out
                await client.ConnectAsync(ipAddress, port).WaitAsync(new TimeSpan(0, 0, 2));
                rtbConsole.Text = string.Empty;
                string message = $"\"{username}\" connected to server";

                // Authenticate the client with the server and send a message to them
                secureClientStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                await secureClientStream.AuthenticateAsClientAsync("domain");
                await SendSecureMessageToServer(secureClientStream, message);

                txtInput.Text = string.Empty;
                btnSend.Enabled = true;

                // Start reading messages in a async manner
                await ReadSecureMessagesAsync();
            }
            catch (Exception e)
            {
                rtbConsole.Text += "\nThere was a connection error, try again." + e.Message;
                rtbConsole.Text += $"\n\n{connectToServerPrompt} ";
                btnSend.Enabled = true;
            }
        }

        async Task SendSecureMessageToServer(SslStream stream, string message)
        {
            try
            {
                // Write the data in frames
                byte[] data = Encoding.ASCII.GetBytes(message);
                await SslNetworkUtils.WriteFrameAsync(stream, data);
                txtInput.Text = string.Empty;
            }
            catch (Exception e)
            {
                rtbConsole.Text += "\n" + e.Message;
            }
        }

        async Task ReadSecureMessagesAsync()
        {
            while (true)
            {
                // Read the data that comes back from the server
                byte[] message = await SslNetworkUtils.ReadFrameAsync(secureClientStream);
                string response = Encoding.ASCII.GetString(message);

                rtbConsole.Text += response + "\n";
            }
        }

        private void frmLocalChat_Load(object sender, EventArgs e)
        {
            rtbConsole.Text = connectToServerPrompt;
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.Username))
            {
                Properties.Settings.Default.Username = username;
            }

            username = Properties.Settings.Default.Username;
            lblUsername.Text = usernameDisplay + username;
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
                return;

            if (client == null || !client.Connected)
            {
                await ConnectToServer(txtInput.Text);
                return;
            }

            string message = $"[{username}]: " + txtInput.Text;
            await SendSecureMessageToServer(secureClientStream, message);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void changeUsernameMnuItm_Click(object sender, EventArgs e)
        {
            string oldUsername = username;
            string newUsername = Microsoft.VisualBasic.Interaction.InputBox($"Enter new username\n\nCurrent: {username}", "Enter username", string.Empty);
            if (string.IsNullOrWhiteSpace(newUsername))
                return;

            username = newUsername;
            lblUsername.Text = usernameDisplay + username;
            Properties.Settings.Default.Username = username;

            if (client == null || !client.Connected || secureClientStream == null)
                return;

            string message = $"[{oldUsername}] changed their name to [{newUsername}]";
            byte[] data = Encoding.ASCII.GetBytes(message);

            try
            {
                await SslNetworkUtils.WriteFrameAsync(secureClientStream, data);
            }
            catch (Exception ex)
            {
                rtbConsole.Text += "\nUnable to broadcast message of name change.";
            }
        }

        private void frmLocalChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        object[] ValidateData(string response)
        {
            rtbConsole.Text += response;
            string[] parameters = txtInput.Text.Split(":");
            if (parameters.Length != 2)
            {
                rtbConsole.Text += "\nConnection string should be in (IP:port) format. (Ex: 192.168.1.1:6000)";
                rtbConsole.Text += $"\n\n{connectToServerPrompt} ";
                return new object[0];
            }

            IPAddress ipAddress = CheckIPAddress(parameters[0]);
            int port = CheckPort(parameters[1]);

            if (ipAddress == IPAddress.None || port == -1)
            {
                rtbConsole.Text += $"\n\n{connectToServerPrompt} ";
                return new object[0];
            }

            return new object[] { ipAddress, port };
        }

        int CheckPort(string num)
        {
            bool validPort = int.TryParse(num, out int port);
            if (!validPort || port <= 0)
            {
                rtbConsole.Text += "\nIncorrect port value.";
                return -1;
            }

            return port;
        }

        IPAddress CheckIPAddress(string ip)
        {
            bool validIPAddress = IPAddress.TryParse(ip, out IPAddress? address);
            if (!validIPAddress)
            {
                rtbConsole.Text += "\nIncorrect IP address.";
                return IPAddress.None;
            }

            return address;
        }
    }
}
