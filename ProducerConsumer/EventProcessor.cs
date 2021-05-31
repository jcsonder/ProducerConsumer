namespace ProducerConsumer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class EventProcessor
    {
        private readonly Random _rnd;
        private readonly ConcurrentQueue<int> _unprocessedEvents;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task _processingTask;

        public EventProcessor()
        {
            _rnd = new Random();
            _unprocessedEvents = new ConcurrentQueue<int>();
            _cancellationTokenSource = new CancellationTokenSource();

            CalculatedData = new List<Result>();
        }

        // TODO: ReadOnly
        public List<Result> CalculatedData { get; }

        // TODO: ReadOnly
        public ConcurrentQueue<int> UnprocessedEvents => _unprocessedEvents;

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Handle(int @event)
        {
            Console.WriteLine($"Mediator handle: {@event}: {DateTime.Now:MM/dd/yyyy hh:mm:ss.fff}");

            _unprocessedEvents.Enqueue(@event);

            // fire & forget
            if (_processingTask == null || _processingTask.IsCompleted)
            {
                _processingTask = Task.Run(SlowBatchProcessing, _cancellationTokenSource.Token);
            }
        }

        private void SlowBatchProcessing()
        {
            Console.WriteLine($"SlowBatchProcessing BatchSize=:{_unprocessedEvents.Count}");

            // simulation: processing takes longer and longer when there is more and more data
            Thread.Sleep(CalculatedData.Count * 10);

            while (_unprocessedEvents.Count > 0)
            {
                if (_unprocessedEvents.TryDequeue(out var @event))
                {
                    Console.WriteLine($"                   : {@event}");
                    CalculatedData.Add(new Result(@event, @event + 0.1));
                }
            }
        }
    }
}