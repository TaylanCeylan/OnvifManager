using SPCConnector.Services;

namespace SPCConnector.Classes
{
    internal class CameraDiscovery
    {
        private const string MmfName = "SPCameraDiscovery";

        public static bool Start()
        {
            var resultList = new List<Tuple<string,string, int>>();

            try
            {
                var task = Task.Run(async () =>
                {
                    var discoveryService = new OnvifDiscoveryService();
                    return await discoveryService.DiscoverOnvifCameras();
                });

                var cameras = task.GetAwaiter().GetResult();

                if (cameras != null && cameras.Count > 0)
                {
                    foreach (var cam in cameras)
                    {
                        resultList.Add(new Tuple<string, string, int>(cam.Name, cam.Ip, cam.Port));
                    }
                }
            }
            catch
            {
            }

            MmfHelper.WriteJsonToMmf(MmfName, resultList, openExistingFirst: true);

            Thread.Sleep(1000);
            return true;
        }
    }
}