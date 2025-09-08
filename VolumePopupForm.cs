using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace SystemSoundsVolumeTray
{
    public partial class VolumePopupForm : Form
    {
        private readonly TrackBar _volumeSlider;
        private readonly AppConfig _config;
        private readonly Point _clickPosition;
        private readonly Label _currentValueLabel;

        private const int WM_SETTINGCHANGE = 0x001A;

        public VolumePopupForm(AppConfig config, Point clickPosition)
        {
            _config = config;
            _clickPosition = clickPosition;

            TopMost = true;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(220, 65);

            _currentValueLabel = new Label
            {
                AutoSize = true,
                Location = new Point(10, 10),
                Font = new Font(this.Font.FontFamily, 9F, FontStyle.Bold)
            };

            _volumeSlider = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Orientation = Orientation.Horizontal,
                TickStyle = TickStyle.None,
                AutoSize = false,
                Size = new Size(this.ClientSize.Width - 20, 30),
                Location = new Point(10, _currentValueLabel.Bottom),
                Value = _config.Volume
            };

            UpdateCurrentValueLabel();

            Controls.Add(_currentValueLabel);
            Controls.Add(_volumeSlider);
            
            _volumeSlider.ValueChanged += OnVolumeChanged;

            SetPosition();
            ApplyTheme();
        }

        private void UpdateCurrentValueLabel()
        {
            _currentValueLabel.Text = $"Volume: {_volumeSlider.Value}%";
            _currentValueLabel.Left = (this.ClientSize.Width - _currentValueLabel.Width) / 2;
        }

        private void SetPosition()
        {
            Screen screen = Screen.FromPoint(_clickPosition);
            Rectangle workingArea = screen.WorkingArea;
            
            int left = _clickPosition.X - (this.Width / 2);
            int top = _clickPosition.Y - this.Height - 5;

            if (left < workingArea.Left)
            {
                left = workingArea.Left;
            }
            if (left + this.Width > workingArea.Right)
            {
                left = workingArea.Right - this.Width;
            }

            if (top < workingArea.Top)
            {
                top = _clickPosition.Y + 10;
            }
            
            this.Location = new Point(left, top);
        }

        private void OnVolumeChanged(object? sender, EventArgs e)
        {
            _config.Volume = _volumeSlider.Value;
            _config.Save();
            CoreAudioController.SetSystemSoundsVolume(_config.Volume);
            UpdateCurrentValueLabel();
        }
        
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            this.Close();
        }

        #region Theming

        private void ApplyTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var useLightTheme = (int?)key?.GetValue("AppsUseLightTheme") ?? 1;

                Color backColor, foreColor;
                if (useLightTheme == 0) // Dark theme
                {
                    backColor = Color.FromArgb(43, 43, 43);
                    foreColor = Color.White;
                }
                else // Light theme
                {
                    backColor = SystemColors.Control;
                    foreColor = SystemColors.ControlText;
                }

                this.BackColor = backColor;
                this.ForeColor = foreColor;
                _volumeSlider.BackColor = backColor;
                _currentValueLabel.ForeColor = foreColor;
            }
            catch { /* Ignore errors */ }
        }
        
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SETTINGCHANGE)
            {
                string? param = Marshal.PtrToStringAuto(m.LParam);
                if (param == "ImmersiveColorSet")
                {
                    ApplyTheme();
                }
            }
            base.WndProc(ref m);
        }

        #endregion
    }
}
