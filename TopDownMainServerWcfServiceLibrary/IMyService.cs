using System.ServiceModel;

namespace TopDownMainServerWcfServiceLibrary {
	[ServiceContract]
	public interface IMyService {
		[OperationContract]
		string GetServerAddress();

		[OperationContract]
		string GetServerPort(bool isTCP);
	}
}