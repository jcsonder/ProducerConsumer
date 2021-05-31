﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    class Program
    {
        static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        static async Task Main()
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
                                CancellationTokenSource.Cancel();
                                break;
                        }

                        if (breakLoop)
                        {
                            break;
                        }
                    }
                }
            }, CancellationTokenSource.Token);

            await producerTask;

            mediator.Stop();

            Console.WriteLine($"mediator.CalculatedData.Count={mediator.CalculatedData.Count}");
            mediator.CalculatedData.ForEach(Console.WriteLine);

            Console.WriteLine($"mediator: Not processed event count={mediator.Events.Count}, Items:{string.Join(", ", mediator.Events)}");

            Console.WriteLine("End of Main - Press any key to finish");
            Console.ReadLine();
        }

        private class Mediator
        {
            private readonly Random _rnd;
            private readonly ConcurrentQueue<int> _events;
            private Task _runningCalculation;

            public Mediator()
            {
                _rnd = new Random();
                _events = new ConcurrentQueue<int>();
                CalculatedData = new List<Result>();
            }

            public List<Result> CalculatedData { get; }

            public ConcurrentQueue<int> Events => _events;

            public void Stop()
            {
                CancellationTokenSource.Cancel();

                if (_runningCalculation != null && !_runningCalculation.IsCompleted)
                {
                    Console.WriteLine("Running Task exists");
                }
                else
                {
                    Console.WriteLine("No running Task exists");
                }
            }

            public void Handle(int @event)
            {
                Console.WriteLine($"Mediator handle: {@event}: {DateTime.Now:MM/dd/yyyy hh:mm:ss.fff}");

                _events.Enqueue(@event);

                _runningCalculation = Task.Run(() => SlowBatchProcessingAsync(_events));
            }

            private Task SlowBatchProcessingAsync(ConcurrentQueue<int> events)
            {
                // processing takes at least 200ms: Producer creates the events faster then we can process them!
                int waitTimeInMs = _rnd.Next(200, 1000);
                try
                {
                    Task.Delay(waitTimeInMs, CancellationTokenSource.Token).Wait(CancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    return Task.FromCanceled(CancellationTokenSource.Token);
                }

                while (events.Count > 0)
                {
                    if (events.TryDequeue(out var @event))
                    {
                        Console.WriteLine($"                   : {@event}");
                        CalculatedData.Add(new Result(@event, @event + 0.1));
                    }
                }

                return Task.CompletedTask;
            }

            public class Result
            {
                public Result(int raw, double calculated)
                {
                    RawValue = raw;
                    CalculatedValue = calculated;
                }

                public int RawValue { get; }
                public double CalculatedValue { get; }

                public override string ToString()
                {
                    return $"Raw={RawValue}, CalculatedValue={CalculatedValue}";
                }
            }
        }
    }
}
