namespace SystemSoundsVolumeTray
{
    public class TrayAppContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private VolumePopupForm? _volumeForm;
        private readonly AppConfig _config;
        private readonly System.Windows.Forms.Timer _syncTimer;

        public TrayAppContext()
        {
            _config = AppConfig.Load();

            var contextMenu = new ContextMenuStrip();
			var iconStream = typeof(Program).Assembly.GetManifestResourceStream("SystemSoundsVolumeTray.app.ico");
            contextMenu.Items.Add("Show Volume Level", null, OnToggleVolumeClick);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Exit", null, OnExit);

            _trayIcon = new NotifyIcon()
            {
                Icon = new Icon(iconStream),
                ContextMenuStrip = contextMenu,
                Visible = true
            };
            
            _trayIcon.Click += OnTrayIconClick;

            _syncTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            _syncTimer.Tick += OnSyncTimerTick;
            _syncTimer.Start();
            
            OnSyncTimerTick(this, EventArgs.Empty);
        }

        private void OnToggleVolumeClick(object? sender, EventArgs e)
        {
            ToggleVolumeForm(Cursor.Position);
        }

        private void OnTrayIconClick(object? sender, EventArgs e)
        {
            if (e is MouseEventArgs mouseArgs && mouseArgs.Button == MouseButtons.Left)
            {
                ToggleVolumeForm(Cursor.Position);
            }
        }

        private void ToggleVolumeForm(Point clickPosition)
        {
            if (_volumeForm == null || _volumeForm.IsDisposed)
            {
                _volumeForm = new VolumePopupForm(_config, clickPosition);
                _volumeForm.FormClosed += (s, args) => _volumeForm = null;
                _volumeForm.Show();
            }
            else
            {
                _volumeForm.Close();
            }
        }
        
        private void OnSyncTimerTick(object? sender, EventArgs e)
        {
            CoreAudioController.SetSystemSoundsVolume(_config.Volume);
        }

        private void OnExit(object? sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _syncTimer.Stop();
            _syncTimer.Dispose();
            CoreAudioController.ReleaseVolumeControl();
            Application.Exit();
        }
    }
}
