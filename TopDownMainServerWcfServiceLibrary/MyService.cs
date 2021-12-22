using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDownMainServerWcfServiceLibrary
{
    public class MyService : IMyService
    {
	    public string GetServerAddress() {
		    // return "26.101.252.249";
		    return "26.104.61.15";
	    }

	    public string GetServerPort(bool isTCP) {
			return isTCP ? "5000" : "5001";
		}
    }
}
