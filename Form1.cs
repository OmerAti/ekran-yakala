using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ekran_Al
{
    public partial class Form1 : Form
    {
        private NotifyIcon notifyIcon;
        private ToolStripMenuItem copyUrlMenuItem;
        private string lastUploadedUrl;
        private Form urlForm;


        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);


        private const int MY_HOTKEY_ID = 1;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_SNAPSHOT = 0x2C;

        public Form1()
        {
            InitializeComponent();
            this.ShowInTaskbar = false; 
            this.FormBorderStyle = FormBorderStyle.None; 

       
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Information;
            notifyIcon.Visible = true;
            WindowsFormsSynchronizationContext.Current.Post((obj) => this.Hide(), null);

  
            notifyIcon.Click += NotifyIcon_Click;


            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem uploadMenuItem = new ToolStripMenuItem("Ekran Yakala");
            uploadMenuItem.Click += UploadMenuItem_Click;
            contextMenu.Items.Add(uploadMenuItem);

  
            copyUrlMenuItem = new ToolStripMenuItem("En Son URL Kopyala");
            copyUrlMenuItem.Click += CopyUrlMenuItem_Click;
            copyUrlMenuItem.Enabled = false; 
            contextMenu.Items.Add(copyUrlMenuItem);


            ToolStripMenuItem aboutMenuItem = new ToolStripMenuItem("Hakkımızda");
            aboutMenuItem.Click += AboutMenuItem_Click;
            contextMenu.Items.Add(aboutMenuItem);

            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Çıkış Yap");
            exitMenuItem.Click += ExitMenuItem_Click;
            contextMenu.Items.Add(exitMenuItem);

            notifyIcon.ContextMenuStrip = contextMenu;


            RegisterHotKey(this.Handle, MY_HOTKEY_ID, MOD_ALT, VK_SNAPSHOT);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Visible = false; 
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {

        }

        private async void CopyUrlMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastUploadedUrl))
            {
                Clipboard.SetText(lastUploadedUrl);
                ShowNotification("URL kopyalandı.");
            }
        }

        private void UploadMenuItem_Click(object sender, EventArgs e)
        {
            CaptureSelectedScreen();
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Bu uygulama ekran görüntüsü yakalamak ve yüklemek için kullanılıyor. Omer ATABER - OmerAti", "Hakkımızda", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MY_HOTKEY_ID)
            {
                CaptureSelectedScreen();
            }
        }

        private async void CaptureSelectedScreen()
        {
            try
            {
                using (var selectionForm = new ScreenSelectionForm())
                {
                    selectionForm.StartPosition = FormStartPosition.Manual;
                    selectionForm.Location = new Point(0, 0);
                    selectionForm.Size = Screen.PrimaryScreen.Bounds.Size;

                    if (selectionForm.ShowDialog() == DialogResult.OK)
                    {
                        Rectangle selectionBounds = selectionForm.SelectedBounds;
                        Bitmap screenshot = CaptureScreen(selectionBounds);
                        string imageUrl = await UploadImage(screenshot);
                        ShowUrlForm(imageUrl);
                     lastUploadedUrl = imageUrl;
                        copyUrlMenuItem.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowUrlForm(string imageUrl)
        {
            if (urlForm == null || urlForm.IsDisposed)
            {
                urlForm = new Form();
                urlForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                urlForm.Size = new Size(400, 120); 
                urlForm.StartPosition = FormStartPosition.Manual;
                urlForm.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - urlForm.Width - 10,
                                             Screen.PrimaryScreen.WorkingArea.Height - urlForm.Height - 10);
                urlForm.Text = "Resim Yüklendi";
                TextBox urlTextBox = new TextBox();
                urlTextBox.Text = imageUrl;
                urlTextBox.ReadOnly = true;
                urlTextBox.Location = new Point(10, 10);
                urlTextBox.Width = urlForm.ClientSize.Width - 20;
                urlForm.Controls.Add(urlTextBox);
                Panel buttonPanel = new Panel();
                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 40; 
                urlForm.Controls.Add(buttonPanel);
                Button copyRawButton = new Button();
                copyRawButton.Text = "Ham Kopyala";
                copyRawButton.AutoSize = true;
                copyRawButton.Location = new Point(10, 5); 
                copyRawButton.Click += (sender, e) =>
                {
                    Clipboard.SetText(imageUrl);
                    ShowNotification("URL kopyalandı.");
                    urlForm.Close();
                };
                buttonPanel.Controls.Add(copyRawButton);
                Button copyAdButton = new Button();
                copyAdButton.Text = "Reklamlı Kopyala";
                copyAdButton.AutoSize = true;
                copyAdButton.Location = new Point(copyRawButton.Right + 10, 5); 
                copyAdButton.Click += (sender, e) =>
                {
                    string adUrl = $"https://screenshot.domainadi.com/uploads/index.php?encid={Path.GetFileName(imageUrl)}";
                    Clipboard.SetText(adUrl);
                    ShowNotification("Reklamlı URL kopyalandı.");
                    urlForm.Close();
                };
                buttonPanel.Controls.Add(copyAdButton);

                urlForm.Show();
            }
            else
            {
                urlForm.BringToFront();
                urlForm.Focus();
            }
        }




        private void ShowNotification(string message)
        {
            notifyIcon.BalloonTipText = message;
            notifyIcon.ShowBalloonTip(3000);
        }

        static Bitmap CaptureScreen(Rectangle bounds)
        {
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(screenshot))
            {
                graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }
            return screenshot;
        }

        static async Task<string> UploadImage(Bitmap image)
        {
            using (var client = new HttpClient())
            {
                string uploadUrl = "https://screenshot.domainadi.com/uploadim.php";

                using (var content = new MultipartFormDataContent())
                {
                    string uniqueFileName = Path.GetRandomFileName().Replace(".", "");
                    using (var stream = new MemoryStream())
                    {
                        image.Save(stream, ImageFormat.Png);
                        stream.Position = 0;

                        content.Add(new StreamContent(stream), "file", $"{uniqueFileName}.png");
                        var response = await client.PostAsync(uploadUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();

                            dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);
                            string imageUrl = jsonResponse.url; 

                            return imageUrl.Trim();
                        }
                        else
                        {
                            throw new HttpRequestException($"HTTP error: {response.StatusCode}");
                        }
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, MY_HOTKEY_ID);
            notifyIcon.Dispose();
        }
    }

    public class ScreenSelectionForm : Form
    {
        private Point selectionStart;
        private Rectangle selectedBounds;

        public Rectangle SelectedBounds => selectedBounds;

        public ScreenSelectionForm()
        {
            this.BackColor = Color.White;
            this.Opacity = 0.5;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
            this.TopMost = true;
            this.Cursor = Cursors.Cross;

            this.Paint += ScreenSelectionForm_Paint;
            this.MouseDown += ScreenSelectionForm_MouseDown;
            this.MouseMove += ScreenSelectionForm_MouseMove;
            this.MouseUp += ScreenSelectionForm_MouseUp;

            // Yükleme butonu oluşturma
            Button uploadButton = new Button();
            uploadButton.Text = "Yükle";
            uploadButton.AutoSize = true;
            uploadButton.Location = new Point(10, 10);
            uploadButton.Click += UploadButton_Click;
            this.Controls.Add(uploadButton);
        }

        private void ScreenSelectionForm_Paint(object sender, PaintEventArgs e)
        {
            if (selectedBounds != Rectangle.Empty)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(128, Color.Blue)))
                {
                    e.Graphics.FillRectangle(brush, selectedBounds);
                }
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectedBounds);
                }
            }
        }

        private void ScreenSelectionForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                selectionStart = e.Location;
                selectedBounds = new Rectangle();
                this.Invalidate();
            }
        }

        private void ScreenSelectionForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x = Math.Min(selectionStart.X, e.X);
                int y = Math.Min(selectionStart.Y, e.Y);
                int width = Math.Abs(selectionStart.X - e.X);
                int height = Math.Abs(selectionStart.Y - e.Y);

                selectedBounds = new Rectangle(x, y, width, height);
                this.Invalidate();
            }
        }

        private void ScreenSelectionForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (selectedBounds.Width > 0 && selectedBounds.Height > 0)
                {
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                }
                this.Close();
            }
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
