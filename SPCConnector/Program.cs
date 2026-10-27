using SPCConnector.Classes;
using SPCConnector.UI;

namespace SPCConnector
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
#if LOGACTIVE
            Logger.Initialize();
#endif
            if (!TryResolveMode(args, out Mode mode))
            {
#if LOGACTIVE
                Logger.Log($"SPCConnector için geçersiz parametreler: {string.Join(" ", args)}");
#endif
                return;
            }

            try
            {
                switch (mode)
                {
                    case Mode.Scan:
                        ConnectionScanner.Start(args[1]);
                        break;
                    case Mode.TakeSnapshot:
                        CameraSnapshots.Start(args);
                        break;
                    case Mode.Discovery:
                        CameraDiscovery.Start();
                        break;
                    case Mode.GetProfiles:
                        CameraProfiles.Start(args);
                        break;
                    case Mode.Connect:
                        ApplicationConfiguration.Initialize();
                        Application.Run(new Form_Main(args));
                        break;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
#if LOGACTIVE
                Logger.Log($"SPCConnector başlatılırken hata oluştu: {ex}");
#endif
                if (mode == Mode.Connect)
                    MessageBox.Show($"Kamera işlemi başlatılamadı.\n{ex.Message}", "ONVIF bağlantı hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool TryResolveMode(string[] args, out Mode mode)
        {
            mode = default;
            if (args.Length == 0 || !Enum.TryParse(args[0], ignoreCase: false, out mode) || !Enum.IsDefined(mode))
                return false;

            return mode switch
            {
                Mode.Connect => args.Length >= 1,
                Mode.Scan => args.Length >= 2 && int.TryParse(args[1], out int cameraCount) && cameraCount >= 0,
                Mode.TakeSnapshot => args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1]),
                Mode.Discovery => true,
                Mode.GetProfiles => args.Length >= 5 && int.TryParse(args[2], out int port) && port is > 0 and <= 65535,
                _ => false
            };
        }
    }
}
