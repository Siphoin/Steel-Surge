using RenownedGames.AITree;
using RenownedGames.Apex;
using System;
using UnityEngine;

namespace SteelSurge.Core.AI.Decorators
{
    [NodeContent("Decorators/Network Is Owner", "Check: is agent the network owner")]
    public class NetworkIsOwnerDecorator : ObserverDecorator
    {
        [Title("Settings")]
        [Tooltip("Invert check: agent is NOT the owner")]
        [SerializeField]
        private bool invert = false;

        private bool _lastIsOwner;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _lastIsOwner = IsOwner();
        }

        public override event Action OnValueChange;

        public override bool CalculateResult()
        {
            bool isOwner = IsOwner();

            if (_lastIsOwner != isOwner)
            {
                _lastIsOwner = isOwner;
                OnValueChange?.Invoke();
            }

            return invert ? !isOwner : isOwner;
        }

        protected override void OnFlowUpdate() { }

        public override string GetDescription()
        {
            return invert ? "Is NOT Network Owner" : "Is Network Owner";
        }

        private bool IsOwner()
        {
            var owner = GetOwner();
            if (owner == null) return true;

            var networkObject = owner.GetComponent<Unity.Netcode.NetworkObject>();
            if (networkObject == null) return true;

            if (Unity.Netcode.NetworkManager.Singleton == null) return true;

            return networkObject.IsOwner;
        }
    }
}
