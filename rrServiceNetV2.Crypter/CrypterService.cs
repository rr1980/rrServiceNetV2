using Microsoft.Extensions.Logging;
using rrServiceNetV2.Common;
using System.Text;

namespace rrServiceNetV2.Crypter
{
    public class CrypterService : ICrypter
    {
        private readonly ILogger<CrypterService> _logger;

        public CrypterService(ILogger<CrypterService> logger)
        {
            _logger = logger;
            _logger.LogTrace("init finished");
        }

        public string Decrypt(byte[] data, int bytesRead)
        {
            return Encoding.ASCII.GetString(data, 0, bytesRead);
        }
    }
}
