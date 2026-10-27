namespace SPCConnector.Services
{
    public class OnvifDeviceService
    {
        public async Task<List<OnvifLib.OnvifProfileInfo>> GetCameraProfiles(string ip, int port, string username, string password)
        {
            var camera = Camera.Create(ip, port, username, password);
            await camera.InitTask;

            var profiles = await camera.GetProfiles();
            if (profiles == null || profiles.Count == 0)
            {
                throw new Exception("Camera profiles not found");
            }

            return profiles;
        }

        public async Task<string> GetCameraRtsp(string ip, int port, string username, string password, string profileToken)
        {
            try
            {
                var camera = Camera.Create(ip, port, username, password);
                await camera.InitTask;

                var mediaService = await camera.GetMediaService();
                if (mediaService == null)
                {
                    throw new Exception("Media service is not available");
                }

                string rtspUri = await mediaService.GetStreamUri(profileToken);
                if (string.IsNullOrEmpty(rtspUri))
                {
                    throw new Exception("RTSP stream URI is empty");
                }

                return InsertCredentialsIntoRtsp(rtspUri, username, password);
            }
            catch (Exception ex)
            {
                throw new Exception($"ONVIF media error: {ex.Message}");
            }
        }

        private string InsertCredentialsIntoRtsp(string rtspUri, string username, string password)
        {
            if (string.IsNullOrEmpty(rtspUri))
                return rtspUri;

            if (rtspUri.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase) && Uri.TryCreate(rtspUri, UriKind.Absolute, out var uri))
            {
                var builder = new UriBuilder(uri)
                {
                    UserName = username,
                    Password = password
                };
                return builder.Uri.ToString();
            }
            return rtspUri;
        }

        public async Task TakeSnapshot(string ip, int port, string username, string password, string path)
        {
            var camera = Camera.Create(ip, port, username, password);
            await camera.InitTask;

            var mediaService = await camera.GetMediaService();
            if (mediaService == null)
            {
                throw new Exception("Media service is not available");
            }

            var image = await mediaService.GetImage();
            if (image == null || image.Data == null || image.Data.Length == 0)
                throw new InvalidOperationException("Kamera anlık görüntü verisi döndürmedi.");

            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllBytesAsync(path, image.Data);
        }

        public async Task<bool> IsCameraAliveAsync(string ip, int port, string username, string password)
        {
            try
            {
                var camera = Camera.Create(ip, port, username, password);
                await camera.InitTask;

                var profiles = await camera.GetProfiles();
                if (profiles == null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
