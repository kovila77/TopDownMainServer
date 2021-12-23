using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TopDownMainServerWcfServiceLibrary
{
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class MyService : IMyService
    {
	    public (string, int) GetAvailableServer() {
		    try {
				using (TcpClient client = new TcpClient("localhost", 9090))
				{
					using (var streamReader = new BinaryReader(client.GetStream()))
					{
						using (var streamWriter = new BinaryWriter(client.GetStream()))
						{
							streamWriter.Write(1);
							string address = streamReader.ReadString();
							int port = streamReader.ReadInt32();
							Console.WriteLine($"New server: {address}:{port}");
							return (address, port);
						}
					}
				}
			} catch (Exception e) {
				Console.WriteLine(e);
			}

			return (null, -1);
	    }
    }
}
