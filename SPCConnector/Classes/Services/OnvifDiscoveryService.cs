using OnvifLib;

namespace SPCConnector.Services
{
    public class CameraDevice()
    {
        public string Name { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
        public int Port { get; set; }
    }

    public class OnvifDiscoveryService
    {
        private readonly int timeout = 5;

        public async Task<List<CameraDevice>> DiscoverOnvifCameras()
        {
            var cameraList = new List<CameraDevice>();

            var result = await WsDiscovery.ProbeAsync(TimeSpan.FromSeconds(timeout), CancellationToken.None);

            if (result.ScanOk)
            {
                foreach (var device in result.Devices)
                {
                    cameraList.Add(new CameraDevice
                    {
                        Name = device.Name ?? string.Empty,
                        Ip = device.Ip ?? string.Empty,
                        Port = device.Port > 0 ? device.Port : 80
                    });
                }
            }

            return cameraList;
        }
    }
}
