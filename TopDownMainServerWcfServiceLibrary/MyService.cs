using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TopDownMainServerWcfServiceLibrary
{
    public class MyService : IMyService
    {
	    public (string, int) GetAvailableServer() {
			// var matchmaking = new Matchmaking();
			using (TcpClient client = new TcpClient("localhost", 9090)) {
				using (var streamReader = new BinaryReader(client.GetStream())) {
					using (var streamWriter = new BinaryWriter(client.GetStream())) {
						streamWriter.Write(1);
						string address = streamReader.ReadString();
						int port = streamReader.ReadInt32();
						Console.WriteLine($"New server: {address}:{port}");
						return (address, port);
					}
				}
			}
			
			// var matchmakingResult = matchmaking.GetServerAsync(cancellationToken);
			// matchmakingResult.Wait();
			// return ("26.101.252.249", 5000);
			// return "26.104.61.15";
	    }
    }
}
