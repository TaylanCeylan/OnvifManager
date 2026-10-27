using SPCConnector.Services;

namespace SPCConnector.Classes
{
    internal class CameraSnapshots
    {
        public static bool Start(string[] args)
        {
            string eventId = (args != null && args.Length > 1) ? args[1] : "Default";
            string mmfName = $"SPSnapshot{eventId}";

            var results = new List<Tuple<int, string>>();

            try
            {
                var camerasToSnapshot = MmfHelper.ReadJsonFromMmf<List<Tuple<int, string, int, string, string, string>>>(mmfName);

                if (camerasToSnapshot == null || camerasToSnapshot.Count == 0)
                {
                    MmfHelper.WriteJsonToMmf(mmfName, results);
                    return false;
                }

                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string targetFolder = Path.Combine(baseDirectory, "Snapshots", eventId);

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                var deviceService = new OnvifDeviceService();

                var task = Task.Run(async () =>
                {
                    foreach (var cam in camerasToSnapshot)
                    {
                        int index = cam.Item1;
                        string ip = cam.Item2;
                        int port = cam.Item3;
                        string username = cam.Item4;
                        string password = cam.Item5;

                        string fileName = $"cam_{index}_{ip}_{port}.jpg";
                        string filePath = Path.Combine(targetFolder, fileName);

                        try
                        {
                            await deviceService.TakeSnapshot(ip, port, username, password, filePath);
                            results.Add(new Tuple<int, string>(index, filePath));
                        }
                        catch (Exception ex)
                        {
                            results.Add(new Tuple<int, string>(index, $"Error: {ex.Message}"));
                        }
                    }
                });

                task.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                results.Add(new Tuple<int, string>(-1, $"Critical Error: {ex.Message}"));
            }

            MmfHelper.WriteJsonToMmf(mmfName, results);

            Thread.Sleep(500);
            return true;
        }
    }
}