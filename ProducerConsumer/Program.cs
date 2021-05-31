using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

                for (int i = 1; i <= 100; i++)
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

            mediator.Stop();

            Console.WriteLine($"mediator.CalculatedData.Count={mediator.CalculatedData.Count}");
            mediator.CalculatedData.ForEach(x => Console.WriteLine(x));
            
            Console.WriteLine("End of Main - Press any key to finish");
            Console.ReadLine();
        }

        private class Mediator
        {
            private readonly Random _rnd;
            private readonly BlockingCollection<int> _events;
            private Task _runningCalculation;
            
            // TODO: Order is not guaranteed!
            private List<Result> _calculatedData;

            public Mediator()
            {
                _rnd = new Random();
                _events = new BlockingCollection<int>(new ConcurrentQueue<int>());
                CalculatedData = new List<Result>();
            }

            public List<Result> CalculatedData { get => _calculatedData; private set => _calculatedData = value; }

            public void Stop()
            {
                //_events.CompleteAdding();
                cancellationTokenSource.Cancel();

                if (_runningCalculation != null && !_runningCalculation.IsCompleted)
                {
                    Console.WriteLine($"Running Task exists");
                }
                else
                {
                    Console.WriteLine($"No running Task exists");
                }
            }

            public void Handle(int @event)
            {
                Console.WriteLine($"Mediator handle: {@event}: {DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}");

                _events.Add(@event);

                _runningCalculation = Task.Run(() => SlowBatchProcessingAsync(_events));
            }

            private Task SlowBatchProcessingAsync(BlockingCollection<int> events)
            {
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
                    
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    // TODO: Order is not correct (anymore?): But that's a mus!
                    Console.WriteLine($"                   : {@event}");
                    CalculatedData.Add(new Result(@event, @event + 0.1));
                }

                return Task.CompletedTask;
            }

            public class Result
            {
                public Result(int raw, double calculted)
                {
                    RawValue = raw;
                    CalcultedValue = calculted;
                }

                public int RawValue { get; }
                public double CalcultedValue { get; }

                public override string ToString()
                {
                    return $"Raw={RawValue}, CalculatedValeu={CalcultedValue}";
                }
            }
        }
    }
}
