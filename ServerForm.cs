using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatServer
{
    public class Form1 : Form
    {
        TextBox txtLog;
        Button btnStart, btnStop, btnViewHistory;
        FlowLayoutPanel pnlClients;

        TcpListener listener;
        List<TcpClient> clients = new List<TcpClient>();
        Dictionary<TcpClient, string> clientNames = new Dictionary<TcpClient, string>();

        bool isRunning = false;

        public Form1()
        {
            // ===== FORM STYLE =====
            this.Text = "Zalo Chat Server";
            this.Size = new Size(800, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColorTranslator.FromHtml("#F5F7FA");
            this.Font = new Font("Segoe UI", 10);

            // ===== HEADER =====
            Panel header = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ColorTranslator.FromHtml("#0091FF")
            };
            Label lblTitle = new Label()
            {
                Text = "💻 Zalo Chat Server",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            header.Controls.Add(lblTitle);
            this.Controls.Add(header);

            // ===== BUTTON PANEL =====
            Panel buttonPanel = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.WhiteSmoke
            };

            btnStart = CreateButton("▶ Start Server", ColorTranslator.FromHtml("#28a745"));
            btnStop = CreateButton("■ Stop Server", ColorTranslator.FromHtml("#dc3545"));
            btnViewHistory = CreateButton("🕓 View History", ColorTranslator.FromHtml("#007bff"));
            btnStop.Enabled = false;

            btnStart.Location = new Point(30, 15);
            btnStop.Location = new Point(170, 15);
            btnViewHistory.Location = new Point(310, 15);

            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnViewHistory.Click += BtnViewHistory_Click;

            buttonPanel.Controls.AddRange(new Control[] { btnStart, btnStop, btnViewHistory });
            this.Controls.Add(buttonPanel);

            // ===== SERVER LOG =====
            GroupBox grpLog = new GroupBox()
            {
                Text = "Server Logs",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 140),
                Size = new Size(500, 320),
                BackColor = Color.White
            };
            txtLog = new TextBox()
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BackColor = Color.White,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None
            };
            grpLog.Controls.Add(txtLog);
            this.Controls.Add(grpLog);

            // ===== CLIENT LIST =====
            GroupBox grpClients = new GroupBox()
            {
                Text = "Connected Users",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(540, 140),
                Size = new Size(220, 320),
                BackColor = Color.White
            };
            pnlClients = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };
            grpClients.Controls.Add(pnlClients);
            this.Controls.Add(grpClients);
        }

        private Button CreateButton(string text, Color color)
        {
            return new Button()
            {
                Text = text,
                Size = new Size(120, 35),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            Thread serverThread = new Thread(StartServer);
            serverThread.IsBackground = true;
            serverThread.Start();

            AppendLog("✅ Server started on port 5000...");
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
            pnlClients.Controls.Clear();
            AppendLog("🛑 Server stopped.");
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void BtnViewHistory_Click(object sender, EventArgs e)
        {
            string path = "history.txt";
            if (!File.Exists(path))
            {
                MessageBox.Show("No chat history found.", "History");
                return;
            }
            string history = File.ReadAllText(path, Encoding.UTF8);
            Form f = new Form()
            {
                Text = "Chat History",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White
            };
            TextBox txt = new TextBox()
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                Text = history
            };
            f.Controls.Add(txt);
            f.ShowDialog();
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
                    AppendLog("🔌 A new client connected.");

                    Thread t = new Thread(HandleClient);
                    t.IsBackground = true;
                    t.Start(client);
                }
                catch { break; }
            }
        }

        void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096]; // Increased buffer size for file transfer
            int bytesRead;

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // ===== FILE MESSAGE =====
                    if (message.StartsWith("FILE|"))
                    {
                        ProcessFileTransfer(message, client, stream);
                        continue;
                    }

                    // ===== NAME REGISTRATION =====
                    if (message.StartsWith("NAME:"))
                    {
                        string name = message.Substring(5).Trim();
                        lock (clients)
                        {
                            if (clientNames.Any(x => x.Value.Equals(name, StringComparison.OrdinalIgnoreCase)))
                            {
                                SendToClient("⚠️ Name already in use!", client);
                                continue;
                            }
                            clientNames[client] = name;
                        }
                        AppendLog($"👤 {name} connected.");
                        Broadcast($"{name} joined the chat.", client);
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
                    AppendLog($"❌ {name} disconnected.");
                    Broadcast($"{name} left the chat.", client);
                    lock (clients)
                    {
                        clients.Remove(client);
                        clientNames.Remove(client);
                    }
                    UpdateClientList();
                }
            }
        }

        private void ProcessFileTransfer(string fileHeader, TcpClient sender, NetworkStream stream)
        {
            try
            {
                string[] parts = fileHeader.Split('|');
                if (parts.Length >= 4)
                {
                    string senderName = parts[1];
                    string fileName = parts[2];
                    int fileSize = int.Parse(parts[3]);

                    // Read the file data
                    byte[] fileData = new byte[fileSize];
                    int totalRead = 0;

                    while (totalRead < fileSize)
                    {
                        int bytesToRead = Math.Min(4096, fileSize - totalRead);
                        int bytesRead = stream.Read(fileData, totalRead, bytesToRead);
                        if (bytesRead == 0) break;
                        totalRead += bytesRead;
                    }

                    AppendLog($"📎 {senderName} sent file: {fileName} ({FormatFileSize(fileSize)})");

                    // Check if it's a private file transfer or broadcast
                    if (parts.Length >= 5)
                    {
                        string target = parts[4];
                        if (!string.IsNullOrEmpty(target) && !target.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                        {
                            // Private file transfer
                            TcpClient targetClient = clientNames.FirstOrDefault(x =>
                                x.Value.Equals(target, StringComparison.OrdinalIgnoreCase)).Key;
                            if (targetClient != null)
                            {
                                SendFileToClient(fileHeader, fileData, targetClient);
                                AppendLog($"📤 File sent privately to {target}");
                            }
                            else
                            {
                                SendToClient($"⚠️ User '{target}' not found.", sender);
                            }
                            return;
                        }
                    }

                    // Broadcast file to all clients except sender
                    BroadcastFile(fileHeader, fileData, sender);
                }
            }
            catch (Exception ex)
            {
                AppendLog($"❌ File transfer error: {ex.Message}");
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
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

                    TcpClient targetClient = clientNames.FirstOrDefault(x =>
                        x.Value.Equals(target, StringComparison.OrdinalIgnoreCase)).Key;

                    if (targetClient != null)
                    {
                        string msg = $"[Private] {senderName} → {target}: {content}";
                        SendToClient(msg, targetClient);
                        SendToClient(msg, sender);
                        AppendLog(msg);
                    }
                    else
                    {
                        SendToClient($"⚠️ User '{target}' not found.", sender);
                    }
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
                    if (c != sender && c.Connected)
                        try { c.GetStream().Write(data, 0, data.Length); } catch { }
            }
        }

        void BroadcastFile(string header, byte[] fileBytes, TcpClient sender)
        {
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            lock (clients)
            {
                foreach (var c in clients)
                {
                    if (c != sender && c.Connected)
                    {
                        try
                        {
                            c.GetStream().Write(headerBytes, 0, headerBytes.Length);
                            Thread.Sleep(10); // Small delay to ensure header is processed
                            c.GetStream().Write(fileBytes, 0, fileBytes.Length);
                        }
                        catch { }
                    }
                }
            }
        }

        void SendFileToClient(string header, byte[] fileBytes, TcpClient client)
        {
            try
            {
                if (client.Connected)
                {
                    byte[] headerBytes = Encoding.UTF8.GetBytes(header);
                    client.GetStream().Write(headerBytes, 0, headerBytes.Length);
                    Thread.Sleep(10);
                    client.GetStream().Write(fileBytes, 0, fileBytes.Length);
                }
            }
            catch { }
        }

        void SendToClient(string message, TcpClient client)
        {
            try
            {
                if (client.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    client.GetStream().Write(data, 0, data.Length);
                }
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
            string path = "history.txt";
            string line = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {msg}";
            try { File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8); } catch { }
        }

        void UpdateClientList()
        {
            if (pnlClients.InvokeRequired)
            {
                pnlClients.Invoke(new Action(UpdateClientList));
                return;
            }
            pnlClients.Controls.Clear();
            lock (clients)
            {
                foreach (var name in clientNames.Values)
                {
                    Panel card = new Panel()
                    {
                        Width = 180,
                        Height = 50,
                        BackColor = ColorTranslator.FromHtml("#E6F3FF"),
                        Margin = new Padding(5),
                        Padding = new Padding(8),
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    Label lbl = new Label()
                    {
                        Text = name,
                        Location = new Point(10, 15),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = ColorTranslator.FromHtml("#007BFF")
                    };
                    card.Controls.Add(lbl);
                    pnlClients.Controls.Add(card);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isRunning = false;
            listener?.Stop();
            base.OnFormClosing(e);
        }
    }
}