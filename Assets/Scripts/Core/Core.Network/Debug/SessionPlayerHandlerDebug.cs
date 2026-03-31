using Sirenix.OdinInspector;
using SteelSurge.Core.Network.Handlers;
using SteelSurge.Network.Models;
using Unity.Netcode;
using UnityEngine;

namespace SteelSurge.Core.Network
{
    public class SessionPlayerHandlerDebug : MonoBehaviour
    {
        [Required]
        [SerializeField]
        private SessionPlayerHandler _sessionPlayerHandler;

        [Header("Test Player Settings")]
        [SerializeField]
        private string _testPlayerNamePrefix = "TestPlayer";
        [SerializeField]
        private int _testPlayerCount = 3;
        [SerializeField]
        private byte _testTeamColor = 0;
        [SerializeField]
        private byte _testPlayerTeam = 0;
        [SerializeField]
        private int _testStartClientId = 1000;
        [SerializeField]
        private bool _autoIncrementTeamColor = false;
        [SerializeField]
        private bool _autoIncrementTeam = false;
        [SerializeField]
        private bool _autoIncrementName = true;

        [Header("Remove Settings")]
        [SerializeField]
        private byte _removeTeamColor = 0;

        [Button("Add Test Players")]
        private void AddTestPlayers()
        {
            for (int i = 0; i < _testPlayerCount; i++)
            {
                ulong testClientId = (ulong)_testStartClientId + (ulong)i;
                string playerName = _autoIncrementName 
                    ? $"{_testPlayerNamePrefix}_{i + 1}" 
                    : _testPlayerNamePrefix;

                var testPlayer = new NetworkPlayer
                {
                    ClientId = testClientId,
                    Name = playerName
                };

                byte teamColor = _autoIncrementTeamColor ? (byte)((_testTeamColor + i) % 4) : _testTeamColor;
                byte team = _autoIncrementTeam ? (byte)((_testPlayerTeam + i) % 2) : _testPlayerTeam;

                _sessionPlayerHandler.Add(testPlayer, teamColor: teamColor, playerTeam: team);
            }

            Debug.Log($"[SessionPlayerHandlerDebug] Added {_testPlayerCount} test players");
        }

        [Button("Remove All Players")]
        private void RemoveAllPlayers()
        {

            var handler = _sessionPlayerHandler;
            while (handler.GetSessionPlayersCount() > 0)
            {
                var player = handler.GetByIndex(0);
                if (!player.IsEmpty)
                {
                    handler.Remove(player.ClientId);
                }
                else
                {
                    break;
                }
            }

            Debug.Log("[SessionPlayerHandlerDebug] All players removed");
        }

        [Button("Remove Random Player")]
        private void RemoveRandomPlayer()
        {

            var handler = _sessionPlayerHandler;
            int count = handler.GetSessionPlayersCount();
            if (count == 0)
            {
                Debug.LogWarning("[SessionPlayerHandlerDebug] No players to remove");
                return;
            }

            int randomIndex = Random.Range(0, count);
            var player = handler.GetByIndex(randomIndex);
            if (!player.IsEmpty)
            {
                handler.Remove(player.ClientId);
                Debug.Log($"[SessionPlayerHandlerDebug] Removed random player (index {randomIndex})");
            }
        }

        [Button("Remove Players with Team Color 0")]
        private void RemovePlayersWithTeamColor0()
        {
            RemovePlayersWithTeamColor(0);
        }

        [Button("Remove Players with Specific Team Color")]
        private void RemovePlayersWithSpecificTeamColor()
        {
            RemovePlayersWithTeamColor(_removeTeamColor);
        }

        private void RemovePlayersWithTeamColor(byte teamColor)
        {

            var handler = _sessionPlayerHandler;
            int removedCount = 0;
            for (int i = handler.GetSessionPlayersCount() - 1; i >= 0; i--)
            {
                var player = handler.GetByIndex(i);
                if (!player.IsEmpty && player.TeamColor == teamColor)
                {
                    handler.Remove(player.ClientId);
                    removedCount++;
                }
            }

            Debug.Log($"[SessionPlayerHandlerDebug] Removed {removedCount} players with team color {teamColor}");
        }

        [Button("Log All Players")]
        private void LogAllPlayers()
        {

            var handler = _sessionPlayerHandler;
            int count = handler.GetSessionPlayersCount();
            Debug.Log($"[SessionPlayerHandlerDebug] Total players: {count}");

            for (int i = 0; i < count; i++)
            {
                var player = handler.GetByIndex(i);
                if (!player.IsEmpty)
                {
                    Debug.Log($"[SessionPlayerHandlerDebug] [{i}] {player.PlayerName} (ID: {player.ClientId}, TeamColor: {player.TeamColor}, Team: {player.PlayerTeam}, Energy: {player.CurrentEnergy})");
                }
            }
        }
    }
}
