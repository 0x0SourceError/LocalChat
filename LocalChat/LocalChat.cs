using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalChat
{
    public partial class frmLocalChat : Form
    {
        TcpClient? client;
        string connectToServerPrompt = "Connect to server (IP:port):";
        string usernameDisplay = "Username: ";
        static string username = Environment.MachineName;

        public frmLocalChat()
        {
            InitializeComponent();
        }

        async Task ConnectToServer(string response)
        {
            rtbConsole.Text += response;
            string[] parameters = txtInput.Text.Split(":");
            if (parameters.Length != 2)
            {
                rtbConsole.Text += "\nConnection string should be in (IP:port) format. (Ex: 192.168.1.1:6000)";
                rtbConsole.Text += $"\n\n{connectToServerPrompt}:";
                return;
            }

            IPAddress ipAddress = CheckIPAddress(parameters[0]);
            int port = CheckPort(parameters[1]);

            if (ipAddress == IPAddress.None || port == -1)
            {
                rtbConsole.Text += $"\n\n{connectToServerPrompt}:";
                return;
            }

            try
            {
                // Attempt to connect to the server
                client = new TcpClient();
                rtbConsole.Text += "\nAttempting to connect to server";
                btnSend.Enabled = false;

                // Wait only 2 seconds before displaying a message to the user if timed out
                await client.ConnectAsync(ipAddress, port).WaitAsync(new TimeSpan(0, 0, 2));
                rtbConsole.Text = "Connected to server.";
                txtInput.Text = string.Empty;
                btnSend.Enabled = true;

                // Start reading messages in a async manner using a background worker
                await ReadMessagesAsync();
            }
            catch (Exception)
            {
                rtbConsole.Text += "\nThere was a connection error, try again.";
                rtbConsole.Text += $"\n\n{connectToServerPrompt}:";
                btnSend.Enabled = true;
            }
        }

        async Task SendMessageToServer(NetworkStream stream)
        {
            try
            {
                string message = $"[{username}]: " + txtInput.Text;
                byte[] data = Encoding.ASCII.GetBytes(message);

                // Send a message to the server in a asycnronous manner
                await stream.WriteAsync(data, 0, data.Length);
                txtInput.Text = string.Empty;
            }
            catch (Exception e)
            {
                rtbConsole.Text += "\n" + e.Message;
            }
        }

        async Task ReadMessagesAsync()
        {
            while (true)
            {
                byte[] buffer = new byte[512];
                if (client == null)
                    break;

                int data = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, data);

                // The GUI item is being accessed from a different thread so Invoke() is used
                rtbConsole.Text += "\n" + response;
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

            await SendMessageToServer(client.GetStream());
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

            if (client == null || !client.Connected)
                return;

            string message = $"[{oldUsername}] changed their name to [{newUsername}]";
            byte[] data = Encoding.ASCII.GetBytes(message);

            try
            {
                await client.GetStream().WriteAsync(data, 0, data.Length);
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
