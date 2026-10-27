using SPCConnector.Services;

namespace SPCConnector.Classes
{
    internal class ConnectionScanner
    {
        private const string MmfName = "SPCameraTester";

        #region Fields

        /// <summary>
        /// MMF'den okunan taranacak kamera listesi
        /// ip, port, username, password
        /// </summary>
        private static List<Tuple<string, int, string, string>> CameraList = new();

        /// <summary>
        /// Toplam kamera sayısı
        /// </summary>
        private static int CameraCount;

        #endregion

        #region Methods

        /// <summary>
        /// Taramanın başlatılmasını sağlar.
        /// </summary>
        public static bool Start(string count)
        {
            if (!int.TryParse(count, out CameraCount) || CameraCount < 0)
            {
                WriteResult(new List<Tuple<string, int>>());
                return false;
            }

            CameraList = GetCameraList();

            if (CameraList.Count == 0)
            {
                WriteResult(new List<Tuple<string, int>>());
                return false;
            }

            Task.Run(async () => await StartConnectionTestAsync()).GetAwaiter().GetResult();
            return true;
        }

        /// <summary>
        /// MMF'de bulunan kamera listesinin alınmasını sağlar.
        /// Başarısız olursa null döndürür.
        /// </summary>
        private static List<Tuple<string, int, string, string>> GetCameraList()
        {
            try
            {
                return MmfHelper.ReadJsonFromMmf<List<Tuple<string, int, string, string>>>(MmfName) ?? new();
            }
            catch
            {
                return new();
            }
        }

        /// <summary>
        /// Verilen nesnenin MMF'ye yazılmasını sağlar.
        /// </summary>
        private static void WriteResult(object result)
        {
            try
            {
                MmfHelper.WriteJsonToMmf(MmfName, result);
            }
            catch (Exception ex)
            {
#if LOGACTIVE
                Logger.Log($"WriteResult başarısız. Hata: {ex}");
#endif
            }
        }

        /// <summary>
        /// Kameralar için bağlantı testi yapar ve sonuçları paylaşımlı mmf stream'ine yazar.
        /// </summary>
        private static async Task StartConnectionTestAsync()
        {
            try
            {
                var resultList = new List<Tuple<string, int>>();
                var deviceService = new OnvifDeviceService();

                foreach (var targetCamera in CameraList)
                {
                    bool isAlive = await deviceService.IsCameraAliveAsync(
                        targetCamera.Item1,
                        targetCamera.Item2,
                        targetCamera.Item3,
                        targetCamera.Item4
                    );

                    int connectionState = isAlive ? 1 : 0; // Connected: 1, Disconnected: 0

                    resultList.Add(new Tuple<string, int>(targetCamera.Item1, connectionState));
                }

                WriteResult(resultList);
            }
            catch
            {
                WriteResult(new List<Tuple<string, int>>());
            }
        }

        #endregion
    }
}
