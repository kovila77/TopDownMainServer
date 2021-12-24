using System;
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
            var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9090);
            listener.Start();
            Matchmaking matchmaking = new Matchmaking();
            while (true)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();

                Task.Run(() =>
                {
                    try
                    {
                        //using BinaryReader br = new BinaryReader(tcpClient.GetStream());
                        using BinaryWriter bw = new BinaryWriter(tcpClient.GetStream());
                        CancellationTokenSource cancellationToken = new CancellationTokenSource();
                        cancellationToken.CancelAfter(TimeSpan.FromSeconds(60));
                        var matchmakingResult = matchmaking.GetServerAsync(cancellationToken);
                        matchmakingResult.Wait();

                        if (matchmakingResult.Result is null)
                        {
                            bw.Write("");
                            bw.Write(-1);
                            Console.WriteLine($"  {-1}");
                        }
                        else
                        {
                            bw.Write(matchmakingResult.Result.ServerAddress);
                            bw.Write(matchmakingResult.Result.ServerPort);
                            Console.WriteLine($"{matchmakingResult.Result.ServerAddress}   {matchmakingResult.Result.ServerPort}");
                        }
                        tcpClient.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        tcpClient.Close();
                    }
                    tcpClient.Dispose();
                });
            }
        }
    }
}