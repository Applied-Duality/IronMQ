#region Apache 2 License
// Copyright (c) Applied Duality, Inc., All rights reserved.
// See License.txt in the project root for license information.
#endregion

using System;
using System.Configuration;
using System.Reactive.IronMQ;
using System.Threading;
using System.Threading.Tasks;

namespace IronMQSample
{
    partial class Program
    {
        
        static void Main()
        {
            //ProtectConfiguration();
            MainAsync().Wait();
        }

        static Client client; 

        static async Task MainAsync()
        {
            UnProtectConfiguration();
            client = new Client(ConfigurationManager.AppSettings["applicationID"], ConfigurationManager.AppSettings["oauthToken"]);

            var queue = await client.CreateOrGetQueueAsync("foo");

            //var count = await queue.GetCountAsync();
            //Console.WriteLine(count);
            //await queue.DeleteAsync();
            //await queue.DeleteAsync();
            //await queue.GetCountAsync();
            //Console.WriteLine(count);

            client.ListQueues().Subscribe(_queue =>
            {
                Console.WriteLine("Found {0}", _queue.Name);
            });

            var source = new CancellationTokenSource();
            var add = AddMessages(source.Token);
            var delete = ProcessMessages(source.Token);
            Console.WriteLine("Hit enter to cancel ...");
            Console.ReadLine();
            source.Cancel();
            Task.WaitAll(add, delete);

            Console.WriteLine("Bye from Main!");
            Console.ReadLine();
        }
        
        public static async Task AddMessages(CancellationToken cancel)
        {
            var queue = await client.CreateOrGetQueueAsync("demo_queue");
            while (!cancel.IsCancellationRequested)
            {
                var body = System.DateTime.Now.Ticks.ToString();
                Console.WriteLine(string.Format("!> {0}", body));
                var message = await queue.AddMessageAsync(body);
                await Task.Yield();
            }
            Console.WriteLine("Bye from producer");
        }

        public static async Task ProcessMessages(CancellationToken cancel)
        {
            var queue = await client.CreateOrGetQueueAsync("demo_queue");
            while (!cancel.IsCancellationRequested)
            {
                var message = await queue.GetMessageAsync();
                if (message != null)
                {
                    Console.WriteLine(string.Format("?> {0}", message));
                    await queue.DeleteMessageAsync(message.Value);
                }
                await Task.Yield();
            }
            Console.WriteLine("Bye from consumer");
        }
    }
}
