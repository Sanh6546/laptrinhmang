using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatServer
{
    public class Form1 : Form
    {
        TextBox txtLog;
        Button btnStart, btnStop;
        ListBox lstClients;

        TcpListener listener;
        List<TcpClient> clients = new List<TcpClient>();
        Dictionary<TcpClient, string> clientNames = new Dictionary<TcpClient, string>();
        bool isRunning = false;

        public Form1()
        {
            this.Text = "Chat Server (Private + Multi-client)";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            // --- Nút Start Server ---
            btnStart = new Button()
            {
                Text = "Start Server",
                Location = new Point(20, 20),
                Size = new Size(120, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnStart.Click += BtnStart_Click;

            // --- Nút Stop Server ---
            btnStop = new Button()
            {
                Text = "Stop Server",
                Location = new Point(160, 20),
                Size = new Size(120, 35),
                BackColor = Color.LightCoral,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            btnStop.Click += BtnStop_Click;

            // --- Log ---
            txtLog = new TextBox()
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 70),
                Size = new Size(400, 300),
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LightGreen
            };

            // --- Danh sách client ---
            lstClients = new ListBox()
            {
                Location = new Point(440, 70),
                Size = new Size(120, 300),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblClients = new Label()
            {
                Text = "Connected Users",
                Location = new Point(440, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            this.Controls.AddRange(new Control[] { btnStart, btnStop, txtLog, lstClients, lblClients });
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            Thread serverThread = new Thread(StartServer);
            serverThread.IsBackground = true;
            serverThread.Start();
            AppendLog("Server started on port 5000...");
            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            isRunning = false;
            listener?.Stop();
            lock (clients)
            {
                foreach (var c in clients)
                    c.Close();
                clients.Clear();
                clientNames.Clear();
            }
            lstClients.Items.Clear();
            AppendLog("Server stopped.");
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            isRunning = true;

            while (isRunning)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    lock (clients) clients.Add(client);
                    AppendLog("A new client connected.");

                    Thread clientThread = new Thread(HandleClient);
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch { break; }
            }
        }

        void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Nếu là tin nhắn NAME:
                    if (message.StartsWith("NAME:"))
                    {
                        string name = message.Substring(5).Trim();
                        lock (clients)
                        {
                            clientNames[client] = name;
                        }
                        AppendLog($"User '{name}' connected.");
                        Broadcast($"🔔 {name} has joined the chat.", client);
                        UpdateClientList();
                        continue;
                    }

                    // Xử lý tin nhắn
                    ProcessMessage(message, client);
                }
            }
            catch
            {
                if (clientNames.ContainsKey(client))
                {
                    string name = clientNames[client];
                    AppendLog($"User '{name}' disconnected.");
                    Broadcast($"🔕 {name} has left the chat.", client);
                    lock (clients)
                    {
                        clients.Remove(client);
                        clientNames.Remove(client);
                    }
                    UpdateClientList();
                }
            }
        }

        void ProcessMessage(string message, TcpClient sender)
        {
            string senderName = clientNames.ContainsKey(sender) ? clientNames[sender] : "Unknown";

            // Nếu có định dạng @Tên: nội dung
            if (message.StartsWith("@"))
            {
                int colonIndex = message.IndexOf(':');
                if (colonIndex > 1)
                {
                    string targetName = message.Substring(1, colonIndex - 1).Trim();
                    string content = message.Substring(colonIndex + 1).Trim();

                    TcpClient targetClient = null;
                    lock (clients)
                    {
                        foreach (var kv in clientNames)
                        {
                            if (kv.Value.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                            {
                                targetClient = kv.Key;
                                break;
                            }
                        }
                    }

                    if (targetClient != null)
                    {
                        string privateMsg = $"[Private] {senderName} → {targetName}: {content}";
                        AppendLog(privateMsg);
                        SendToClient(privateMsg, targetClient);
                        SendToClient(privateMsg, sender);
                    }
                    else
                    {
                        SendToClient($"⚠️ User '{targetName}' not found.", sender);
                    }
                }
            }
            else
            {
                // Broadcast tin nhắn thường
                string normalMsg = $"{senderName}: {message}";
                AppendLog(normalMsg);
                Broadcast(normalMsg, sender);
            }
        }

        void Broadcast(string message, TcpClient sender)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            lock (clients)
            {
                foreach (var c in clients)
                {
                    if (c != sender)
                    {
                        try { c.GetStream().Write(data, 0, data.Length); } catch { }
                    }
                }
            }
        }

        void SendToClient(string message, TcpClient client)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                client.GetStream().Write(data, 0, data.Length);
            }
            catch { }
        }

        void AppendLog(string msg)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AppendLog(msg)));
                return;
            }
            txtLog.AppendText(msg + Environment.NewLine);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        void UpdateClientList()
        {
            if (lstClients.InvokeRequired)
            {
                lstClients.Invoke(new Action(UpdateClientList));
                return;
            }

            lstClients.Items.Clear();
            lock (clients)
            {
                foreach (var kv in clientNames)
                    lstClients.Items.Add(kv.Value);
            }
        }
    }
}
