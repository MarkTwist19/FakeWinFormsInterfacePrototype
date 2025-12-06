using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FakeSoftwareInterface
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Show splash screen first
            ShowSplashScreen();
            
            // Then run the main application
            Application.Run(new FakeChatClient());
        }
        
        static void ShowSplashScreen()
        {
            var splash = new Form
            {
                Size = new Size(400, 200),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(240, 240, 240),
                ShowInTaskbar = false
            };
            
            // Add a title
            var titleLabel = new Label
            {
                Text = "SecureChat v0.83β",
                Font = new Font("Microsoft Sans Serif", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 20),
                AutoSize = true
            };
            
            // Add loading message
            var loadingLabel = new Label
            {
                Text = "Initializing secure modules...",
                Location = new Point(20, 70),
                AutoSize = true
            };
            
            // Add fake progress bar
            var progressBar = new ProgressBar
            {
                Location = new Point(20, 100),
                Size = new Size(350, 20),
                Style = ProgressBarStyle.Marquee
            };
            
            // Add version info (amateur touch)
            var versionLabel = new Label
            {
                Text = "Build: 2011-08-23 | UNSTABLE DEVELOPMENT VERSION",
                Font = new Font("Microsoft Sans Serif", 7),
                ForeColor = Color.DarkRed,
                Location = new Point(20, 140),
                AutoSize = true
            };
            
            // Add warning
            var warningLabel = new Label
            {
                Text = "WARNING: Not for production use",
                Font = new Font("Microsoft Sans Serif", 8, FontStyle.Italic),
                ForeColor = Color.Red,
                Location = new Point(20, 160),
                AutoSize = true
            };
            
            splash.Controls.AddRange(new Control[] { 
                titleLabel, loadingLabel, progressBar, versionLabel, warningLabel 
            });
            
            // Show the splash form
            splash.Show();
            
            // Force the splash to paint immediately
            Application.DoEvents();
            
            // Simulate loading time with fake progress
            SimulateLoading(progressBar, loadingLabel);
            
            // Close splash after delay
            Thread.Sleep(2500);
            splash.Close();
        }
        
        static void SimulateLoading(ProgressBar progressBar, Label loadingLabel)
        {
            // Fake loading sequence
            string[] loadingSteps = {
                "Loading encryption modules...",
                "Initializing network stack...",
                "Checking system integrity...",
                "Verifying license (skipped)...",
                "Allocating secure memory...",
                "WARNING: Some DLLs failed checksum",
                "Loading configuration...",
                "Ready"
            };
            
            // Simulate step-by-step loading
            int stepDelay = 300; // ms per step
            
            for (int i = 0; i < loadingSteps.Length; i++)
            {
                // Update label text
                if (loadingLabel.InvokeRequired)
                {
                    loadingLabel.Invoke(new Action(() => 
                    {
                        loadingLabel.Text = loadingSteps[i];
                    }));
                }
                else
                {
                    loadingLabel.Text = loadingSteps[i];
                }
                
                // Update progress bar
                if (progressBar.InvokeRequired)
                {
                    progressBar.Invoke(new Action(() => 
                    {
                        if (progressBar.Style == ProgressBarStyle.Marquee)
                        {
                            progressBar.Style = ProgressBarStyle.Continuous;
                        }
                        progressBar.Value = (int)((i + 1) * (100.0 / loadingSteps.Length));
                    }));
                }
                else
                {
                    if (progressBar.Style == ProgressBarStyle.Marquee)
                    {
                        progressBar.Style = ProgressBarStyle.Continuous;
                    }
                    progressBar.Value = (int)((i + 1) * (100.0 / loadingSteps.Length));
                }
                
                // Force UI update
                Application.DoEvents();
                Thread.Sleep(stepDelay);
            }
        }
    }
}