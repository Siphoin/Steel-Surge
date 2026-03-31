using SteelSurge.Network.Handlers;
using SteelSurge.Network.Models;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SteelSurge.Core.Network.Handlers
{
    public class SessionPlayerHandler : SubNetworkHandler
    {
        private NetworkList<SessionPlayer> _sessionPlayers = new();
        private readonly Dictionary<ulong, SessionPlayer> _playerCache = new();

            public SessionPlayer LocalPlayer {
            get
            {
                ulong localClientId = NetworkManager.Singleton.LocalClientId;
                return Get(localClientId);
            }
        }

        public void Add(NetworkPlayer networkPlayer, byte teamColor = 0, byte playerTeam = 0)
        {
            if (!IsServer) return;

            var sessionPlayer = new SessionPlayer(networkPlayer, teamColor, playerTeam);
            _sessionPlayers.Add(sessionPlayer);
            _playerCache[sessionPlayer.ClientId] = sessionPlayer;

            Debug.Log($"[SessionPlayerHandler] SessionPlayer added: {sessionPlayer.PlayerName} (ID: {sessionPlayer.ClientId}, Total: {_sessionPlayers.Count})");
        }

        public void Remove(ulong clientId)
        {
            if (!IsServer) return;

            for (int i = 0; i < _sessionPlayers.Count; i++)
            {
                if (_sessionPlayers[i].ClientId == clientId)
                {
                    var playerName = _sessionPlayers[i].PlayerName;
                    _sessionPlayers.RemoveAt(i);
                    _playerCache.Remove(clientId);

                    Debug.Log($"[SessionPlayerHandler] SessionPlayer removed: {playerName} (ID: {clientId}, Total: {_sessionPlayers.Count})");
                    break;
                }
            }
        }

        public SessionPlayer Get(ulong clientId)
        {
            if (_playerCache.TryGetValue(clientId, out var cached))
            {
                return cached;
            }

            for (int i = 0; i < _sessionPlayers.Count; i++)
            {
                if (_sessionPlayers[i].ClientId == clientId)
                {
                    _playerCache[clientId] = _sessionPlayers[i];
                    return _sessionPlayers[i];
                }
            }

            return default;
        }

        public void UpdateEnergy(ulong clientId, int energy)
        {
            if (!IsServer) return;

            for (int i = 0; i < _sessionPlayers.Count; i++)
            {
                if (_sessionPlayers[i].ClientId == clientId)
                {
                    var player = _sessionPlayers[i];
                    player.CurrentEnergy = energy;
                    _sessionPlayers[i] = player;
                    _playerCache[clientId] = player;
                    break;
                }
            }
        }

        public void UpdateTeamColor(ulong clientId, byte teamColor)
        {
            if (!IsServer) return;

            for (int i = 0; i < _sessionPlayers.Count; i++)
            {
                if (_sessionPlayers[i].ClientId == clientId)
                {
                    var player = _sessionPlayers[i];
                    player.TeamColor = teamColor;
                    _sessionPlayers[i] = player;
                    _playerCache[clientId] = player;
                    break;
                }
            }
        }
    }
}
