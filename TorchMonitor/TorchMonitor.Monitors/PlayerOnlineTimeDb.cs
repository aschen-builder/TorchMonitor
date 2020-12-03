﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Utils.General;

namespace TorchMonitor.Monitors
{
    public sealed class PlayerOnlineTimeDb
    {
        class Document
        {
            [JsonConstructor]
            Document()
            {
            }

            public Document(ulong steamId, double onlineTime)
            {
                SteamId = $"{steamId}";
                OnlineTime = onlineTime;
            }

            [JsonProperty("steam_id"), StupidDb.Id]
            public string SteamId { get; private set; }

            [JsonProperty("online_time")]
            public double OnlineTime { get; private set; }
        }

        const string TableName = "online_times";

        readonly StupidDb _localDb;
        readonly Dictionary<ulong, double> _dbCopy;

        internal PlayerOnlineTimeDb(StupidDb localDb)
        {
            _localDb = localDb;
            _dbCopy = new Dictionary<ulong, double>();
        }

        public void Read()
        {
            _dbCopy.Clear();

            var docs = _localDb.Query<Document>(TableName);
            foreach (var doc in docs)
            {
                var steamId = ulong.Parse(doc.SteamId);
                _dbCopy[steamId] = doc.OnlineTime;
            }
        }

        public void IncrementPlayerOnlineTime(ulong steamId, double addedOnlineTime)
        {
            _dbCopy.TryGetValue(steamId, out var onlineTime);
            onlineTime += addedOnlineTime;
            _dbCopy[steamId] = onlineTime;
        }

        public double GetPlayerOnlineTime(ulong steamId)
        {
            _dbCopy.TryGetValue(steamId, out var onlineTime);
            return onlineTime;
        }

        public double GetTotalOnlineTime()
        {
            return _dbCopy.Sum(p => p.Value);
        }

        public void Write()
        {
            var docs = new List<Document>();
            foreach (var (steamId, onlineTime) in _dbCopy)
            {
                var doc = new Document(steamId, onlineTime);
                docs.Add(doc);
            }

            _localDb.Insert(TableName, docs);
            _localDb.Write();
        }
    }
}