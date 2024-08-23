using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Net.NetworkInformation;
using System.Timers;
using System.Windows.Forms;

namespace UptimeCheck
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelPingResult;
        private System.Windows.Forms.Button buttonOpenLog;
        private System.Timers.Timer pingTimer;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;

        private string logFilePath = "PingLog.txt";

        public Form1()
        {
            InitializeComponent();
            InitializePingMonitor();
        }

        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelPingResult = new System.Windows.Forms.Label();
            this.buttonOpenLog = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(50, 50);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(50, 50);
            this.panel1.BackColor = System.Drawing.Color.Green;
            this.Controls.Add(this.panel1);
            // 
            // labelPingResult
            // 
            this.labelPingResult.AutoSize = true;
            this.labelPingResult.Location = new System.Drawing.Point(50, 120);
            this.labelPingResult.Name = "labelPingResult";
            this.labelPingResult.Size = new System.Drawing.Size(0, 13);
            this.labelPingResult.TabIndex = 1;
            this.Controls.Add(this.labelPingResult);
            // 
            // buttonOpenLog
            // 
            this.buttonOpenLog.Location = new System.Drawing.Point(50, 150);
            this.buttonOpenLog.Name = "buttonOpenLog";
            this.buttonOpenLog.Size = new System.Drawing.Size(100, 30);
            this.buttonOpenLog.Text = "Open Log";
            this.buttonOpenLog.UseVisualStyleBackColor = true;
            this.buttonOpenLog.Click += new System.EventHandler(this.ButtonOpenLog_Click);
            this.Controls.Add(this.buttonOpenLog);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Text = "Simple Uptime Monitor";
            this.notifyIcon.Icon = SystemIcons.Application;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.NotifyIcon_DoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new ToolStripItem[] { this.exitToolStripMenuItem });
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "Form1";
            this.Text = "Simple Uptime Monitor";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void InitializePingMonitor()
        {
            pingTimer = new System.Timers.Timer(25000); // 25 seconds
            pingTimer.Elapsed += PingTimer_Elapsed;
            pingTimer.AutoReset = true;
            pingTimer.Enabled = true;

            // Optional: Start the ping immediately
            PingTimer_Elapsed(this, null);
        }

        private void PingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string host = "google.com";
            Ping ping = new Ping();
            PingReply reply = null;

            try
            {
                reply = ping.Send(host);
            }
            catch (PingException ex)
            {
                string errorMessage = $"Ping failed: {ex.Message}";
                SystemSounds.Beep.Play();
                SetPanelColor(Color.Red);
                PlayAlertSound();
                LogEvent(errorMessage);
                return; // Exit the method early since the ping failed.
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred: {ex.Message}";
                UpdateLabelText(errorMessage);
                SetPanelColor(Color.Red);
                PlayAlertSound();
                LogEvent(errorMessage);
                return; // Exit the method early since the ping failed.
            }

            if (reply != null && reply.Status == IPStatus.Success)
            {
                string result = $"Ping to {host}: {reply.RoundtripTime}ms";
                UpdateLabelText(result);
                LogEvent(result);

                if (reply.RoundtripTime < 400)
                {
                    SetPanelColor(Color.Green);
                }
                else
                {
                    SetPanelColor(Color.Red);
                    PlayAlertSound();
                    LogEvent($"Ping exceeded 400ms: {reply.RoundtripTime}ms");
                }
            }
            else
            {
                string result = $"Ping to {host} failed.";
                UpdateLabelText(result);
                SetPanelColor(Color.Red);
                PlayAlertSound();
                LogEvent(result);
            }
        }

        private void UpdateLabelText(string text)
        {
            if (labelPingResult.InvokeRequired)
            {
                labelPingResult.Invoke(new Action(() => labelPingResult.Text = text));
            }
            else
            {
                labelPingResult.Text = text;
            }
        }

        private void SetPanelColor(Color color)
        {
            if (panel1.InvokeRequired)
            {
                panel1.Invoke(new Action(() => panel1.BackColor = color));
            }
            else
            {
                panel1.BackColor = color;
            }
        }

        private void PlayAlertSound()
        {
            SystemSounds.Beep.Play();
        }

        private void LogEvent(string message)
        {
            string logMessage = $"{DateTime.Now}: {message}";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }

        private void ButtonOpenLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(logFilePath))
            {
                System.Diagnostics.Process.Start("notepad.exe", logFilePath);
            }
            else
            {
                MessageBox.Show("Log file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
