namespace ProducerConsumer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        const int TotalEventCount = 111;
        const int MillisecondsEventTimeout = 100; // every 100 ms we have to handle an event

        static async Task Main()
        {
            EventProcessor mediator = new EventProcessor();
            var producerTask = Task.Run(() =>
            {
                for (int i = 1; i <= TotalEventCount; i++)
                {
                    Thread.Sleep(MillisecondsEventTimeout);
                    mediator.Handle(i);

                    bool breakLoop = false;
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        switch (key.Key)
                        {
                            case ConsoleKey.X:
                                breakLoop = true;
                                mediator.Stop();
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

            Console.WriteLine($"mediator.CalculatedData.Count={mediator.CalculatedData.Count}, Items:{string.Join(", ", mediator.CalculatedData)}");
            Console.WriteLine($"mediator.UnprocessedEvents.Count={mediator.UnprocessedEvents.Count}, Items:{string.Join(", ", mediator.UnprocessedEvents)}");

            Console.WriteLine("End of Main - Press any key to finish");
            Console.ReadLine();
        }
    }
}
