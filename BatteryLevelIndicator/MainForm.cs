using System;
using System.Drawing;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BatteryLevelIndicator
{
    public partial class MainForm : Form
    {
        double charged = 4.1;       // for one cell. can be 4.1 or 4.2 v.
        double discharged = 3.3;    // cutout
        double voltage;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Hide();
            Opacity = 100;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            formBG = new Bitmap(Size.Width, Size.Height);
            batteryBigImg = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            UpdateVoltage();
            UpdateIcon();
            Opacity = 0;
            timer.Start();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            UpdateForm();
            Show();
            Activate();
            Task.Run(() =>
            {
                UpdateVoltage();
                UpdateIcon();
                DrawBatteryImage(GetLevelByVoltage(voltage));
            });
        }

        private void Form_Deactivate(object sender, EventArgs e)    // focus lost
        {
            Hide();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            UpdateVoltage();
            UpdateIcon();
            if (Visible)
                UpdateForm();
        }

        Bitmap icoImg = new Bitmap(30, 30);
        Pen icoPen = new Pen(Color.White, 2);

        private void UpdateIcon()
        {
            Graphics g = Graphics.FromImage(icoImg);
            g.DrawRectangle(icoPen, 7, 3, 16, 26);
            g.DrawLine(icoPen, 11, 0, 19, 0);
            Color color = GetColor(GetLevelByVoltage(voltage));
            Brush brush = new SolidBrush(color);
            g.FillRectangle(brush, 8, 4, 14, 24);
            Invoke((MethodInvoker)delegate
            {
                Icon = Icon.FromHandle(icoImg.GetHicon());
                notifyIcon1.Icon = Icon;
            });
        }

        private Color GetColor(int v)
        {
            if (v < 25)
                return Color.FromArgb(255, v * 255 / 24, 0, 0);
            v -= 25;
            if (v < 25)
                return Color.FromArgb(255, 255, v * 255 / 24, 0);
            v -= 25;
            if (v < 25)
                return Color.FromArgb(255, (24 - v) * 255 / 24, 255, 0);
            v -= 25;
            return Color.FromArgb(255, 0, (25 - v) * 128 / 25 + 127, v * 255 / 50);
        }

        int batteryImagePercentage = -1;
        Image batteryBigImg;
        Pen bPen = new Pen(Color.White, 3);
        private void DrawBatteryImage(int p)
        {
            if (p == batteryImagePercentage)
                return;
            batteryImagePercentage = p;
            int w = batteryBigImg.Width;
            int h = batteryBigImg.Height;
            Graphics g = Graphics.FromImage(batteryBigImg);
            g.Clear(Color.Transparent);
            g.DrawRectangle(bPen, 1, 1, w - 6, h - 3);
            g.DrawLine(bPen, w - 1, h / 3, w - 1, h / 3 * 2);
            int fillw = (w - 15) * p;
            fillw /= 100;
            g.FillRectangle(Brushes.Green, 6, 6, fillw, h - 12);

            //for (int i = 0; i <= 100; i++)
            //    for (int j = 0; j < 30; j++)
            //        ((Bitmap)batteryBigImg).SetPixel(i, j, GetColor(i));
        }

        Bitmap formBG;
        Pen borderPen = new Pen(Brushes.Black, 2);
        Brush bgBrush = new SolidBrush(Color.FromArgb(224, 0, 0, 16));
        private void UpdateForm()
        {
            var screen = Screen.PrimaryScreen.Bounds;
            int x = screen.Width - Size.Width - screen.Height / 20;
            int y = screen.Height - Size.Height - screen.Height / 20;
            Location = new Point(x, y);
            Graphics g = Graphics.FromImage(formBG);
            g.CopyFromScreen(x, y, 0, 0, formBG.Size);
            g.DrawRectangle(borderPen, 0, 0, formBG.Width, formBG.Height);
            g.FillRectangle(bgBrush, 0, 0, formBG.Width, formBG.Height);
            BackgroundImage = formBG;
            int l = GetLevelByVoltage(voltage);
            DrawBatteryImage(l);
            pictureBox1.Image = batteryBigImg;
            percentageLabel.Text = l + "%";
            voltageLabel.Text = voltage.ToString("0.00") + " V";
        }

        private int GetLevelByVoltage(double v)
        {
            if (v > 5)
                v /= 3; //in laptops there are usually 3 liion cells in sequence

            if (v > charged)
                return 100;

            if (v < discharged)
                return 0;

            return (int)((v - discharged) * (v + discharged) / ((charged - discharged) * (charged + discharged)) * 100);

        }

        private void UpdateVoltage()
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_Battery");
            foreach (ManagementObject mo in mos.Get())
                voltage = double.Parse(mo["DesignVoltage"].ToString()) / 1000;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
