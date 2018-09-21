using Microsoft.Extensions.Logging;
using rrServiceNetV2.Common;
using rrServiceNetV2.Gateway.Server;

namespace rrServiceNetV2.Gateway.Handler.Commands
{
    public abstract class CommandBase
    {
        internal abstract void Execute(CallPackage cp);
    }

    public abstract class CommandBase<T> : CommandBase
    {
        protected readonly ServerService ServerService;
        protected ILogger<T> _logger;

        protected CommandBase(ServerService serverService, ILogger<T> logger)
        {
            this.ServerService = serverService;
            _logger = logger;

            _logger.LogTrace("init finished");
        }

    }
}
