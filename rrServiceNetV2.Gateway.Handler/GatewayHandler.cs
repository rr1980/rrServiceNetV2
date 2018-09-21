using Microsoft.Extensions.Logging;
using rrServiceNetV2.Common;
using rrServiceNetV2.Gateway.Handler.Commands;
using System.Collections.Generic;
using System.Linq;

namespace rrServiceNetV2.Gateway.Handler
{
    public class GatewayHandler : IHandler
    {
        private readonly ILogger<GatewayHandler> _logger;
        readonly Dictionary<string[], CommandBase> commands = new Dictionary<string[], CommandBase>();

        public GatewayHandler(ILogger<GatewayHandler> logger)
        {
            _logger = logger;
            _logger.LogTrace("init finished");
        }

        public void Handle(CallPackage response)
        {
            _logger.LogTrace("+: command: " + response.Command);

            var com = commands.FirstOrDefault(x => x.Key.Any(y => y == response.Command)).Value;

            if (com != null)
            {
                com.Execute(response);
            }
        }

        public void Add(string[] key, CommandBase command)
        {
            commands.Add(key, command);
        }
    }
}
