using SteelSurge.Main;
using SteelSurge.Network.Configs;
using SteelSurge.Network.Signals;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Zenject;

namespace SteelSurge.Network.Handlers
{
    public class NetworkHandler : MonoBehaviour, INetworkHandler
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private NetworkHandlerConfig _config;

        private readonly List<SubNetworkHandler> _subHandlers = new();

        // Публичные свойства для тестирования
        public SignalBus SignalBus => _signalBus;
        public NetworkHandlerConfig Config => _config;
        public IReadOnlyList<SubNetworkHandler> SubHandlers => _subHandlers;

        private void Start()
        {
             if (Application.isBatchMode)
            {
                StartServer();
            }
        }

        public T GetSubHandler<T>()
        {
            if (_subHandlers.Count == 0) FindSubHandlers();
            return _subHandlers.OfType<T>().FirstOrDefault();
        }

        public void StartHost()
        {
            SetupAndStart(() => NetworkManager.Singleton.StartHost(), true, true);
        }

        public void StartClient()
        {
            SetupAndStart(() => NetworkManager.Singleton.StartClient(), false, false);
        }

        public void StartServer()
        {
            SetupAndStart(() => NetworkManager.Singleton.StartServer(), true, false);
        }

        /// <summary>
        /// Ручная инициализация для тестов (без Zenject).
        /// </summary>
        public void Initialize(SignalBus signalBus, NetworkHandlerConfig config)
        {
            _signalBus = signalBus;
            _config = config;
        }

        private void SetupAndStart(Func<bool> startAction, bool isServer, bool isHost)
        {
            EnsureNetworkManagerExists();
            ApplyTransportSettings();

            if (isServer) NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

            if (startAction.Invoke())
            {
                _signalBus?.Fire(new NetworkStartedSignal(isServer, isHost, NetworkManager.Singleton.LocalClientId));

                if (isServer)
                {
                    SubscribeServerEvents();
                    SpawnSubHandlers();
                }
                else
                {
                    FindSubHandlers();
                }
            }
        }

        private void SpawnSubHandlers()
        {
            var prefabs = Resources.LoadAll<SubNetworkHandler>("Network/NetworkSubHandlers");
            foreach (var prefab in prefabs)
            {
                var handler = Instantiate(prefab, transform);
                var netObj = handler.GetComponent<NetworkObject>();
                netObj.Spawn(true);
                _subHandlers.Add(handler);
            }
        }

        private void FindSubHandlers()
        {
            _subHandlers.Clear();
            var handlers = FindObjectsByType<SubNetworkHandler>(FindObjectsSortMode.None);
            _subHandlers.AddRange(handlers);
        }

        private void ApplyTransportSettings()
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = _config?.Address ?? "127.0.0.1";
                transport.ConnectionData.Port = _config?.Port ?? 7777;
            }
        }

        public void Shutdown()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();

                foreach (var handler in _subHandlers.Where(handler => handler != null))
                {
                    var netObj = handler.GetComponent<NetworkObject>();
                    if (netObj != null && netObj.IsSpawned) netObj.Despawn();
                }

                Destroy(NetworkManager.Singleton.gameObject);
                _subHandlers.Clear();
            }
        }

        private void EnsureNetworkManagerExists()
        {
            if (NetworkManager.Singleton != null) return;
            if (_config?.NetworkManagerPrefab != null)
            {
                Instantiate(_config.NetworkManagerPrefab);
            }
            else
            {
                // Для тестов: создаём минимальный NetworkManager
                var go = new GameObject("NetworkManager");
                go.AddComponent<NetworkManager>();
            }
        }

        private void SubscribeServerEvents()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += (id) => _signalBus?.Fire(new PlayerJoinedSignal(id));
            NetworkManager.Singleton.OnClientDisconnectCallback += (id) => _signalBus?.Fire(new PlayerLeftSignal(id));
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (request.Payload.Length > 0)
            {
                string playerName = System.Text.Encoding.UTF8.GetString(request.Payload);
                _signalBus?.Fire(new ConnectionApprovedSignal(request.ClientNetworkId, playerName));
            }
            else if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost && request.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
            {
                // Для хоста без payload
                _signalBus?.Fire(new ConnectionApprovedSignal(request.ClientNetworkId, "Host"));
            }

            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Pending = false;
        }

        /// <summary>
        /// Вызывает ApprovalCheck для тестирования.
        /// </summary>
        public NetworkManager.ConnectionApprovalResponse TestApprovalCheck(ulong clientId, string playerName)
        {
            var request = new NetworkManager.ConnectionApprovalRequest
            {
                ClientNetworkId = clientId,
                Payload = System.Text.Encoding.UTF8.GetBytes(playerName ?? "")
            };
            var response = new NetworkManager.ConnectionApprovalResponse();
            ApprovalCheck(request, response);
            return response;
        }
    }
}