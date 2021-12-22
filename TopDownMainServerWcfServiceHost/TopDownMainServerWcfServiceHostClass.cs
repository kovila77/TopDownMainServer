using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace TopDownMainServerWcfServiceHost
{
	public class TopDownMainServerWcfServiceHostClass
	{
		static void Main(string[] args)
		{
			ServiceMetadataBehavior behavior = new ServiceMetadataBehavior
			{
				HttpGetEnabled = true,
				MetadataExporter = { PolicyVersion = PolicyVersion.Policy15 }
			};
			var host = new ServiceHost(typeof(TopDownMainServerWcfServiceLibrary.MyService), new Uri("http://26.101.252.249:5003/MyService"));
			host.Description.Behaviors.Add(behavior);
			host.AddServiceEndpoint(typeof(TopDownMainServerWcfServiceLibrary.IMyService), new BasicHttpBinding(), "basic");
			// host.AddServiceEndpoint(typeof(TopDownMainServerWcfServiceLibrary.IMyService), new WSHttpBinding(), "ws");
			// host.AddServiceEndpoint(typeof(TopDownMainServerWcfServiceLibrary.IMyService), new NetTcpBinding(), "net.tcp://localhost:13054/our/service/tcp");
			// host.AddServiceEndpoint(typeof(TopDownMainServerWcfServiceLibrary.IMyService), new NetNamedPipeBinding(), "net.pipe://localhost/our/service/pipe");
			host.Open();
			Console.WriteLine("Server is running");
			Console.ReadLine();
        }
	}
}
