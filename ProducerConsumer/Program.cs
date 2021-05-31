using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    class Program
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            Mediator mediator = new Mediator();
            var producerTask = Task.Run(() => {

                for (int i = 1; i <= 1000; i++)
                {
                    // every 100 ms we have to handle an event
                    Thread.Sleep(100);
                    mediator.Handle(i);

                    bool breakLoop = false;
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        switch (key.Key)
                        {
                            case ConsoleKey.X:
                                breakLoop = true;
                                cancellationTokenSource.Cancel();
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
            }, cancellationTokenSource.Token);

            await producerTask;

            mediator.CalculatedData.ForEach(x => Console.WriteLine($"{x.Item1}_{x.Item2}"));

            Console.WriteLine("End of Main - Press any key to finish");
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
                    _runningCalculation = Task.Run(() => SlowBatchProcessingAsync(_events));
                }
            }

            private Task SlowBatchProcessingAsync(BlockingCollection<int> events)
            {
                //Console.WriteLine($"SlowBatchProcessing: {string.Join(",", events.ToArray())}");

                // processing takes at least 200ms: Producer creates the events faster then we can process them!
                int waitTimeInMs = _rnd.Next(200, 1000);
                try
                {
                    Task.Delay(waitTimeInMs, cancellationTokenSource.Token).Wait(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    return Task.FromCanceled(cancellationTokenSource.Token);
                }
                
                while (events.Count > 0)
                {
                    int @event;
                    try
                    {
                        @event = events.Take(cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Take operation was canceled. IsCancellationRequested={0}", cancellationTokenSource.IsCancellationRequested);
                        return Task.FromCanceled(cancellationTokenSource.Token);
                    }
                    
                    Console.WriteLine($"                   : {@event}");
                    CalculatedData.Add(new Tuple<int, double>(@event, @event + 0.1));
                }

                return Task.CompletedTask;
            }
        }
    }
}
