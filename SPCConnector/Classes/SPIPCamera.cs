using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using SPCConnector.Services;

namespace SPCConnector.Classes
{
    /// <summary>
    /// Kamera bağlantı durumu.
    /// </summary>
    public enum ConnectionState
    {
        Disconnected,
        Connected,
        Testing,
        None
    }

    internal class SPIPCamera
    {
        #region Fields

        /// <summary>
        /// id(0), ip(1), port(2), userName(3), password(4), profile(5), mahal(6)
        /// </summary>
        private Tuple<int, string, int, string, string, string, string>? CameraInfo;

        private readonly OnvifDeviceService _onvifService;
        private readonly OnvifPtzService _onvifPtzService;
        private string? _activeProfileToken;

        // LibVLC Bileşenleri
        private readonly LibVLC? _libVLC;
        private readonly MediaPlayer? _mediaPlayer;
        private Media? _currentMedia;
        private readonly VideoView? _videoView;

        private int TimeOutCounter;

        #endregion

        #region Ctor

        public SPIPCamera(LibVLC libVLC, VideoView videoView)
        {
            _onvifService = new OnvifDeviceService();
            _onvifPtzService = new OnvifPtzService();
            _libVLC = libVLC;
            _videoView = videoView;

            if (_videoView != null && _libVLC != null)
            {
                _mediaPlayer = new MediaPlayer(_libVLC);
                _videoView.MediaPlayer = _mediaPlayer;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// ONVIF üzerinden kamera profillerini çeker, RTSP adresini alır ve LibVLC ile oynatır.
        /// </summary>
        public async Task<bool> ConnectAsync(Tuple<int, string, int, string, string, string, string> camera)
        {
            try
            {
                CameraInfo = camera;
                string ip = camera.Item2;
                int port = camera.Item3;
                string username = camera.Item4;
                string password = camera.Item5;
                string targetProfileName = camera.Item6;

                var profiles = await _onvifService.GetCameraProfiles(ip, port, username, password);
                if (profiles == null || profiles.Count == 0)
                    return false;

                var selectedProfile = profiles.FirstOrDefault(p => p.Token.Equals(targetProfileName, StringComparison.OrdinalIgnoreCase))
                                      ?? profiles.First();

                _activeProfileToken = selectedProfile.Token;

                string rtspUri = await _onvifService.GetCameraRtsp(ip, port, username, password, _activeProfileToken);
                if (string.IsNullOrEmpty(rtspUri))
                    return false;

                StartVlcStream(rtspUri);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// LibVLC üzerinden RTSP akışını başlatır.
        /// </summary>
        private void StartVlcStream(string rtspUri)
        {
            if (_mediaPlayer == null || _libVLC == null) return;

            if (_currentMedia != null)
            {
                _currentMedia.Dispose();
                _currentMedia = null;
            }

            _currentMedia = new Media(_libVLC, new Uri(rtspUri), ":rtsp-tcp", ":network-caching=150");

            _mediaPlayer.EncounteredError += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("LibVLC Hata Aldı: Akış oynatılamadı!");
            };

            _mediaPlayer.Playing += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("LibVLC Başarıyla Oynatıyor.");
            };

            _mediaPlayer.Play(_currentMedia);

            if (_videoView != null && _videoView.Width > 0 && _videoView.Height > 0)
            {
                _mediaPlayer.AspectRatio = $"{_videoView.Width}:{_videoView.Height}";
            }
        }

        /// <summary>
        /// Bağlantıyı keser ve kaynakları temizler.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Stop();
                    _mediaPlayer.Dispose();
                }

                if (_currentMedia != null)
                {
                    _currentMedia.Dispose();
                    _currentMedia = null;
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Butonların Tag değerine ("Up", "Down", "Left", "Right", "In", "Out") göre PTZ hareketini başlatır.
        /// </summary>
        public async void MoveCamera(string directionTag)
        {
            if (CameraInfo == null || string.IsNullOrEmpty(_activeProfileToken) || string.IsNullOrEmpty(directionTag))
                return;

            try
            {
                float pan = 0f, tilt = 0f, zoom = 0f;

                switch (directionTag.ToLowerInvariant())
                {
                    case "left": pan = -0.5f; break;
                    case "right": pan = 0.5f; break;
                    case "up": tilt = 0.5f; break;
                    case "down": tilt = -0.5f; break;
                    case "in": zoom = 0.5f; break;
                    case "out": zoom = -0.5f; break;
                }

                await _onvifPtzService.PtzStart(
                    CameraInfo.Item2,
                    CameraInfo.Item3,
                    CameraInfo.Item4,
                    CameraInfo.Item5,
                    _activeProfileToken,
                    pan,
                    tilt,
                    zoom
                );
            }
            catch
            { }
        }

        /// <summary>
        /// Kamera hareketinin durdurulmasını sağlar (Mouse bırakıldığında tetiklenir).
        /// </summary>
        public async void CameraMouseUpMove()
        {
            if (CameraInfo == null || string.IsNullOrEmpty(_activeProfileToken))
                return;

            try
            {
                await _onvifPtzService.PtzStop(
                    CameraInfo.Item2,
                    CameraInfo.Item3,
                    CameraInfo.Item4,
                    CameraInfo.Item5,
                    _activeProfileToken
                );
            }
            catch
            { }
        }

        /// <summary>
        /// Zaman aşımı sayacını yönetir.
        /// </summary>
        public bool IsTimedOut()
        {
            return System.Threading.Interlocked.Increment(ref TimeOutCounter) > 11;
        }

        public void ResetTimeOutCounter() => System.Threading.Interlocked.Exchange(ref TimeOutCounter, 0);

        #endregion
    }
}
