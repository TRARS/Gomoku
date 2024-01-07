using System.IO;
using System.IO.Compression;
using System.Text;

namespace Gomoku.SocketUtils.Helper.Service
{
    public interface IDeflateService
    {
        public byte[] CompressText(string text);
        public byte[] CompressText(byte[] text);
        public string DecompressData(byte[] compressedData);
    }

    internal class DeflateService : IDeflateService
    {
        public byte[] CompressText(string text)
        {
            byte[] rawData = Encoding.UTF8.GetBytes(text);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(rawData, 0, rawData.Length);
                }

                return memoryStream.ToArray();
            }
        }

        public byte[] CompressText(byte[] text)
        {
            byte[] rawData = text;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(rawData, 0, rawData.Length);
                }

                return memoryStream.ToArray();
            }
        }

        public string DecompressData(byte[] compressedData)
        {
            using (MemoryStream memoryStream = new MemoryStream(compressedData))
            {
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
                {
                    using (MemoryStream decompressedStream = new MemoryStream())
                    {
                        deflateStream.CopyTo(decompressedStream);
                        byte[] decompressedBytes = decompressedStream.ToArray();
                        return Encoding.UTF8.GetString(decompressedBytes);
                    }
                }
            }
        }
    }
}
