using System.IO;
using System.Threading.Tasks;

namespace MemeScrapper
{
    public class BlobStorageService
    {
        readonly string directory;
        public BlobStorageService(string directory)
        {
            this.directory = directory;
        }

        public async Task<string> Save(byte[] bytes)
        {
            var key = HashingExtensions.Md5Hash(bytes);
            var path = Path.Combine(directory, key);
            await File.WriteAllBytesAsync(path, bytes);
            return key;
        }

        public async Task<byte[]> Read(string key)
        {
            var path = Path.Combine(directory, key);
            var bytes = await File.ReadAllBytesAsync(path);
            return bytes;
        }
    }
}
