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
            BlockingCollection<int> events = new BlockingCollection<int>();
            Task runningCalculation;

            public void Handle(int @event)
            {
                System.Console.WriteLine(System.DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"));

                events.Add(@event);

                if (runningCalculation == null || runningCalculation.IsCompleted)
                {
                    runningCalculation = Task.Run(() => SlowBatchProcessing(events));
                }
            }

            private Task SlowBatchProcessing(BlockingCollection<int> events)
            {
                System.Threading.Thread.Sleep(1000);
                System.Console.WriteLine($"event calculation: {string.Join(", ", @events)}");

                while (events.Count > 0)
                {
                    events.TryTake(out _);
                }

                return Task.CompletedTask;
            }
        }
    }
}
