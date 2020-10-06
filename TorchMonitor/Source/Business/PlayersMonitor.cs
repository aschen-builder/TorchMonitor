﻿using Sandbox.Game.World;
using Torch.Server.InfluxDb;

namespace TorchMonitor.Business
{
    public class PlayersMonitor : IMonitor
    {
        readonly InfluxDbClient _client;

        public PlayersMonitor(InfluxDbClient client)
        {
            _client = client;
        }

        public void OnInterval(int intervalsSinceStart)
        {
            if (intervalsSinceStart % 10 != 0) return;

            var playerCount = MySession.Static.Players.GetOnlinePlayerCount();
            var point = _client.MakePointIn("server")
                .Field("players", playerCount);

            _client.WritePoints(point);
        }
    }
}