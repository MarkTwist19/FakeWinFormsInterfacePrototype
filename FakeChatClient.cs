using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FakeSoftwareInterface
{
    public partial class FakeChatClient : Form
    {
        // Hotkey registration
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID_CRASH = 1;
        private const int HOTKEY_ID_GLITCH = 2;
        private const int MOD_CONTROL_ALT = 0x0003;
        private const int VK_F12 = 0x7B;
        private const int VK_F11 = 0x7A;

        // Fake data and state
        private List<FakeMessage> messageQueue = new List<FakeMessage>();
        private System.Windows.Forms.Timer chatTimer;
        private System.Windows.Forms.Timer statusTimer;
        private Random random = new Random();
        private int messageIndex = 0;
        private bool isConnected = false;
        private bool isEncrypted = false;

        // Fake chat messages script
        private readonly List<string[]> fakeChatScript = new List<string[]>
        {
            new string[] { "System", "Initializing secure channel...", "10:23:01" },
            new string[] { "System", "Handshake with relay node #42 established", "10:23:03" },
            new string[] { "User_Alpha", "Status check. Are we green?", "10:23:12" },
            new string[] { "System", "Encryption: AES-256 (simulated)", "10:23:15" },
            new string[] { "User_Sigma", "Green here. Package is secure.", "10:23:22" },
            new string[] { "User_Alpha", "Proceed with phase 2. Use protocol 7B.", "10:23:30" },
            new string[] { "System", "WARNING: Unusual traffic pattern detected", "10:23:45" },
            new string[] { "User_Sigma", "Acknowledged. Switching to backup.", "10:23:52" },
            new string[] { "User_Null", "*** CONNECTION COMPROMISED ***", "10:24:01" },
            new string[] { "System", "FATAL: Integrity check failed. Buffer overflow imminent.", "10:24:03" }
        };

        // Control declarations - MUST be here
        private RichTextBox chatDisplay;
        private Button btnConnect;
        private Button btnEncrypt;
        private Button btnSend;
        private TextBox txtMessage;
        private ListBox lstLog;
        private Label lblStatus;
        private ProgressBar progressBar;

        public FakeChatClient()
        {
            InitializeComponent();
            SetupForm();
            RegisterHotkeys();
            SetupTimers();
            LoadFakeData();
        }

        private void SetupForm()
        {
            // Deliberately amateur styling
            this.Text = "SecureChat v0.83β - [UNSTABLE BUILD]";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft Sans Serif", 8.25f);
            this.BackColor = SystemColors.Control;
            this.FormClosing += (s, e) => UnregisterHotkeys();
        }

        private void InitializeComponent()
        {
            // Main container with awkward layout
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            
            // Top status bar (cluttered)
            var statusPanel = new Panel 
            { 
                Height = 80, 
                Dock = DockStyle.Top, 
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            lblStatus = new Label 
            { 
                Text = "DISCONNECTED", 
                ForeColor = Color.Red,
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            var lblBuffer = new Label 
            { 
                Text = "Buffer: 87%", 
                Location = new Point(150, 10),
                AutoSize = true 
            };

            var lblEncryption = new Label 
            { 
                Text = "Encryption: DISABLED", 
                Location = new Point(250, 10),
                AutoSize = true 
            };

            progressBar = new ProgressBar 
            { 
                Location = new Point(10, 40),
                Size = new Size(300, 20),
                Style = ProgressBarStyle.Continuous
            };

            statusPanel.Controls.AddRange(new Control[] { lblStatus, lblBuffer, lblEncryption, progressBar });

            // Chat display area
            chatDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BorderStyle = BorderStyle.Fixed3D,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            // Control panel (mismatched controls)
            var controlPanel = new Panel 
            { 
                Height = 120, 
                Dock = DockStyle.Bottom,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnConnect = new Button 
            { 
                Text = "Initialize Connection Protocol",
                Location = new Point(10, 10),
                Size = new Size(180, 30),
                BackColor = SystemColors.Control,
                FlatStyle = FlatStyle.Standard
            };
            btnConnect.Click += BtnConnect_Click;

            btnEncrypt = new Button 
            { 
                Text = "Enable Encryption (AES)",
                Location = new Point(200, 10),
                Size = new Size(150, 30),
                Enabled = false
            };
            btnEncrypt.Click += BtnEncrypt_Click;

            btnSend = new Button 
            { 
                Text = "Transmit Message",
                Location = new Point(360, 10),
                Size = new Size(120, 30),
                Enabled = false
            };
            btnSend.Click += BtnSend_Click;

            txtMessage = new TextBox 
            { 
                Location = new Point(10, 50),
                Size = new Size(400, 20),
                Text = "Enter secure message...",
                ForeColor = SystemColors.GrayText
            };
            txtMessage.Enter += (s, e) => {
                if (txtMessage.Text == "Enter secure message...")
                {
                    txtMessage.Text = "";
                    txtMessage.ForeColor = SystemColors.WindowText;
                }
            };
            txtMessage.KeyPress += (s, e) => {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    BtnSend_Click(s, e);
                    e.Handled = true;
                }
            };

            // Log area
            var logLabel = new Label 
            { 
                Text = "System Log:", 
                Location = new Point(500, 10),
                AutoSize = true 
            };

            lstLog = new ListBox 
            { 
                Location = new Point(500, 30),
                Size = new Size(250, 80),
                BackColor = Color.FromArgb(255, 255, 240)
            };

            controlPanel.Controls.AddRange(new Control[] { 
                btnConnect, btnEncrypt, btnSend, txtMessage, logLabel, lstLog 
            });

            // Assembly
            mainPanel.Controls.AddRange(new Control[] { statusPanel, chatDisplay, controlPanel });
            this.Controls.Add(mainPanel);

            // Add some fake menu items
            var mainMenu = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Open Session...", null, null, "Ctrl+O") { Enabled = false },
                new ToolStripMenuItem("Save Log", null, null, "Ctrl+S") { Enabled = false },
                new ToolStripSeparator(),
                new ToolStripMenuItem("Exit", null, (s, e) => this.Close())
            });

            var toolsMenu = new ToolStripMenuItem("Tools");
            toolsMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Advanced Settings", null, null, "Ctrl+Shift+A") { Enabled = false },
                new ToolStripMenuItem("Debug Console", null, null, "F5") { Enabled = false },
                new ToolStripSeparator(),
                new ToolStripMenuItem("Compress Logs", null, (s, e) => AddLogMessage("System", "Log compression failed: Not enough memory"))
            });

            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add(new ToolStripMenuItem("About SecureChat v0.83β...", null, 
                (s, e) => MessageBox.Show("SecureChat v0.83β\nBuild: 2011-08-23\n\nWARNING: This is an unstable development version.\nNot intended for production use.", 
                "About", MessageBoxButtons.OK, MessageBoxIcon.Warning)));

            mainMenu.Items.AddRange(new ToolStripItem[] { fileMenu, toolsMenu, helpMenu });
            this.Controls.Add(mainMenu);
            this.MainMenuStrip = mainMenu;
        }

        private void RegisterHotkeys()
        {
            // Register Ctrl+Alt+F12 for crash
            RegisterHotKey(this.Handle, HOTKEY_ID_CRASH, MOD_CONTROL_ALT, VK_F12);
            // Register Ctrl+Alt+F11 for glitch effect
            RegisterHotKey(this.Handle, HOTKEY_ID_GLITCH, MOD_CONTROL_ALT, VK_F11);
        }

        private void UnregisterHotkeys()
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID_CRASH);
            UnregisterHotKey(this.Handle, HOTKEY_ID_GLITCH);
        }

        protected override void WndProc(ref Message m)
        {
            // Handle hotkey messages
            if (m.Msg == 0x0312) // WM_HOTKEY
            {
                int id = m.WParam.ToInt32();
                if (id == HOTKEY_ID_CRASH)
                {
                    TriggerFakeCrash();
                }
                else if (id == HOTKEY_ID_GLITCH)
                {
                    TriggerGlitchEffect();
                }
            }
            base.WndProc(ref m);
        }

        private void SetupTimers()
        {
            // Timer for fake chat messages
            chatTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            chatTimer.Tick += (s, e) => ProcessNextMessage();
            chatTimer.Stop();

            // Timer for fake status updates
            statusTimer = new System.Windows.Forms.Timer { Interval = 500 };
            statusTimer.Tick += (s, e) => UpdateFakeStatus();
            statusTimer.Start();
        }

        private void LoadFakeData()
        {
            // Load some initial system messages using the fixed method
            AppendColoredText("System: SecureChat v0.83β initialized\n", Color.Gray);
            AppendColoredText("System: Loading encryption modules...\n", Color.Gray);
            AppendColoredText("System: WARNING: Some modules failed checksum verification\n", Color.Yellow);
            
            // Initialize fake progress
            progressBar.Value = random.Next(20, 40);
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                isConnected = true;
                lblStatus.Text = "CONNECTING...";
                lblStatus.ForeColor = Color.Orange;
                btnConnect.Text = "Terminate Connection";
                btnEncrypt.Enabled = true;
                btnSend.Enabled = true;
                
                AddLogMessage("Connection", "Initiating handshake with relay network...");
                AppendColoredText("\n=== Establishing secure connection ===\n", Color.White);
                
                // Fake connection sequence
                progressBar.Style = ProgressBarStyle.Marquee;
                
                // Start the fake chat script after delay
                var connectTimer = new System.Windows.Forms.Timer { Interval = 3000 };
                connectTimer.Tick += (s, args) => {
                    connectTimer.Stop();
                    lblStatus.Text = "CONNECTED";
                    lblStatus.ForeColor = Color.Green;
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Value = 100;
                    AddLogMessage("Connection", "Secure channel established");
                    AppendColoredText("Connection established to relay node #42\n", Color.LightGreen);
                    chatTimer.Start();
                };
                connectTimer.Start();
            }
            else
            {
                isConnected = false;
                lblStatus.Text = "DISCONNECTED";
                lblStatus.ForeColor = Color.Red;
                btnConnect.Text = "Initialize Connection Protocol";
                btnEncrypt.Enabled = false;
                btnSend.Enabled = false;
                chatTimer.Stop();
                AddLogMessage("Connection", "Channel terminated by user");
                AppendColoredText("\n=== Connection terminated ===\n", Color.Red);
            }
        }

        private void BtnEncrypt_Click(object sender, EventArgs e)
        {
            isEncrypted = !isEncrypted;
            btnEncrypt.Text = isEncrypted ? "Disable Encryption" : "Enable Encryption (AES)";
            AddLogMessage("Security", isEncrypted ? 
                "AES-256 encryption enabled (simulated)" : 
                "Encryption disabled - WARNING: Traffic is unencrypted");
            
            AppendColoredText($"Encryption {(isEncrypted ? "ENABLED" : "DISABLED")}\n", 
                isEncrypted ? Color.LightGreen : Color.Orange);
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMessage.Text) && txtMessage.Text != "Enter secure message...")
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                AppendColoredText($"[{time}] You: {txtMessage.Text}\n", Color.Cyan);
                AddLogMessage("Transmit", $"Message queued ({txtMessage.Text.Length} bytes)");
                
                // Fake response after random delay
                var responseTimer = new System.Windows.Forms.Timer { Interval = random.Next(1000, 3000) };
                responseTimer.Tick += (s, args) => {
                    responseTimer.Stop();
                    string[] responses = {
                        "Acknowledged.",
                        "Received. Processing...",
                        "Error: Protocol mismatch. Resend?",
                        "Message authenticated.",
                        "***ENCRYPTION REQUIRED FOR THIS CHANNEL***"
                    };
                    string response = responses[random.Next(responses.Length)];
                    string respTime = DateTime.Now.ToString("HH:mm:ss");
                    AppendColoredText($"[{respTime}] Relay: {response}\n", Color.Yellow);
                };
                responseTimer.Start();
                
                txtMessage.Text = "";
            }
        }

        private void ProcessNextMessage()
        {
            if (messageIndex < fakeChatScript.Count)
            {
                var msg = fakeChatScript[messageIndex];
                Color color = msg[0] == "System" ? Color.Red : 
                             msg[0].Contains("User") ? Color.LightGreen : Color.Orange;
                
                AppendColoredText($"[{msg[2]}] {msg[0]}: {msg[1]}\n", color);
                
                if (msg[0] != "System")
                {
                    AddLogMessage("Receive", $"Message from {msg[0]}");
                }
                
                messageIndex++;
                
                // Randomly stop for dramatic effect
                if (messageIndex == 7 || random.NextDouble() < 0.3)
                {
                    chatTimer.Stop();
                    var resumeTimer = new System.Windows.Forms.Timer { Interval = random.Next(3000, 6000) };
                    resumeTimer.Tick += (s, e) => {
                        resumeTimer.Stop();
                        if (isConnected) chatTimer.Start();
                    };
                    resumeTimer.Start();
                }
            }
            else
            {
                chatTimer.Stop();
            }
        }

        private void UpdateFakeStatus()
        {
            // Fake buffer percentage
            int currentVal = progressBar.Value;
            int change = random.Next(-5, 6);
            int newVal = Math.Max(0, Math.Min(100, currentVal + change));
            progressBar.Value = newVal;
            
            // Occasionally add random log messages
            if (random.NextDouble() < 0.1)
            {
                string[] statusMsgs = {
                    $"Buffer: {newVal}%",
                    $"Heap allocation: {random.Next(100, 500)}KB",
                    $"Threads active: {random.Next(3, 8)}",
                    $"Network latency: {random.Next(20, 200)}ms",
                    $"Memory leak detected in module chat_core.dll"
                };
                AddLogMessage("Status", statusMsgs[random.Next(statusMsgs.Length)]);
            }
        }

        private void AddLogMessage(string source, string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            string entry = $"[{time}] [{source}] {message}";
            lstLog.Items.Add(entry);
            lstLog.TopIndex = lstLog.Items.Count - 1;
            
            // Keep log from growing too large
            if (lstLog.Items.Count > 50)
            {
                lstLog.Items.RemoveAt(0);
            }
        }

        // FIXED METHOD: Properly appends colored text to RichTextBox
        private void AppendColoredText(string text, Color color)
        {
            chatDisplay.SelectionStart = chatDisplay.TextLength;
            chatDisplay.SelectionLength = 0;
            chatDisplay.SelectionColor = color;
            chatDisplay.AppendText(text);
            chatDisplay.SelectionColor = chatDisplay.ForeColor; // Reset to default
            chatDisplay.ScrollToCaret(); // Auto-scroll to bottom
        }

        private void TriggerFakeCrash()
        {
            AddLogMessage("CRITICAL", "Kernel panic triggered via hotkey");
            AppendColoredText("\n*** FATAL SYSTEM ERROR ***\n", Color.Red);
            
            // Create dramatic crash sequence
            var crashForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                WindowState = FormWindowState.Maximized,
                BackColor = Color.Blue,
                TopMost = true,
                ControlBox = false
            };

            var crashText = new Label
            {
                Text = "A problem has been detected and Windows has been shut down to prevent damage\n" +
                       "to your computer.\n\n" +
                       "UNEXPECTED_KERNEL_MODE_TRAP\n\n" +
                       "If this is the first time you've seen this error screen,\n" +
                       "restart your computer. If this screen appears again, follow\n" +
                       "these steps:\n\n" +
                       "Check for viruses on your computer. Remove any newly installed\n" +
                       "hard drives or hard drive controllers. Check your hard drive\n" +
                       "to make sure it is properly configured and terminated.\n\n" +
                       "Technical information:\n\n" +
                       "*** STOP: 0x0000007E (0xC0000005, 0xF86A5A4C, 0xF8971208, 0xF8970F04)\n\n" +
                       "***   chat_core.sys - Address F86A5A4C base at F86A5000, DateStamp 3d7f8c22",
                Font = new Font("Lucida Console", 12),
                ForeColor = Color.White,
                Size = crashForm.ClientSize,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(50)
            };

            crashForm.Controls.Add(crashText);
            crashForm.Show();

            // Make main window freeze
            this.Enabled = false;
            
            // Close the fake crash after 5 seconds
            var timer = new System.Windows.Forms.Timer { Interval = 5000 };
            timer.Tick += (s, e) => {
                timer.Stop();
                crashForm.Close();
                this.Close(); // Close main app too for effect
            };
            timer.Start();
        }

        private void TriggerGlitchEffect()
        {
            AddLogMessage("ANOMALY", "Visual corruption detected");
            AppendColoredText("WARNING: Display subsystem failure\n", Color.Magenta);
            
            // Create glitch effect by rapidly changing window position and colors
            Point originalLocation = this.Location;
            Color originalBackColor = chatDisplay.BackColor;
            Color originalForeColor = chatDisplay.ForeColor;
            
            var glitchTimer = new System.Windows.Forms.Timer { Interval = 50 };
            int glitchCount = 0;
            
            glitchTimer.Tick += (s, e) => {
                glitchCount++;
                
                // Randomly shift window
                this.Location = new Point(
                    originalLocation.X + random.Next(-10, 11),
                    originalLocation.Y + random.Next(-10, 11)
                );
                
                // Randomly change colors
                if (glitchCount % 3 == 0)
                {
                    chatDisplay.BackColor = Color.FromArgb(
                        random.Next(256), 
                        random.Next(256), 
                        random.Next(256)
                    );
                    chatDisplay.ForeColor = Color.FromArgb(
                        random.Next(256), 
                        random.Next(256), 
                        random.Next(256)
                    );
                }
                
                // Occasionally add random characters to chat
                if (glitchCount % 5 == 0)
                {
                    chatDisplay.AppendText("�");
                }
                
                if (glitchCount > 25)
                {
                    glitchTimer.Stop();
                    this.Location = originalLocation;
                    chatDisplay.BackColor = originalBackColor;
                    chatDisplay.ForeColor = originalForeColor;
                    AddLogMessage("System", "Visual systems restored");
                    AppendColoredText("Display subsystem stabilized\n", Color.LightGreen);
                }
            };
            
            glitchTimer.Start();
        }
    }

    // Helper class for fake messages
    public class FakeMessage
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public string Time { get; set; }
    }
}