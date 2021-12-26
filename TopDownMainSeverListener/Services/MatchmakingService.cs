using System;
using System.Configuration;
using System.IO;
using System.Linq;
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
            var ipAddresses = Array.FindAll(
                Dns.GetHostEntry(Dns.GetHostName()).AddressList,
                a => a.AddressFamily == AddressFamily.InterNetwork);
            var listener = new TcpListener(ipAddresses.First(), 
                Convert.ToInt32(Environment.GetEnvironmentVariable("TOPDOWN_GAMESERVER_PORT")));
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