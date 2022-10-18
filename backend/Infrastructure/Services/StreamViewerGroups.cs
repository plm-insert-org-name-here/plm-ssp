using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Services.DetectorController;
using Application.Services.StreamHandler;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Application.Services
{
    public class StreamViewerGroups
    {
        private readonly IHubContext<StreamHub> _hubContext;
        private readonly ILogger _logger;
        private readonly Dictionary<string, int> _viewersPerGroup = new();
        private readonly DetectorCommandQueues _queues;

        public StreamViewerGroups(IHubContext<StreamHub> hubContext, ILogger logger, DetectorCommandQueues queues)
        {
            _hubContext = hubContext;
            _logger = logger;
            _queues = queues;
        }

        public void Join(string groupName)
        {
            if (_viewersPerGroup.ContainsKey(groupName))
            {
                _viewersPerGroup[groupName]++;
            }
            else
            {
                _viewersPerGroup.Add(groupName, 1);

                try
                {
                    if (groupName.StartsWith(Routes.Detectors.StreamGroupPrefix))
                    {
                        var deviceId = ExtractDeviceId(groupName);
                        _queues.EnqueueCommand(deviceId, DetectorCommandType.StartStreaming);
                    }
                }
                catch (InvalidViewerGroupEx ex)
                {
                    _logger.Error("{Message}", ex.Message);
                }
            }
        }

        public void Leave(string groupName)
        {
            if (_viewersPerGroup.ContainsKey(groupName))
            {
                _viewersPerGroup[groupName]--;

                if (_viewersPerGroup[groupName] == 0)
                {
                    _viewersPerGroup.Remove(groupName);

                    try
                    {
                        if (groupName.StartsWith(Routes.Detectors.StreamGroupPrefix))
                        {
                            var deviceId = ExtractDeviceId(groupName);
                            _queues.EnqueueCommand(deviceId, DetectorCommandType.StopStreaming);
                        }
                    }
                    catch (InvalidViewerGroupEx ex)
                    {
                        _logger.Error("{Message}", ex.Message);
                    }
                }
            }
        }

        public async Task SendStreamFrameToGroup(int detectorId, byte[] frame)
        {
            var groupName = Routes.Detectors.StreamGroupPrefix + detectorId;
            if (_viewersPerGroup.ContainsKey(groupName))
                await _hubContext.Clients.Group(groupName)
                    .SendAsync("StreamFrame", frame);
        }

        public bool HasViewers(int detectorId) => _viewersPerGroup.ContainsKey(Routes.Detectors.StreamGroupPrefix + detectorId);

        public static int ExtractDeviceId(string group)
        {
            var deviceIdString = group.Split('-')[1];
            var result = int.TryParse(deviceIdString, out var deviceId);
            if (!result) throw new InvalidViewerGroupEx("Could not extract device Id from group name: " + group);

            return deviceId;
        }


    }
}