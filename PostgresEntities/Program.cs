using System;
using System.Linq;
using PostgresEntities.Entities;

namespace PostgresEntities
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello");

            using (ServersContext sc = new ServersContext())
            {
                Console.WriteLine(sc.Servers.First().Address);
            }


            Console.WriteLine("!");
        }
    }
}
