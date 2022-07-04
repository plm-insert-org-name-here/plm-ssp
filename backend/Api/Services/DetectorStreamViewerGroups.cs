using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Services.DetectorController;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Api.Services.DetectorStreamProcessor
{
    public class DetectorStreamViewerGroups
    {
        private readonly IHubContext<DetectorHub> _hubContext;
        private readonly ILogger _logger;
        private readonly Dictionary<string, int> _viewersPerGroup = new();
        private readonly DetectorCommandQueues _queues;

        public DetectorStreamViewerGroups(IHubContext<DetectorHub> hubContext, ILogger logger, DetectorCommandQueues queues)
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
                catch (InvalidViewerGroupException ex)
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
                    catch (InvalidViewerGroupException ex)
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
                    .SendAsync("ReceiveStreamFrame", frame);
        }

        public bool Exists(string groupName) => _viewersPerGroup.ContainsKey(groupName);

        public static int ExtractDeviceId(string group)
        {
            var deviceIdString = group.Split('-')[1];
            var result = int.TryParse(deviceIdString, out var deviceId);
            if (!result) throw new InvalidViewerGroupException("Could not extract device Id from group name: " + group);

            return deviceId;
        }


    }
}