using System;
using System.Collections.Generic;
using System.Threading;
using Serilog;

namespace Api.Services.DetectorController
{
    public class DetectorCommandQueues
    {
        private readonly Dictionary<int, DetectorCommandQueue> _queues = new();
        private readonly ILogger _logger;

        public DetectorCommandQueues(ILogger logger)
        {
            _logger = logger;
        }

        public DetectorCommandQueue this[int index] => _queues[index];

        public bool EnqueueCommand(int key, DetectorCommand command)
        {
            if (!_queues.ContainsKey(key)) return false;

            _queues[key].EnqueueCommand(command);
            return true;
        }

        public DetectorCommandQueue AddQueue(int key)
        {
            var queue = new DetectorCommandQueue();
            try
            {
                _queues.Add(key, queue);
            }
            catch (ArgumentException _)
            {
                _logger.Warning("Command queue for detector (id: {Id}) already exists", key);
            }

            return queue;
        }

        public bool RemoveQueue(int key)
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