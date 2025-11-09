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
        Button btnStart, btnStop, btnViewHistory;
        ListBox lstClients;

        TcpListener listener;
        List<TcpClient> clients = new List<TcpClient>();
        Dictionary<TcpClient, string> clientNames = new Dictionary<TcpClient, string>();
        bool isRunning = false;

        public Form1()
        {
            this.Text = "Chat Server (Private + Multi-client)";
            this.Size = new Size(600, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            btnStart = new Button()
            {
                Text = "Start Server",
                Location = new Point(20, 20),
                Size = new Size(120, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnStart.Click += BtnStart_Click;

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

            btnViewHistory = new Button()
            {
                Text = "View History",
                Location = new Point(300, 20),
                Size = new Size(120, 35),
                BackColor = Color.LightSkyBlue,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnViewHistory.Click += BtnViewHistory_Click;

            txtLog = new TextBox()
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 70),
                Size = new Size(400, 330),
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LightGreen
            };

            lstClients = new ListBox()
            {
                Location = new Point(440, 70),
                Size = new Size(120, 330),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White
            };

            Label lblClients = new Label()
            {
                Text = "Connected Users",
                Location = new Point(440, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            this.Controls.AddRange(new Control[] { btnStart, btnStop, btnViewHistory, txtLog, lstClients, lblClients });
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
                foreach (var c in clients) c.Close();
                clients.Clear();
                clientNames.Clear();
            }

            lstClients.Items.Clear();
            AppendLog("Server stopped.");
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void BtnViewHistory_Click(object sender, EventArgs e)
        {
            string path = "chat_log.txt";
            if (!System.IO.File.Exists(path))
            {
                MessageBox.Show("No chat history found.", "History", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string history = System.IO.File.ReadAllText(path, Encoding.UTF8);
            Form historyForm = new Form()
            {
                Text = "Chat History",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent
            };

            TextBox txtHistory = new TextBox()
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                Text = history
            };

            historyForm.Controls.Add(txtHistory);
            historyForm.ShowDialog();
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
                    // --- Xử lý NAME ---
                    if (message.StartsWith("NAME:"))
                    {
                        string name = message.Substring(5).Trim();

                        lock (clients)
                        {
                            if (clientNames.ContainsKey(client))
                            {
                                SendToClient("⚠️ You are already registered.", client);
                                continue;
                            }

                            if (clientNames.Any(x => x.Value.Equals(name, StringComparison.OrdinalIgnoreCase)))
                            {
                                SendToClient("⚠️ Name already in use! Choose another.", client);
                                continue;
                            }

                            clientNames[client] = name;
                        }

                        AppendLog($"User '{name}' connected.");
                        Broadcast($"🔔 {name} has joined the chat.", client);
                        UpdateClientList();
                        continue;
                    }

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

            if (message.StartsWith("@"))
            {
                int colonIdx = message.IndexOf(':');
                if (colonIdx > 1)
                {
                    string target = message.Substring(1, colonIdx - 1);
                    string content = message.Substring(colonIdx + 1).Trim();

                    TcpClient targetClient = null;

                    lock (clients)
                    {
                        targetClient = clientNames.FirstOrDefault(x =>
                            x.Value.Equals(target, StringComparison.OrdinalIgnoreCase)).Key;
                    }

                    if (targetClient != null)
                    {
                        string msg = $"[Private] {senderName} → {target}: {content}";
                        SendToClient(msg, targetClient);
                        SendToClient(msg, sender);
                        AppendLog(msg);
                    }
                    else
                        SendToClient($"⚠️ User '{target}' not found.", sender);
                    return;
                }
            }

            string normalMsg = $"{senderName}: {message}";
            AppendLog(normalMsg);
            Broadcast(normalMsg, sender);
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
                        try { c.GetStream().Write(data, 0, data.Length); }
                        catch { }
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
            txtLog.ScrollToCaret();
            SaveToHistory(msg);
        }

        void SaveToHistory(string msg)
        {
            string path = "chat_log.txt";
            string line = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {msg}";
            try
            {
                System.IO.File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
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
                foreach (var name in clientNames.Values)
                    lstClients.Items.Add(name);
            }
        }
    }
}
