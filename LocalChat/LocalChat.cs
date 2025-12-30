using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalChat
{
    public partial class frmLocalChat : Form
    {
        TcpClient? client;

        public frmLocalChat()
        {
            InitializeComponent();
        }

        private void frmLocalChat_Load(object sender, EventArgs e)
        {
            rtbConsole.Text = "Connect to server (IP:port): ";
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
                return;

            if (client == null || !client.Connected)
            {
                ConnectToServer(txtInput.Text);
                return;
            }

            await SendMessageToServer(client.GetStream());
        }

        void ConnectToServer(string response)
        {
            rtbConsole.Text += response;
            string[] parameters = txtInput.Text.Split(":");
            if (parameters.Length != 2)
            {
                rtbConsole.Text += "\nConnection string should be in (IP:port) format. (Ex: 192.168.1.1:6000)";
                rtbConsole.Text += "\n\nConnect to server (IP:port):";
                return;
            }

            IPAddress ipAddress = CheckIPAddress(parameters[0]);
            int port = CheckPort(parameters[1]);

            if (ipAddress == IPAddress.None || port == -1)
            {
                rtbConsole.Text += "\n\nConnect to server (IP:port):";
                return;
            }

            try
            {
                // Attempt to connect to the server
                client = new TcpClient(ipAddress.ToString(), port);
                rtbConsole.Text = "Connected to server.";
                txtInput.Text = string.Empty;

                // Start reading messages in a async manner using a background worker
                bwrReadMessages.RunWorkerAsync();
            }
            catch (SocketException)
            {
                rtbConsole.Text += "\nThere was a connection error, try again.";
                rtbConsole.Text += "\n\nConnect to port: ";
            }
        }

        async Task SendMessageToServer(NetworkStream stream)
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(txtInput.Text);

                // Send a message to the server in a asycnronous manner
                await stream.WriteAsync(data, 0, data.Length);
                txtInput.Text = string.Empty;
            }
            catch (Exception e)
            {
                rtbConsole.Text += "\n" + e.Message;
            }
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

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void bwrReadMessages_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (true)
            {
                byte[] buffer = new byte[512];
                if (client == null)
                    break;

                // This Read() is done within a asyncronous background worker
                int data = client.GetStream().Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, data);

                // The GUI item is being accessed from a different thread so Invoke() is used
                rtbConsole.Invoke(() => rtbConsole.Text += "\n[CLIENT]: " + response);
            }
        }
    }
}
