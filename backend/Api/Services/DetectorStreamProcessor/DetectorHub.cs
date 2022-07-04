using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Api.Services.DetectorStreamProcessor
{
    public class DetectorHub : Hub
    {
        private readonly DetectorStreamViewerGroups _groups;
        private readonly ILogger _logger;

        public DetectorHub(DetectorStreamViewerGroups groups, ILogger logger)
        {
            _groups = groups;
            _logger = logger;
        }

        public async Task JoinGroup(string groupName)
        {
            // TODO(rg): validate groupName

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _groups.Join(groupName);

            _logger.Information("Connection {Id} joined group {Group}", Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            if (_groups.Exists(groupName))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                _groups.Leave(groupName);

                _logger.Information("Connection {Id} left group {Group}", Context.ConnectionId, groupName);
            }

            // TODO(rg): handle case where groupName is invalid
        }

    }
}