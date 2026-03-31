using SteelSurge.Main;
using SteelSurge.Network.Models;
using SteelSurge.Network.Signals;
using System;
using System.Collections.Generic;
using UniRx;
using Unity.Collections;
using Unity.Netcode;
using Zenject;
using Sirenix.OdinInspector;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;
using UnityEngine;

namespace SteelSurge.Network.Handlers
{
    public class NetworkPlayerHandler : SubNetworkHandler
    {
        [Inject] private SignalBus _signalBus;

        [ShowInInspector, ReadOnly]
        private NetworkList<NetworkPlayer> ConnectedPlayers { get; set; }
        private readonly Dictionary<ulong, string> _pendingNames = new();

        // Публичные свойства для тестирования
        public SignalBus SignalBus => _signalBus;

        /// <summary>
        /// Ручная инициализация для тестов (без Zenject).
        /// </summary>
        public void Initialize(SignalBus signalBus, NetworkList<NetworkPlayer> connectedPlayers = null)
        {
            _signalBus = signalBus;
            if (connectedPlayers != null)
            {
                ConnectedPlayers = connectedPlayers;
            }
        }

        private void Awake() => ConnectedPlayers ??= new NetworkList<NetworkPlayer>();

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                Debug.Log("[NetworkPlayerHandler] Spawned (Server)");
                
                _signalBus.GetStream<ConnectionApprovedSignal>()
                    .Subscribe(sig => _pendingNames[sig.ClientId] = sig.PlayerName)
                    .AddTo(this);

                _signalBus.GetStream<PlayerJoinedRoomSignal>()
                    .Subscribe(sig => UpdatePlayerRoom(sig.ClientId, new(sig.InstanceId) ))
                    .AddTo(this);

                _signalBus.GetStream<PlayerLeftRoomSignal>()
                    .Subscribe(sig => UpdatePlayerRoom(sig.ClientId, default))
                    .AddTo(this);

                NetworkManager.Singleton.OnClientConnectedCallback += OnServerClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnServerClientDisconnected;

                if (NetworkManager.Singleton.IsHost)
                {
                    Debug.Log("[NetworkPlayerHandler] Host detected, adding local player");
                    OnServerClientConnected(NetworkManager.Singleton.LocalClientId);
                }
            }
            else
            {
                Debug.Log("[NetworkPlayerHandler] Spawned (Client)");
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                Debug.Log($"[NetworkPlayerHandler] Despawning (Server). Players count: {ConnectedPlayers.Count}");
                NetworkManager.Singleton.OnClientConnectedCallback -= OnServerClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnServerClientDisconnected;
            }
        }

        private void OnServerClientConnected(ulong clientId)
        {
            _pendingNames.TryGetValue(clientId, out string playerName);

            var newPlayer = new NetworkPlayer
            {
                Name = playerName ?? $"Player {clientId}",
                ClientId = clientId,
                InstanceId = Guid.NewGuid()
            };

            ConnectedPlayers.Add(newPlayer);
            _pendingNames.Remove(clientId);

            Debug.Log($"[NetworkPlayerHandler] Player connected: {newPlayer.Name} (ID: {clientId}, Total: {ConnectedPlayers.Count})");
        }

        private void OnServerClientDisconnected(ulong clientId)
        {
            string removedName = "Unknown";
            for (int i = 0; i < ConnectedPlayers.Count; i++)
            {
                if (ConnectedPlayers[i].ClientId == clientId)
                {
                    removedName = ConnectedPlayers[i].Name.ToString();
                    ConnectedPlayers.RemoveAt(i);
                    break;
                }
            }
            _pendingNames.Remove(clientId);

            Debug.Log($"[NetworkPlayerHandler] Player disconnected: {removedName} (ID: {clientId}, Total: {ConnectedPlayers.Count})");
        }

        [Rpc(SendTo.Server)]
        public void UpdatePlayerInstanceRpc(ulong clientId, NetworkGuid instanceId)
        {
            for (int i = 0; i < ConnectedPlayers.Count; i++)
            {
                if (ConnectedPlayers[i].ClientId == clientId)
                {
                    var player = ConnectedPlayers[i];
                    player.InstanceId = instanceId;
                    ConnectedPlayers[i] = player;
                    break;
                }
            }
        }

        private void UpdatePlayerRoom(ulong clientId, NetworkGuid roomGuid)
        {
            for (int i = 0; i < ConnectedPlayers.Count; i++)
            {
                if (ConnectedPlayers[i].ClientId == clientId)
                {
                    var player = ConnectedPlayers[i];
                    player.CurrentRoomId = roomGuid;
                    ConnectedPlayers[i] = player;
                    break;
                }
            }
        }

        public NetworkPlayer GetPlayer(ulong clientId)
        {
            if (ConnectedPlayers == null)
            {
                return NetworkPlayer.Empty;
            }

            for (int i = 0; i < ConnectedPlayers.Count; i++)
            {
                if (ConnectedPlayers[i].ClientId == clientId)
                {
                    return ConnectedPlayers[i];
                }
            }

            return NetworkPlayer.Empty;
        }
    }
}