using System;
using System.Threading;
using System.Threading.Tasks;

namespace TopDownMainServer
{
    class Program
    {
        static Matchmaking matchmaking = new Matchmaking();

        static async Task Main(string[] args)
        {


            Thread.Sleep(1000);

            var t1 = Task.Run(Action);

            Thread.Sleep(1000);

            var t2 = Task.Run(Action);

            t1.Wait();
            t2.Wait();


            Thread.Sleep(1000);

            var t3 = Task.Run(Action);
            var t5 = Task.Run(Action);

            Thread.Sleep(1000);

            var t4 = Task.Run(Action);

            t3.Wait();
            t4.Wait();
            t5.Wait();
        }

        private async static Task Action()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var ser = await matchmaking.GetServerAsync(cts);

            if (ser is null)
                Console.WriteLine("No server");
            else
                Console.WriteLine(ser.ServerPort);
        }
    }
}
