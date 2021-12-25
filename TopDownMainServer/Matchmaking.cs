using PostgresEntities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace TopDownMainServer
{
    public class Matchmaking
    {
        public double CountDownTime => 1000 * 20;

        public int MaxPlayers => 8;

        private readonly Timer _timer;

        private readonly LinkedList<CancellationTokenSource> _playersQueue = new();

        private readonly Dictionary<CancellationTokenSource, MatchmakingResult> _playersServers = new();

        private bool _timerGoing = false;

        private Dictionary<Server, DateTime> _usedServers = new();

        public Matchmaking()
        {
            _timer = new Timer();

            _timer.AutoReset = false;

            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_playersQueue)
            {
                var availableServer = GetAvailableServer();
                if (availableServer == null) // no available servers
                {
                    _timer.Interval = CountDownTime;
                    _timer.Start();
                    return;
                }

                for (int i = 0; i < MaxPlayers; i++)
                {
                    if (_playersQueue.Count < 1)
                    {
                        break;
                    }

                    var playerToGame = _playersQueue.First.Value;
                    _playersQueue.RemoveFirst();

                    _playersServers[playerToGame] = availableServer;
                    playerToGame.Cancel();
                }

                _timer.Stop();
                _timerGoing = false;
            }
        }
        
        public async Task<MatchmakingResult> GetServerAsync(CancellationTokenSource cancelSource)
        {
            lock (_playersQueue)
            {
                Console.WriteLine($"new player. Players in queue {_playersQueue.Count + 1}");
                _playersQueue.AddLast(cancelSource);

                if (!_timerGoing && _playersQueue.Count > 1)
                {
                    _timer.Start();
                    _timerGoing = true;
                }
            }

            await Task.Run(() => cancelSource.Token.WaitHandle.WaitOne()); // wait for server

            return GetServer(cancelSource);
        }

        private MatchmakingResult GetServer(CancellationTokenSource cancelSource)
        {
            lock (_playersQueue)
            {
                if (_playersServers.ContainsKey(cancelSource))
                {
                    var result = _playersServers[cancelSource];
                    _playersServers.Remove(cancelSource);
                    return result;
                }

                if (_playersQueue.Contains(cancelSource))
                {
                    _playersQueue.Remove(cancelSource);
                    if (_playersQueue.Count < 2)
                    {
                        _timer.Stop();
                        _timerGoing = false;
                    }
                }

                return null;
            }
        }

        private MatchmakingResult GetAvailableServer()
        {
            List<Server> rem = new List<Server>();
            foreach (var ser in _usedServers)
            {
                if ((DateTime.Now - ser.Value).TotalSeconds > 10)
                {
                    rem.Add(ser.Key);
                }
            }
            rem.ForEach(x => _usedServers.Remove(x));

            using ServersContext sc = new ServersContext();


            foreach (var server in sc.Servers.Where(x => x.Status == 1))
            {
                if (_usedServers.Any(x => x.Key.Address == server.Address && x.Key.Port == server.Port)) continue;

                var status = ServerService.GetServerStatus(server);

                if (status == 1)
                {
                    _usedServers.Add(server, DateTime.Now);

                    return new MatchmakingResult()
                    {
                        ServerAddress = server.Address,
                        ServerPort = server.Port
                    };
                }
            }

            return null;
        }
    }
}
