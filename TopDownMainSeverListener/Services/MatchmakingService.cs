using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TopDownMainServer;

namespace TopDownMainSeverListener.Services
{
    public class MatchmakingService
    {
        public void Run()
        {
            var listener = new TcpListener(IPAddress.Parse(ConfigurationManager.AppSettings["ServerMatchmakingAddress"]!), int.Parse(ConfigurationManager.AppSettings["ServerMatchmakingPort"]!));
            listener.Start();
            Matchmaking matchmaking = new Matchmaking();
            while (true)
            {
                var c = listener.AcceptTcpClient();
                Task.Run(() => SendServerInfoToClient(c, matchmaking));
            }
        }

        private static async Task SendServerInfoToClient(TcpClient tcpClient, Matchmaking matchmaking)
        {
            try
            {
                CancellationTokenSource cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(TimeSpan.FromSeconds(60));
                var matchmakingResult = await matchmaking.GetServerAsync(cancellationToken);

                await using BinaryWriter bw = new BinaryWriter(tcpClient.GetStream());
                if (matchmakingResult is null)
                {
                    bw.Write("");
                    bw.Write(-1);
                    Console.WriteLine($"  {-1}");
                }
                else
                {
                    bw.Write(matchmakingResult.ServerAddress);
                    bw.Write(matchmakingResult.ServerPort);
                    Console.WriteLine($"{matchmakingResult.ServerAddress}   {matchmakingResult.ServerPort}");
                }

                tcpClient.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                tcpClient.Close();
            }

            tcpClient.Dispose();
        }
    }
}