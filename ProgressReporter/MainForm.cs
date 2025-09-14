using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProgressReporter
{
    public partial class MainForm : Form
    {
        private string ReportFileName;
        
        // UE5 Color Scheme
        private readonly Color UE5_BACKGROUND = Color.FromArgb(40, 40, 40);
        private readonly Color UE5_PANEL_BACKGROUND = Color.FromArgb(56, 56, 56);
        private readonly Color UE5_PROGRESS_BACKGROUND = Color.FromArgb(30, 30, 30);
        private readonly Color UE5_PROGRESS_FOREGROUND = Color.FromArgb(0, 142, 255);
        private readonly Color UE5_TEXT_PRIMARY = Color.FromArgb(200, 200, 200);
        private readonly Color UE5_TEXT_SECONDARY = Color.FromArgb(150, 150, 150);
        private readonly Color UE5_BORDER = Color.FromArgb(20, 20, 20);

        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public MainForm(string[] args)
        {
            ReportFileName = args[0];
            InitializeComponent();
            ApplyUE5Theme();
        }

        private void ApplyUE5Theme()
        {
            // Main form styling
            this.BackColor = UE5_BACKGROUND;
            this.ForeColor = UE5_TEXT_PRIMARY;
            
            // Window chrome customization
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Add custom border
            //this.Paint += MainForm_Paint;
            
            // Enable dragging
            this.MouseDown += Form_MouseDown;
            
            // Style the group boxes
            StyleGroupBox(CurrentTaskBox);
            StyleGroupBox(OverallTaskBox);
            
            // Style progress bars
            StyleProgressBar(CurrentProgressBar);
            StyleProgressBar(OverallProgressBar);
            
            // Add close button
            AddCustomTitleBar();
        }

        private void AddCustomTitleBar()
        {
            // Title bar panel
            Panel titleBar = new Panel();
            titleBar.BackColor = Color.FromArgb(30, 30, 30);
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 32;
            titleBar.MouseDown += Form_MouseDown;
            
            // Title label
            Label titleLabel = new Label();
            titleLabel.Text = "GPU Lightmass Progress";
            titleLabel.ForeColor = UE5_TEXT_PRIMARY;
            titleLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            titleLabel.Location = new Point(10, 6);
            titleLabel.AutoSize = true;
            titleLabel.MouseDown += Form_MouseDown;
            
            // Close button
            Button closeButton = new Button();
            closeButton.Text = "✕";
            closeButton.ForeColor = UE5_TEXT_SECONDARY;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
            closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, 70, 70);
            closeButton.Size = new Size(32, 32);
            closeButton.Location = new Point(this.Width - 32, 0);
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.Font = new Font("Segoe UI", 12F);
            closeButton.Click += (s, e) => Application.Exit();
            closeButton.MouseEnter += (s, e) => closeButton.ForeColor = Color.White;
            closeButton.MouseLeave += (s, e) => closeButton.ForeColor = UE5_TEXT_SECONDARY;
            
            // Minimize button
            Button minimizeButton = new Button();
            minimizeButton.Text = "—";
            minimizeButton.ForeColor = UE5_TEXT_SECONDARY;
            minimizeButton.FlatStyle = FlatStyle.Flat;
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
            minimizeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, 70, 70);
            minimizeButton.Size = new Size(32, 32);
            minimizeButton.Location = new Point(this.Width - 64, 0);
            minimizeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            minimizeButton.Font = new Font("Segoe UI", 12F);
            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            minimizeButton.MouseEnter += (s, e) => minimizeButton.ForeColor = Color.White;
            minimizeButton.MouseLeave += (s, e) => minimizeButton.ForeColor = UE5_TEXT_SECONDARY;
            
            titleBar.Controls.Add(titleLabel);
            titleBar.Controls.Add(closeButton);
            titleBar.Controls.Add(minimizeButton);
            
            this.Controls.Add(titleBar);
            
            // Adjust other controls position
            CurrentTaskBox.Top += 32;
            OverallTaskBox.Top += 32;
            this.Height += 32;
        }

        private void StyleGroupBox(GroupBox groupBox)
        {
            groupBox.ForeColor = UE5_TEXT_PRIMARY;
            groupBox.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            groupBox.FlatStyle = FlatStyle.Flat;
            
            // Custom paint for group box
            groupBox.Paint += (sender, e) =>
            {
                GroupBox box = sender as GroupBox;
                e.Graphics.Clear(UE5_PANEL_BACKGROUND);
                
                // Draw custom border
                using (Pen borderPen = new Pen(UE5_BORDER, 0))
                {
                    Rectangle rect = new Rectangle(0, 7, box.Width - 1, box.Height - 8);
                    e.Graphics.DrawRectangle(borderPen, rect);
                }
                
                // Draw text background
                SizeF textSize = e.Graphics.MeasureString(box.Text, box.Font);
                Rectangle textRect = new Rectangle(10, 0, (int)textSize.Width + 6, (int)textSize.Height);
                e.Graphics.FillRectangle(new SolidBrush(UE5_PANEL_BACKGROUND), textRect);
                
                // Draw text
                using (Brush textBrush = new SolidBrush(UE5_TEXT_PRIMARY))
                {
                    e.Graphics.DrawString(box.Text, box.Font, textBrush, 13, 0);
                }
            };
        }

        private void StyleProgressBar(ProgressBar progressBar)
        {
            // Since we can't directly style ProgressBar, we'll create a custom one
            progressBar.Visible = false;
            
            Panel customProgress = new Panel();
            customProgress.Name = progressBar.Name + "_Custom";
            customProgress.Location = progressBar.Location;
            customProgress.Size = progressBar.Size;
            customProgress.BackColor = UE5_PROGRESS_BACKGROUND;
            customProgress.Tag = progressBar; // Store reference to original progress bar
            
            Panel progressFill = new Panel();
            progressFill.Name = progressBar.Name + "_Fill";
            progressFill.Location = new Point(0, 0);
            progressFill.Height = customProgress.Height;
            progressFill.BackColor = UE5_PROGRESS_FOREGROUND;
            progressFill.Width = 0; // Start with 0 width
            
            customProgress.Controls.Add(progressFill);
            progressBar.Parent.Controls.Add(customProgress);
            
            // Add glow effect on paint
            customProgress.Paint += (sender, e) =>
            {
                Panel panel = sender as Panel;
                ProgressBar linkedBar = panel.Tag as ProgressBar;
                
                // Find the fill panel by iterating through controls
                Panel fill = null;
                foreach (Control control in panel.Controls)
                {
                    if (control is Panel && control.Name.EndsWith("_Fill"))
                    {
                        fill = control as Panel;
                        break;
                    }
                }
                
                if (fill != null && linkedBar != null)
                {
                    int fillWidth = (int)((panel.Width * linkedBar.Value) / 100.0);
                    fill.Width = fillWidth;
                    
                    // Draw subtle glow effect at the end of progress
                    if (linkedBar.Value > 0 && linkedBar.Value < 100 && fillWidth > 20)
                    {
                        try
                        {
                            using (LinearGradientBrush glowBrush = new LinearGradientBrush(
                                new Rectangle(Math.Max(0, fillWidth - 20), 0, Math.Min(20, fillWidth), panel.Height),
                                Color.FromArgb(100, UE5_PROGRESS_FOREGROUND),
                                UE5_PROGRESS_FOREGROUND,
                                LinearGradientMode.Horizontal))
                            {
                                e.Graphics.FillRectangle(glowBrush, Math.Max(0, fillWidth - 20), 0, Math.Min(20, fillWidth), panel.Height);
                            }
                        }
                        catch
                        {
                            // Ignore gradient errors for very small values
                        }
                    }
                }
                
                // Draw border (subtle, optional)
                using (Pen borderPen = new Pen(Color.FromArgb(60, 60, 60), 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, panel.Width - 1, panel.Height - 1);
                }
            };
        }

        // Removed the MainForm_Paint method since we no longer need the external border

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            // Enable window dragging
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        // Windows API for dragging
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Process[] pname = Process.GetProcessesByName("UnrealLightmass");
                if(pname.Length == 0)
                    Application.Exit();

                string[] lines = System.IO.File.ReadAllLines(ReportFileName);
                CurrentTaskBox.Text = lines[0];
                CurrentProgressBar.Value = Clamp(int.Parse(lines[1]), 0, 100);
                OverallTaskBox.Text = lines[2];
                OverallProgressBar.Value = Clamp(int.Parse(lines[3]), 0, 100);
                
                // Force redraw of custom progress bars
                foreach (Control control in CurrentTaskBox.Controls)
                {
                    if (control is Panel && control.Tag is ProgressBar)
                        control.Invalidate();
                }
                foreach (Control control in OverallTaskBox.Controls)
                {
                    if (control is Panel && control.Tag is ProgressBar)
                        control.Invalidate();
                }
            }
            catch (FileNotFoundException)
            {
                Application.Exit();
            }
            catch(IOException)
            {
                // Silently handle IO exceptions (file being written)
            }
        }
    }
}