using System;
using System.Collections.Generic;
using System.Drawing;
<<<<<<< HEAD
using System.Linq;
=======
<<<<<<< HEAD
using System.Linq;
=======
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
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
<<<<<<< HEAD
        Button btnStart, btnStop, btnViewHistory;
=======
        Button btnStart, btnStop;
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
        ListBox lstClients;

        TcpListener listener;
        List<TcpClient> clients = new List<TcpClient>();
<<<<<<< HEAD
        Dictionary<TcpClient, string> clientNames = new Dictionary<TcpClient, string>();
=======
<<<<<<< HEAD
        Dictionary<TcpClient, string> clientNames = new Dictionary<TcpClient, string>();
=======
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
        bool isRunning = false;

        public Form1()
        {
<<<<<<< HEAD
            this.Text = "Chat Server (Private + Multi-client)";
            this.Size = new Size(600, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

=======
<<<<<<< HEAD
            this.Text = "Chat Server (Private + Multi-client)";
=======
            this.Text = "Chat Server (Multi-client)";
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

<<<<<<< HEAD
            // --- Nút Start Server ---
=======
            // Nút Start Server
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
            btnStart = new Button()
            {
                Text = "Start Server",
                Location = new Point(20, 20),
                Size = new Size(120, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnStart.Click += BtnStart_Click;

<<<<<<< HEAD
=======
<<<<<<< HEAD
            // --- Nút Stop Server ---
=======
            // Nút Stop Server
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
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

<<<<<<< HEAD
            btnViewHistory = new Button()
            {
                Text = "View History",
                Location = new Point(300, 20),
                Size = new Size(120, 35),
                BackColor = Color.LightSkyBlue,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnViewHistory.Click += BtnViewHistory_Click;

=======
<<<<<<< HEAD
            // --- Log ---
=======
            // Textbox hiển thị log
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
            txtLog = new TextBox()
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 70),
<<<<<<< HEAD
                Size = new Size(400, 330),
=======
                Size = new Size(400, 300),
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LightGreen
            };

<<<<<<< HEAD
            lstClients = new ListBox()
            {
                Location = new Point(440, 70),
                Size = new Size(120, 330),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White
=======
<<<<<<< HEAD
            // --- Danh sách client ---
=======
            // Danh sách client
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
            lstClients = new ListBox()
            {
                Location = new Point(440, 70),
                Size = new Size(120, 300),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
            };

            Label lblClients = new Label()
            {
<<<<<<< HEAD
                Text = "Connected Users",
=======
<<<<<<< HEAD
                Text = "Connected Users",
=======
                Text = "Connected Clients",
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
                Location = new Point(440, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

<<<<<<< HEAD
            this.Controls.AddRange(new Control[] { btnStart, btnStop, btnViewHistory, txtLog, lstClients, lblClients });
        }
=======
            this.Controls.AddRange(new Control[] { btnStart, btnStop, txtLog, lstClients, lblClients });
        }

>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
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
<<<<<<< HEAD

            lock (clients)
            {
                foreach (var c in clients) c.Close();
                clients.Clear();
                clientNames.Clear();
            }

=======
            lock (clients)
            {
                foreach (var c in clients)
                    c.Close();
                clients.Clear();
<<<<<<< HEAD
                clientNames.Clear();
=======
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
            }
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
            lstClients.Items.Clear();
            AppendLog("Server stopped.");
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

<<<<<<< HEAD
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

=======
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
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
<<<<<<< HEAD

                    AppendLog("A new client connected.");
=======
                    AppendLog("A new client connected.");
<<<<<<< HEAD
=======
                    UpdateClientList();
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa

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
<<<<<<< HEAD
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

=======
<<<<<<< HEAD

                    // Nếu là tin nhắn NAME:
                    if (message.StartsWith("NAME:"))
                    {
                        string name = message.Substring(5).Trim();
                        lock (clients)
                        {
                            clientNames[client] = name;
                        }
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
                        AppendLog($"User '{name}' connected.");
                        Broadcast($"🔔 {name} has joined the chat.", client);
                        UpdateClientList();
                        continue;
                    }

<<<<<<< HEAD
                    ProcessMessage(message, client);
=======
                    // Xử lý tin nhắn
                    ProcessMessage(message, client);
=======
                    AppendLog("Received: " + message);
                    Broadcast(message, client);
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
                }
            }
            catch
            {
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
                if (clientNames.ContainsKey(client))
                {
                    string name = clientNames[client];
                    AppendLog($"User '{name}' disconnected.");
                    Broadcast($"🔕 {name} has left the chat.", client);
<<<<<<< HEAD

=======
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
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

<<<<<<< HEAD
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
=======
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
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
                    }

                    if (targetClient != null)
                    {
<<<<<<< HEAD
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
=======
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
=======
                lock (clients) clients.Remove(client);
                AppendLog("A client disconnected.");
                UpdateClientList();
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
            }
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
        }

        void Broadcast(string message, TcpClient sender)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
<<<<<<< HEAD

=======
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
            lock (clients)
            {
                foreach (var c in clients)
                {
                    if (c != sender)
                    {
<<<<<<< HEAD
                        try { c.GetStream().Write(data, 0, data.Length); }
                        catch { }
=======
                        try { c.GetStream().Write(data, 0, data.Length); } catch { }
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
                    }
                }
            }
        }

<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
        void SendToClient(string message, TcpClient client)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                client.GetStream().Write(data, 0, data.Length);
            }
            catch { }
        }

<<<<<<< HEAD
=======
=======
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
        void AppendLog(string msg)
        {
            if (txtLog.InvokeRequired)
            {
<<<<<<< HEAD
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
=======
<<<<<<< HEAD
                txtLog.Invoke(new Action(() => AppendLog(msg)));
                return;
            }
            txtLog.AppendText(msg + Environment.NewLine);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
=======
                txtLog.Invoke(new Action(() =>
                {
                    txtLog.AppendText(msg + Environment.NewLine);
                    txtLog.SelectionStart = txtLog.Text.Length;
                    txtLog.ScrollToCaret();
                }));
            }
            else
            {
                txtLog.AppendText(msg + Environment.NewLine);
                txtLog.SelectionStart = txtLog.Text.Length;
                txtLog.ScrollToCaret();
            }
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
        }

        void UpdateClientList()
        {
            if (lstClients.InvokeRequired)
            {
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
                lstClients.Invoke(new Action(UpdateClientList));
                return;
            }

            lstClients.Items.Clear();
            lock (clients)
            {
<<<<<<< HEAD
                foreach (var name in clientNames.Values)
                    lstClients.Items.Add(name);
=======
                foreach (var kv in clientNames)
                    lstClients.Items.Add(kv.Value);
=======
                lstClients.Invoke(new Action(() =>
                {
                    lstClients.Items.Clear();
                    lock (clients)
                    {
                        foreach (var c in clients) lstClients.Items.Add(c.Client.RemoteEndPoint.ToString());
                    }
                }));
            }
            else
            {
                lstClients.Items.Clear();
                lock (clients)
                {
                    foreach (var c in clients) lstClients.Items.Add(c.Client.RemoteEndPoint.ToString());
                }
>>>>>>> b7558a56e43304b5f75382792fd3c6dd63f37c4d
>>>>>>> 0e1df023994a6c860a96913d4bfa0486d49df2fa
            }
        }
    }
}
