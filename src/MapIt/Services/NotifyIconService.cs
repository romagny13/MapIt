using System.Drawing;
using System.Windows.Forms;

namespace MapIt.Services
{

    public class NotifyIconService : INotifyIconService
    {
        private NotifyIcon _notifyIcon;
        private readonly IKeyMappingService _keyMappingService;
        private readonly IKeyboardHookService _keyboardHookService;

        public NotifyIconService(IKeyMappingService keyMappingService, IKeyboardHookService keyboardHookService)
        {
            _keyMappingService = keyMappingService;
            _keyboardHookService = keyboardHookService;
        }

        public void ShowBalloonTip(string title, string message)
        {
            _notifyIcon.ShowBalloonTip(1000, title, message, ToolTipIcon.Info);
        }

        public void Initialize()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("MapIt.ico"),
                Visible = true,
                Text = "MapIt"
            };

            ShowBalloonTip("MapIt", "The application is running.");

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Update configuration", null, (sender, e) =>
            {
                var keyMapping = _keyMappingService.LoadKeyMappings();
                _keyboardHookService.SetKeyMapping(keyMapping);
            });

            contextMenu.Items.Add(new ToolStripSeparator());

            // Ajouter un bouton pour activer/désactiver l'écoute des touches
            var toggleListeningItem = new ToolStripMenuItem(_keyboardHookService.IsListening ? "Disable key listening" : "Enable key listening");
            toggleListeningItem.Click += (sender, e) =>
            {
                if (_keyboardHookService.IsListening)
                {
                    // Désactiver l'écoute des touches
                    _keyboardHookService.StopListening();
                    toggleListeningItem.Text = "Enable key listening";
                }
                else
                {
                    // Activer l'écoute des touches
                    _keyboardHookService.StartListening();
                    toggleListeningItem.Text = "Disable key listening";
                }
            };
            contextMenu.Items.Add(toggleListeningItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            contextMenu.Items.Add("Exit", null, (sender, e) => Application.Exit());

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
        }
    }


}
