using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Mediator mediator = new Mediator();
            var producerTask = Task.Run(() => {

                for (int i = 1; i < 100; i++)
                {
                    System.Threading.Thread.Sleep(100);
                    mediator.Handle(i);
                }
            });

            await producerTask;
        }

        private class Mediator
        {
            private readonly Random _rnd;
            private readonly BlockingCollection<int> _events;
            private Task _runningCalculation;

            public Mediator()
            {
                _rnd = new Random();
                _events = new BlockingCollection<int>();
            }

            public void Handle(int @event)
            {
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));

                _events.Add(@event);

                if (_runningCalculation == null || _runningCalculation.IsCompleted)
                {
                    _runningCalculation = Task.Run(() => SlowBatchProcessing(_events));
                }
            }

            private Task SlowBatchProcessing(BlockingCollection<int> events)
            {
                int timeout = _rnd.Next(200, 1000);
                System.Threading.Thread.Sleep(timeout);
                
                System.Console.WriteLine($"event calculation: {string.Join(", ", @events)}");

                // remove all items from blocking collection
                while (events.Count > 0)
                {
                    events.TryTake(out _);
                }

                return Task.CompletedTask;
            }
        }
    }
}
