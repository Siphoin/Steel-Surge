using UnityEngine;
using Zenject;
using UniRx;
using SteelSurge.Network.Handlers;
using SteelSurge.Network.Signals;
using SteelSurge.Main;
using SteelSurge.Core.InputSystem;
using SteelSurge.Network.Configs;

namespace SteelSurge.Tests
{
    public class NetworkBehaviorTest : MonoBehaviour
    {
        [Inject] private INetworkHandler _networkHandler;
        [Inject] private IInputSystem _inputSystem;
        [Inject] private SignalBus _signalBus;
        [Inject] private NetworkHandlerConfig _config;

        private void Start()
        {
            _signalBus.GetStream<NetworkStartedSignal>()
                .Subscribe(signal => Debug.Log($"[Test] Network Started. ID: {signal.LocalClientId}"))
                .AddTo(this);

            _inputSystem.AddListener(OnKeyDown, StandaloneInputEventType.KeyDown);
        }

        private void OnKeyDown(KeyCode keyCode)
        {
            if (keyCode == _config.GetHotkey("Host")) _networkHandler.StartHost();
            else if (keyCode == _config.GetHotkey("Client")) _networkHandler.StartClient();
            else if (keyCode == _config.GetHotkey("Server")) _networkHandler.StartServer();
            else if (keyCode == _config.GetHotkey("Shutdown")) _networkHandler.Shutdown();
        }

        private void OnDestroy()
        {
            _inputSystem.RemoveListener(OnKeyDown, StandaloneInputEventType.KeyDown);
        }
    }
}