using System.ServiceModel;

namespace TopDownMainServerWcfServiceLibrary {
	[ServiceContract]
	public interface IMyService {
		[OperationContract]
		(string, int) GetAvailableServer();
	}
}