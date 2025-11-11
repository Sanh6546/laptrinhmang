using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace ChatClient
{
    public class Form1 : Form
    {
        TextBox txtLog, txtMessage, txtName;
        Button btnSend, btnConnect, btnAttachFile;
        Panel headerPanel, footerPanel;

        TcpClient client;
        NetworkStream stream;
        Thread receiveThread;
        bool isConnected = false;

        public Form1()
        {
            this.Text = "Chat Client (Private + Multi-user)";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);

            // Header Panel
            headerPanel = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.LightSkyBlue,
                Padding = new Padding(20, 10, 20, 10)
            };

            Label lblTitle = new Label()
            {
                Text = "💬 Chat Client",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 20)
            };

            txtName = new TextBox()
            {
                PlaceholderText = "Enter your name...",
                Width = 140,
                Height = 30,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                Location = new Point(250, 20)
            };

            btnConnect = new Button()
            {
                Text = "Connect",
                Size = new Size(90, 30),
                BackColor = Color.White,
                ForeColor = Color.LightSkyBlue,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(400, 20)
            };
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.Click += BtnConnect_Click;

            headerPanel.Controls.AddRange(new Control[] { lblTitle, txtName, btnConnect });

            // Chat Log
            txtLog = new TextBox()
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Footer Panel
            footerPanel = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10)
            };

            txtMessage = new TextBox()
            {
                PlaceholderText = "Type your message...",
                Width = 300,
                Height = 30,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 20)
            };
            txtMessage.KeyPress += TxtMessage_KeyPress;

            btnAttachFile = new Button()
            {
                Text = "📎",
                Size = new Size(40, 30),
                BackColor = Color.LightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Location = new Point(320, 20)
            };
            btnAttachFile.FlatAppearance.BorderSize = 0;
            btnAttachFile.Click += BtnAttachFile_Click;

            btnSend = new Button()
            {
                Text = "Send",
                Size = new Size(80, 30),
                BackColor = Color.LightSkyBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(370, 20)
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += BtnSend_Click;

            footerPanel.Controls.AddRange(new Control[] { txtMessage, btnAttachFile, btnSend });

            this.Controls.AddRange(new Control[] { headerPanel, txtLog, footerPanel });
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
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to connect to server: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            SendTextMessage();
        }

        private void TxtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SendTextMessage();
                e.Handled = true;
            }
        }

        private void SendTextMessage()
        {
            if (stream == null || !isConnected) return;

            string text = txtMessage.Text.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                SendMessage($"[{txtName.Text}]: {text}");
                txtMessage.Clear();
            }
        }

        private void BtnAttachFile_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                MessageBox.Show("Please connect to server first!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select file to send";
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SendFile(openFileDialog.FileName);
                }
            }
        }

        private void SendFile(string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                byte[] fileData = File.ReadAllBytes(filePath);

                // Create file header: FILE|SenderName|FileName|FileSize
                string fileHeader = $"FILE|{txtName.Text}|{fileName}|{fileData.Length}";
                byte[] headerBytes = Encoding.UTF8.GetBytes(fileHeader);

                // Send header
                stream.Write(headerBytes, 0, headerBytes.Length);

                // Small delay to ensure header is processed
                Thread.Sleep(100);

                // Send file data
                stream.Write(fileData, 0, fileData.Length);

                AppendChat($"[File Sent] You sent: {fileName} ({FormatFileSize(fileData.Length)})");
            }
            catch (Exception ex)
            {
                AppendChat($"[Error] Failed to send file: {ex.Message}");
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

        void ReceiveMessages()
        {
            byte[] buffer = new byte[4096]; // Larger buffer for file transfer
            int bytesRead;

            while (isConnected && client != null && client.Connected)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        AppendChat("Disconnected from server.");
                        break;
                    }

                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Check if this is a file transfer
                    if (receivedData.StartsWith("FILE|"))
                    {
                        ProcessFileTransfer(receivedData, buffer, bytesRead);
                    }
                    else
                    {
                        // Regular text message
                        AppendChat(receivedData);
                    }
                }
                catch (IOException)
                {
                    AppendChat("Disconnected from server.");
                    break;
                }
                catch (Exception ex)
                {
                    AppendChat($"[Error] Receive error: {ex.Message}");
                    break;
                }
            }
        }

        private void ProcessFileTransfer(string fileHeader, byte[] buffer, int initialBytesRead)
        {
            try
            {
                string[] headerParts = fileHeader.Split('|');
                if (headerParts.Length >= 4)
                {
                    string senderName = headerParts[1];
                    string fileName = headerParts[2];
                    int fileSize = int.Parse(headerParts[3]);

                    // Calculate how much file data we've already received in the initial read
                    int headerLength = Encoding.UTF8.GetByteCount(fileHeader);
                    int fileDataReceived = initialBytesRead - headerLength;

                    byte[] fileData = new byte[fileSize];

                    // Copy any file data that came with the header
                    if (fileDataReceived > 0)
                    {
                        Array.Copy(buffer, headerLength, fileData, 0, fileDataReceived);
                    }

                    // Read remaining file data
                    int totalReceived = fileDataReceived;
                    while (totalReceived < fileSize)
                    {
                        int bytesToRead = Math.Min(buffer.Length, fileSize - totalReceived);
                        int bytesRead = stream.Read(fileData, totalReceived, bytesToRead);
                        if (bytesRead == 0) break;
                        totalReceived += bytesRead;
                    }

                    // Save the file
                    string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string savePath = Path.Combine(downloadsPath, fileName);

                    // Handle duplicate file names
                    int counter = 1;
                    string originalPath = savePath;
                    while (File.Exists(savePath))
                    {
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalPath);
                        string extension = Path.GetExtension(originalPath);
                        savePath = Path.Combine(downloadsPath, $"{fileNameWithoutExt} ({counter}){extension}");
                        counter++;
                    }

                    File.WriteAllBytes(savePath, fileData);
                    AppendChat($"[File Received] {senderName} sent: {Path.GetFileName(savePath)} ({FormatFileSize(fileSize)})");
                    AppendChat($"[File Saved] Location: {savePath}");
                }
            }
            catch (Exception ex)
            {
                AppendChat($"[Error] File receive failed: {ex.Message}");
            }
        }

        void SendMessage(string msg)
        {
            try
            {
                if (stream != null && stream.CanWrite)
                {
                    byte[] data = Encoding.UTF8.GetBytes(msg);
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                AppendChat($"Failed to send message: {ex.Message}");
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
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isConnected = false;
            stream?.Close();
            client?.Close();
            receiveThread?.Abort();
            base.OnFormClosing(e);
        }
    }
}