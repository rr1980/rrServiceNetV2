using Microsoft.Extensions.Logging;
using rrServiceNetV2.Common;
using rrServiceNetV2.Gateway.Server;
using System;

namespace rrServiceNetV2.Gateway.Handler.Commands
{
    public class Command_Raw : CommandBase<Command_Raw>
    {
        public Command_Raw(ServerService serverService, ILogger<Command_Raw> logger) : base(serverService, logger)
        {
        }

        internal override void Execute(CallPackage cp)
        {
            _logger.LogDebug(Environment.NewLine + cp.Data);
        }
    }
}
