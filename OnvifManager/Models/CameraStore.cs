using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.ComponentModel;

namespace OnvifManager.Models
{
    internal static class CameraStore
    {
        private static readonly string StorePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OnvifManager",
            "cameras.json");

        public static BindingList<CameraDevice> Load()
        {
            try
            {
                if (!File.Exists(StorePath))
                    return new BindingList<CameraDevice>();

                var saved = JsonSerializer.Deserialize<List<StoredCamera>>(File.ReadAllText(StorePath)) ?? new();
                return new BindingList<CameraDevice>(saved.Select(camera => new CameraDevice
                {
                    Name = camera.Name ?? string.Empty,
                    Ip = camera.Ip ?? string.Empty,
                    Port = camera.Port is > 0 and <= 65535 ? camera.Port : 80,
                    Username = camera.Username ?? string.Empty,
                    Password = CredentialProtection.Unprotect(camera.ProtectedPassword)
                }).ToList());
            }
            catch
            {
                return new BindingList<CameraDevice>();
            }
        }

        public static void Save(IEnumerable<CameraDevice> cameras)
        {
            var folder = Path.GetDirectoryName(StorePath)!;
            Directory.CreateDirectory(folder);

            var saved = cameras.Select(camera => new StoredCamera
            {
                Name = camera.Name,
                Ip = camera.Ip,
                Port = camera.Port,
                Username = camera.Username,
                ProtectedPassword = CredentialProtection.Protect(camera.Password)
            }).ToList();

            string tempPath = StorePath + ".tmp";
            File.WriteAllText(tempPath, JsonSerializer.Serialize(saved, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(tempPath, StorePath, overwrite: true);
        }

        private sealed class StoredCamera
        {
            public string? Name { get; set; }
            public string? Ip { get; set; }
            public int Port { get; set; }
            public string? Username { get; set; }
            public string? ProtectedPassword { get; set; }
        }

        private static class CredentialProtection
        {
            private const int UiForbidden = 0x1;

            public static string Protect(string? value)
            {
                if (string.IsNullOrEmpty(value))
                    return string.Empty;

                byte[] input = Encoding.UTF8.GetBytes(value);
                return Convert.ToBase64String(Transform(input, protect: true));
            }

            public static string Unprotect(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return string.Empty;

                try
                {
                    return Encoding.UTF8.GetString(Transform(Convert.FromBase64String(value), protect: false));
                }
                catch
                {
                    return string.Empty;
                }
            }

            private static byte[] Transform(byte[] input, bool protect)
            {
                IntPtr inputMemory = Marshal.AllocHGlobal(input.Length);
                try
                {
                    Marshal.Copy(input, 0, inputMemory, input.Length);
                    var inputBlob = new DataBlob { Length = input.Length, Data = inputMemory };
                    DataBlob outputBlob;
                    bool success = protect
                        ? CryptProtectData(ref inputBlob, null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, UiForbidden, out outputBlob)
                        : CryptUnprotectData(ref inputBlob, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, UiForbidden, out outputBlob);

                    if (!success)
                        throw new SecurityException("Windows kimlik bilgisi koruması başarısız oldu.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                    try
                    {
                        var result = new byte[outputBlob.Length];
                        Marshal.Copy(outputBlob.Data, result, 0, outputBlob.Length);
                        return result;
                    }
                    finally
                    {
                        LocalFree(outputBlob.Data);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(inputMemory);
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct DataBlob
            {
                public int Length;
                public IntPtr Data;
            }

            [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CryptProtectData(ref DataBlob input, string? description, IntPtr optionalEntropy, IntPtr reserved, IntPtr prompt, int flags, out DataBlob output);

            [DllImport("crypt32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CryptUnprotectData(ref DataBlob input, IntPtr description, IntPtr optionalEntropy, IntPtr reserved, IntPtr prompt, int flags, out DataBlob output);

            [DllImport("kernel32.dll")]
            private static extern IntPtr LocalFree(IntPtr memory);
        }
    }
}
