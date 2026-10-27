using SPCConnector.Services;

namespace SPCConnector.Classes
{
    internal class CameraProfiles
    {
        private const string OutputMmfName = "SPCameraProfiles_Output";

        public static bool Start(string[] args)
        {
            var profileInfos = new List<Tuple<string,string>>();

            try
            {
                if (args == null || args.Length < 5)
                {
                    MmfHelper.WriteJsonToMmf(OutputMmfName, profileInfos, openExistingFirst: true);
                    return false;
                }

                string ip = args[1];
                if (!int.TryParse(args[2], out int port)) port = 80;
                string username = args[3];
                string password = args[4];

                var task = Task.Run(async () =>
                {
                    var deviceService = new OnvifDeviceService();
                    return await deviceService.GetCameraProfiles(ip, port, username, password);
                });

                profileInfos = task.GetAwaiter().GetResult()?.Select(p => new Tuple<string, string>(p.Name, p.Token)).ToList() ?? new List<Tuple<string, string>>();
            }
            catch (Exception)
            {
                profileInfos = new List<Tuple<string, string>>();
            }

            MmfHelper.WriteJsonToMmf(OutputMmfName, profileInfos, openExistingFirst: true);

            Thread.Sleep(500);
            return true;
        }
    }
}