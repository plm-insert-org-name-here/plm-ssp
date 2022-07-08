using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Api.Services.StreamHandler
{
    public class StreamHub : Hub
    {
        private readonly StreamViewerGroups _groups;
        private readonly ILogger _logger;

        public StreamHub(StreamViewerGroups groups, ILogger logger)
        {
            _groups = groups;
            _logger = logger;
        }

        // Called by the frontend (remote procedure call)
        public async Task JoinGroup(string groupName)
        {
            // TODO(rg): validate groupName

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _groups.Join(groupName);

            _logger.Information("Connection {Id} joined group {Group}", Context.ConnectionId, groupName);
        }

        // Called by the frontend (remote procedure call)
        public async Task LeaveGroup(string groupName)
        {
            var detectorId = StreamViewerGroups.ExtractDeviceId(groupName);

            if (_groups.HasViewers(detectorId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                _groups.Leave(groupName);

                _logger.Information("Connection {Id} left group {Group}", Context.ConnectionId, groupName);
            }

            // TODO(rg): handle case where groupName is invalid
        }
    }
}