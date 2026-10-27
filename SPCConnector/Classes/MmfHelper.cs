#nullable enable
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Json;

namespace SPCConnector.Classes
{
    public static class MmfHelper
    {
        public static void WriteJsonToMmf(string tag, object data, bool openExistingFirst = false)
        {
            byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
            MemoryMappedFile mmf;
            if (openExistingFirst)
            {
                try
                {
                    mmf = MemoryMappedFile.OpenExisting(tag);
                }
                catch (FileNotFoundException)
                {
                    mmf = MemoryMappedFile.CreateOrOpen(tag, Math.Max(65536, payload.Length + sizeof(int)));
                }
            }
            else
            {
                mmf = MemoryMappedFile.CreateOrOpen(tag, Math.Max(65536, payload.Length + sizeof(int)));
            }

            using (mmf)
            using (var stream = mmf.CreateViewStream())
            {
                stream.Write(BitConverter.GetBytes(payload.Length));
                stream.Write(payload);
                stream.Flush();
            }
        }

        public static T? ReadJsonFromMmf<T>(string tag)
        {
            try
            {
                using var mmf = MemoryMappedFile.OpenExisting(tag);
                using var stream = mmf.CreateViewStream();
                using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
                int length = reader.ReadInt32();
                if (length <= 0 || length > stream.Length - sizeof(int) || length > 10 * 1024 * 1024)
                    return default;

                byte[] payload = reader.ReadBytes(length);
                return payload.Length == length ? JsonSerializer.Deserialize<T>(payload) : default;
            }
            catch
            {
                return default;
            }
        }
    }
}
