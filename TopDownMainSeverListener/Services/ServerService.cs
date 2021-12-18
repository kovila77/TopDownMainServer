using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PostgresEntities.Entities;

namespace TopDownMainSeverListener.Service
{
    public class ServerService
    {
        public void NewServer(Server server)
        {
            using (ServersContext sc = new ServersContext())
            {
                //onsole.WriteLine(sc.Servers.First().Address);
                var ser = sc.Servers.Find(server.Address, server.Port);

                if (ser is null)
                {
                    sc.Servers.Add(server);
                    sc.SaveChanges();
                }
            }
        }
    }
}
