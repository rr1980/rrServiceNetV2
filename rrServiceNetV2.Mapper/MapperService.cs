using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using rrServiceNetV2.Common;

namespace rrServiceNetV2.Mapper
{
    public class MapperService : IMapper
    {
        private readonly ILogger<MapperService> _logger;

        public MapperService(ILogger<MapperService> logger)
        {
            _logger = logger;
            _logger.LogTrace("init finished");
        }

        public T Map<T>(string response_string) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(response_string);
            }
            catch { }

            return null;
        }
    }
}
