using System;
using System.Collections.Generic;
using System.Threading;
using Serilog;

namespace Application.Services.DetectorController
{
    using DetectorId = Int32;

    public class DetectorCommandQueues
    {
        private readonly Dictionary<DetectorId, DetectorCommandQueue> _queues = new();
        private readonly ILogger _logger;

        public DetectorCommandQueues(ILogger logger)
        {
            _logger = logger;
        }

        public bool EnqueueCommand(DetectorId key, DetectorCommand command)
        {
            if (!_queues.ContainsKey(key)) return false;

            _queues[key].EnqueueCommand(command);
            return true;
        }

        public DetectorCommandQueue AddQueue(DetectorId key)
        {
            var queue = new DetectorCommandQueue();
            try
            {
                _queues.Add(key, queue);
            }
            catch (ArgumentException)
            {
                _logger.Debug("Command queue for detector (id: {Id}) already exists", key);
            }

            return queue;
        }

        public bool RemoveQueue(DetectorId key)
        {
            return _queues.Remove(key);
        }
    }

    public class DetectorCommandQueue
    {
        private readonly Queue<DetectorCommand> _commands = new();
        public readonly AutoResetEvent EnqueueEvent = new(false);

        public int Count => _commands.Count;

        public void EnqueueCommand(DetectorCommand command)
        {
            _commands.Enqueue(command);
            EnqueueEvent.Set();
        }

        public DetectorCommand Dequeue()
        {
            return _commands.Dequeue();
        }
    }
}