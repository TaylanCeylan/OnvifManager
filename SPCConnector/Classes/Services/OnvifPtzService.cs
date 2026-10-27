namespace SPCConnector.Services
{
    public class OnvifPtzService
    {
        public async Task PtzStart(string ip, int port, string username, string password, string profileToken,float panVelocity, float tiltVelocity, float zoomVelocity)
        {
            var camera = Camera.Create(ip, port, username, password);
            await camera.InitTask;

            var ptz = await camera.GetPtzService();
            if (ptz != null)
            {
                await ptz.ContinuousMoveAsync(profileToken, panTiltX: panVelocity, panTiltY: tiltVelocity, zoom: zoomVelocity, "PT10S");
            }
        }

        public async Task PtzStop(string ip, int port, string username, string password, string profileToken)
        {
            var camera = Camera.Create(ip, port, username, password);
            await camera.InitTask;

            var ptz = await camera.GetPtzService();
            if (ptz != null)
            {
                await ptz.StopAsync(profileToken);
            }
        }
    }
}
