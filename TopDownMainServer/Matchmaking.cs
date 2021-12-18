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
        public double CountDownTime => 1000 * 1;

        public int MaxPlayers => 8;

        private readonly Timer _timer;

        private readonly LinkedList<CancellationTokenSource> _playersQueue = new LinkedList<CancellationTokenSource>();

        private readonly Dictionary<CancellationTokenSource, MatchmakingResult> _playersServers =
            new Dictionary<CancellationTokenSource, MatchmakingResult>();

        private bool _isMatchmaking = false;

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
                }

                for (int i = 0; i < MaxPlayers; i++)
                {
                    if (_playersQueue.Count < 1)
                    {
                        break;
                    }
                    else
                    {
                        var playerToGame = _playersQueue.First.Value;
                        _playersQueue.RemoveFirst();

                        _playersServers[playerToGame] = availableServer;
                        playerToGame.Cancel();
                    }
                }

                _isMatchmaking = false;
            }
        }



        public async Task<MatchmakingResult> GetServerAsync(CancellationTokenSource cancelSource)
        {
            lock (_playersQueue)
            {
                _playersQueue.AddLast(cancelSource);

                if (!_isMatchmaking && _playersQueue.Count > 1)
                {
                    _timer.Interval = CountDownTime;
                    _timer.Start();
                    _isMatchmaking = true;
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
                else
                {
                    if (_playersQueue.Contains(cancelSource))
                    {
                        _playersQueue.Remove(cancelSource);
                        if (_playersQueue.Count < 2)
                        {
                            _timer.Stop();
                            _isMatchmaking = false;
                        }
                    }

                    return null;
                }
            }
        }

        private MatchmakingResult GetAvailableServer()
        {
            using ServersContext sc = new ServersContext();

            foreach (var server in sc.Servers.Where(x => x.Status == 1))
            {
                var status = ServerService.GetServerStatus(server);

                if (status == 1)
                {
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
