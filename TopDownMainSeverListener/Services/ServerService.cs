﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using PostgresEntities.Entities;

namespace TopDownMainSeverListener.Service
{
    public class ServerService
    {
        private static System.Timers.Timer _timer;

        public ServerService()
        {
            if (_timer is null)
            {
                _timer = new Timer();

                _timer.AutoReset = false;

                _timer.Interval = GetInterval();

                _timer.Elapsed += TimerOnElapsed;
                _timer.Start();
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Updating servers status...");
            UpdateServersStatus();
            _timer.Interval = GetInterval();
            _timer.Start();
        }

        static double GetInterval()
        {
            return 1000 * 5;
        }


        public void NewServer(Server server)
        {
            using (ServersContext sc = new ServersContext())
            {
                var ser = sc.Servers.Find(server.Address, server.Port);

                if (ser is null)
                {
                    sc.Servers.Add(server);
                }
                else
                {
                    ser.Status = server.Status;
                    ser.Info = server.Info;
                }

                sc.SaveChanges();
            }
        }

        public void UpdateServersStatus(bool deleteBadOnes = false)
        {
            using (ServersContext sc = new ServersContext())
            {
                Parallel.ForEach(sc.Servers.ToList(), s =>
               {
                   int status = GetServerStatus(s);

                   if (status == -1)
                   {
                       sc.Servers.Remove(s);
                   }
                   else
                   {
                       s.Status = status;
                   }
               });
                sc.SaveChanges();
            }
        }

        public int GetServerStatus(Server server)
        {
            try
            {
                TcpClient tcpClient = new TcpClient(server.Address, server.Port);
                tcpClient.SendTimeout = 1000 * 15;
                tcpClient.ReceiveTimeout = 1000 * 15;

                using BinaryReader socketBinaryReader = new BinaryReader(tcpClient.GetStream());
                using BinaryWriter socketBinaryWriter = new BinaryWriter(tcpClient.GetStream());
                socketBinaryWriter.Write(1);

                int response = socketBinaryReader.ReadInt32();
                if (response is 1 or 2)
                {
                    return response;
                }
                else
                {
                    throw new Exception("Unknown server status");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }
    }
}
