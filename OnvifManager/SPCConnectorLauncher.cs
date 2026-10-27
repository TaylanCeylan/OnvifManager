using OnvifManager.Models;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Json;

namespace OnvifManager
{
    public sealed class SPCConnectorLauncher
    {
        private static MemoryMappedFile? _persistentLiveMmf;
        private readonly string _connectorPath;

        public SPCConnectorLauncher(string connectorExePath)
        {
            _connectorPath = connectorExePath;
        }

        public Process LaunchLiveView(string ip, int port, string user, string pass, string profile, string locationText, string language = "tr")
        {
            const string mmfName = "SPCameraViewer";
            var cameraData = new Tuple<int, string, int, string, string, string, string>(1, ip, port, user, pass, profile, locationText);
            byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cameraData));

            _persistentLiveMmf?.Dispose();
            _persistentLiveMmf = MemoryMappedFile.CreateOrOpen(mmfName, Math.Max(65536, payload.Length + sizeof(int)));
            WritePayload(_persistentLiveMmf, payload);

            return StartProcess(new[] { "0", language }, showWindow: true);
        }

        public async Task<List<Tuple<string, int>>> RunConnectionScannerAsync(IList<CameraDevice> cameras)
        {
            const string mmfName = "SPCameraTester";
            var input = cameras.Select(camera => Tuple.Create(camera.Ip, camera.Port, camera.Username, camera.Password)).ToList();
            byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(input));

            using var mmf = MemoryMappedFile.CreateOrOpen(mmfName, Math.Max(65536, payload.Length + sizeof(int)));
            WritePayload(mmf, payload);
            using var process = StartProcess(new[] { "1", cameras.Count.ToString() });
            await WaitForExitAsync(process, TimeSpan.FromMinutes(5));
            return ReadPayload<List<Tuple<string, int>>>(mmf) ?? new();
        }

        public async Task<List<Tuple<int, string>>> TakeSnapshotsAsync(string eventId, IList<CameraDevice> cameras)
        {
            string mmfName = $"SPSnapshot{eventId}";
            var input = cameras.Select((camera, index) => Tuple.Create(index + 1, camera.Ip, camera.Port, camera.Username, camera.Password, string.Empty)).ToList();
            byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(input));

            using var mmf = MemoryMappedFile.CreateOrOpen(mmfName, Math.Max(65536, payload.Length + sizeof(int)));
            WritePayload(mmf, payload);
            using var process = StartProcess(new[] { "2", eventId });
            await WaitForExitAsync(process, TimeSpan.FromMinutes(5));
            return ReadPayload<List<Tuple<int, string>>>(mmf) ?? new();
        }

        public async Task<List<Tuple<string, string, int>>> DiscoverNetworkCamerasAsync()
        {
            const string mmfName = "SPCameraDiscovery";
            using var mmf = MemoryMappedFile.CreateOrOpen(mmfName, 65536);
            ClearPayload(mmf);
            using var process = StartProcess(new[] { "3" });
            await WaitForExitAsync(process, TimeSpan.FromSeconds(45));
            return ReadPayload<List<Tuple<string, string, int>>>(mmf) ?? new();
        }

        public async Task<List<Tuple<string, string>>> GetCameraProfilesAsync(string ip, int port, string username, string password)
        {
            const string mmfName = "SPCameraProfiles_Output";
            using var mmf = MemoryMappedFile.CreateOrOpen(mmfName, 65536);
            ClearPayload(mmf);
            using var process = StartProcess(new[] { "4", ip, port.ToString(), username, password });
            await WaitForExitAsync(process, TimeSpan.FromSeconds(45));
            return ReadPayload<List<Tuple<string, string>>>(mmf) ?? new();
        }

        private Process StartProcess(IEnumerable<string> arguments, bool showWindow = false)
        {
            if (!File.Exists(_connectorPath))
                throw new FileNotFoundException($"SPCConnector.exe bulunamadı: {_connectorPath}", _connectorPath);

            string connectorDirectory = Path.GetDirectoryName(Path.GetFullPath(_connectorPath))!;
            var startInfo = new ProcessStartInfo(_connectorPath)
            {
                UseShellExecute = false,
                CreateNoWindow = !showWindow,
                WorkingDirectory = connectorDirectory
            };

            foreach (string argument in arguments)
                startInfo.ArgumentList.Add(argument);

            return Process.Start(startInfo) ?? throw new InvalidOperationException("SPCConnector başlatılamadı.");
        }

        private static async Task WaitForExitAsync(Process process, TimeSpan timeout)
        {
            using var cancellation = new CancellationTokenSource(timeout);
            try
            {
                await process.WaitForExitAsync(cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill(entireProcessTree: true);
                }
                catch
                {
                }

                throw new TimeoutException($"SPCConnector işlemi {timeout.TotalSeconds:0} saniye içinde tamamlanmadı.");
            }
        }

        private static void WritePayload(MemoryMappedFile mmf, byte[] payload)
        {
            using var stream = mmf.CreateViewStream();
            stream.Write(BitConverter.GetBytes(payload.Length));
            stream.Write(payload);
            stream.Flush();
        }

        private static void ClearPayload(MemoryMappedFile mmf)
        {
            using var stream = mmf.CreateViewStream();
            stream.Write(BitConverter.GetBytes(0));
            stream.Flush();
        }

        private static T? ReadPayload<T>(MemoryMappedFile mmf)
        {
            using var stream = mmf.CreateViewStream();
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
            int length = reader.ReadInt32();
            if (length <= 0 || length > stream.Length - sizeof(int) || length > 10 * 1024 * 1024)
                return default;

            byte[] data = reader.ReadBytes(length);
            if (data.Length != length)
                return default;

            return JsonSerializer.Deserialize<T>(data);
        }
    }
}
