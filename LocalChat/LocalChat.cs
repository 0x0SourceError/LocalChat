using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LocalChat
{
    public partial class frmLocalChat : Form
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        TcpClient? client;

        public frmLocalChat()
        {
            InitializeComponent();
        }

        private void frmLocalChat_Load(object sender, EventArgs e)
        {
            rtbConsole.Text = "Connect to port: ";
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
            int port = CheckPort();

            if (port == -1)
                return;

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
                // Send a message to the server in a asycnronous manner
                byte[] data = Encoding.ASCII.GetBytes(txtInput.Text);
                await stream.WriteAsync(data, 0, data.Length);
                txtInput.Text = string.Empty;
            }
            catch (Exception e)
            {
                rtbConsole.Text += "\n" + e.Message;
            }
        }

        int CheckPort()
        {
            bool validPort = int.TryParse(txtInput.Text, out int port);
            if (!validPort || port <= 0)
            {
                rtbConsole.Text += "\nIncorrect port value.";
                rtbConsole.Text += "\n\nConnect to port: ";
                return -1;
            }

            return port;
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

                int data = client.GetStream().Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, data);
                rtbConsole.Invoke(() => rtbConsole.Text += "\n[CLIENT]: " + response);
            }
        }
    }
}
