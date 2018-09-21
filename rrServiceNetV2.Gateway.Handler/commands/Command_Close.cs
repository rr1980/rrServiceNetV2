

using Microsoft.Extensions.Logging;
using rrServiceNetV2.Common;
using rrServiceNetV2.Gateway.Server;

namespace rrServiceNetV2.Gateway.Handler.Commands
{
    public class Command_Close : CommandBase<Command_Close>
    {
        public Command_Close(ServerService serverService, ILogger<Command_Close> logger) : base(serverService, logger)
        {
        }

        internal override void Execute(CallPackage cp)
        {
            ServerService.Send(cp.Client, "go exit");
            ServerService.DisconnectClient(cp.Client);
        }
    }
}
