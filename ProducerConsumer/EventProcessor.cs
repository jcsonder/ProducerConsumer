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
            Task.Run(SlowBatchProcessingAsync, _cancellationTokenSource.Token);
        }

        private Task SlowBatchProcessingAsync()
        {
            // processing takes at least 200ms: Producer creates the events faster then we can process them!
            int waitTimeInMs = _rnd.Next(200, 1000);
            try
            {
                Task.Delay(waitTimeInMs, _cancellationTokenSource.Token).Wait(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return Task.FromCanceled(_cancellationTokenSource.Token);
            }

            while (_unprocessedEvents.Count > 0)
            {
                if (_unprocessedEvents.TryDequeue(out var @event))
                {
                    Console.WriteLine($"                   : {@event}");
                    CalculatedData.Add(new Result(@event, @event + 0.1));
                }
            }

            return Task.CompletedTask;
        }
    }
}