using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatClient
{
    public class Form1 : Form
    {
        TextBox txtLog, txtMessage, txtName;
        Button btnSend, btnConnect;

        TcpClient client;
        NetworkStream stream;
        Thread receiveThread;

        bool isConnected = false;   // ✅ NGĂN CONNECT NHIỀU LẦN

        public Form1()
        {
            this.Text = "Chat Client (Private + Multi-user)";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            Label lblName = new Label()
            {
                Text = "Name:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            txtName = new TextBox()
            {
                Location = new Point(80, 20),
                Width = 150,
                Font = new Font("Segoe UI", 10)
            };

            btnConnect = new Button()
            {
                Text = "Connect",
                Location = new Point(250, 20),
                Size = new Size(100, 30),
                BackColor = Color.LightGreen,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnConnect.Click += BtnConnect_Click;

            txtLog = new TextBox()
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 60),
                Size = new Size(540, 250),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 10)
            };

            txtMessage = new TextBox()
            {
                Location = new Point(20, 330),
                Size = new Size(420, 30),
                Font = new Font("Segoe UI", 10)
            };

            btnSend = new Button()
            {
                Text = "Send",
                Location = new Point(460, 330),
                Size = new Size(100, 30),
                BackColor = Color.LightSkyBlue,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSend.Click += BtnSend_Click;

            this.Controls.AddRange(new Control[] { lblName, txtName, btnConnect, txtLog, txtMessage, btnSend });
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                MessageBox.Show("You are already connected.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter your name.");
                return;
            }

            try
            {
                client = new TcpClient("127.0.0.1", 5000);
                stream = client.GetStream();

                SendMessage($"NAME:{txtName.Text}");

                receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                AppendChat("Connected to server.");
                isConnected = true;
                btnConnect.Enabled = false;
                txtName.ReadOnly = true;
            }
            catch
            {
                MessageBox.Show("Unable to connect to server.");
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (stream == null) return;

            string text = txtMessage.Text.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                SendMessage(text);
                txtMessage.Clear();
            }
        }

        void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            int bytes;

            while (true)
            {
                try
                {
                    bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes == 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
                    AppendChat(msg);
                }
                catch
                {
                    AppendChat("Disconnected from server.");
                    break;
                }
            }
        }

        void SendMessage(string msg)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);
            }
            catch
            {
                AppendChat("Failed to send message.");
            }
        }

        void AppendChat(string msg)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AppendChat(msg)));
                return;
            }

            txtLog.AppendText(msg + Environment.NewLine);
            txtLog.ScrollToCaret();
        }
    }
}
