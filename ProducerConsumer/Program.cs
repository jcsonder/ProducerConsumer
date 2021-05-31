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

                for (int i = 1; i <= 100; i++)
                {
                    // every 100 ms we have to handle an event
                    System.Threading.Thread.Sleep(100);
                    mediator.Handle(i);

                    bool breakLoop = false;
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        switch (key.Key)
                        {
                            case ConsoleKey.X:
                                breakLoop = true;
                                break;
                            default:
                                break;
                        }

                        if (breakLoop)
                        {
                            break;
                        }
                    }
                }
            });

            await producerTask;

            mediator.Stop();
            mediator.CalculatedData.ForEach(x => Console.WriteLine($"{x.Item1}_{x.Item2}"));

            Console.ReadLine();
        }

        private class Mediator
        {
            private readonly Random _rnd;
            private readonly BlockingCollection<int> _events;
            
            private Task _runningCalculation;
            private List<Tuple<int, double>> _calculatedData;

            public Mediator()
            {
                _rnd = new Random();
                _events = new BlockingCollection<int>();
                CalculatedData = new List<Tuple<int, double>>();
            }

            public List<Tuple<int, double>> CalculatedData { get => _calculatedData; private set => _calculatedData = value; }

            public void Handle(int @event)
            {
                Console.WriteLine($"incoming event: {@event}: {DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}");

                _events.Add(@event);

                if (_runningCalculation == null || _runningCalculation.IsCompleted)
                {
                    _runningCalculation = Task.Run(() => SlowBatchProcessing(_events));
                }
            }

            public void Stop()
            {
                if (_runningCalculation != null)
                {
                    // TODO: cancel
                }
            }

            private Task SlowBatchProcessing(BlockingCollection<int> events)
            {
                //Console.WriteLine($"SlowBatchProcessing: {string.Join(",", events.ToArray())}");

                // processing takes at least 200ms: Producer creates the events faster then we can process them!
                int timeout = _rnd.Next(200, 1000);
                System.Threading.Thread.Sleep(timeout);
                
                while (events.Count > 0)
                {
                    if (events.TryTake(out int @event))
                    {
                        Console.WriteLine($"                   : {@event}");
                        CalculatedData.Add(new Tuple<int, double>(@event, @event + 0.1));
                    }
                }

                return Task.CompletedTask;
            }
        }
    }
}
