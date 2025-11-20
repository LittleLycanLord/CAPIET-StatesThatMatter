using System;
using LilLycanLord_Official;
using UnityEngine;

namespace LilLycanLord_Official
{
    /// <summary>
    /// Example implementation showing how to use the new GameEventManager with the HasSignals pattern.
    /// This demonstrates both the modern type-safe approach and legacy string-based approach.
    /// </summary>
    public class ExampleSignalUser : HasSignalsBase
    {
        [Header("Example Settings")]
        [SerializeField]
        private float health = 100f;

        [SerializeField]
        private int playerLevel = 1;

        [SerializeField]
        private bool useModernEvents = true;

        protected override void OnInitializeSignals()
        {
            if (useModernEvents)
            {
                InitializeModernEvents();
            }
            else
            {
                InitializeLegacyEvents();
            }
        }

        protected override void OnCleanupSignals()
        {
            if (useModernEvents)
            {
                CleanupModernEvents();
            }
            else
            {
                CleanupLegacyEvents();
            }
        }

        /// <summary>
        /// Example of using the new type-safe event system.
        /// </summary>
        private void InitializeModernEvents()
        {
            // Subscribe to type-safe events with compile-time checking
            Subscribe(GameEvents.PlayerSpawned, OnPlayerSpawned);
            Subscribe(GameEvents.GameStarted, OnGameStarted);
            Subscribe(GameEvents.PlayerHealthChanged, OnPlayerHealthChanged);
        }

        private void CleanupModernEvents()
        {
            // Unsubscribe from type-safe events
            Unsubscribe(GameEvents.PlayerSpawned, OnPlayerSpawned);
            Unsubscribe(GameEvents.GameStarted, OnGameStarted);
            Unsubscribe(GameEvents.PlayerHealthChanged, OnPlayerHealthChanged);
        }

        /// <summary>
        /// Example of using the legacy string-based event system (backwards compatibility).
        /// </summary>
        private void InitializeLegacyEvents()
        {
            // Subscribe to string-based events (legacy approach)
            GameEventManager.Instance.AddAction("Player_Spawned", OnPlayerSpawnedLegacy);
            GameEventManager.Instance.AddAction("Game_Started", OnGameStartedLegacy);
            GameEventManager.Instance.AddAction(
                "Player_Health_Changed",
                OnPlayerHealthChangedLegacy
            );
        }

        private void CleanupLegacyEvents()
        {
            // Unsubscribe from string-based events
            GameEventManager.Instance.RemoveAction("Player_Spawned", OnPlayerSpawnedLegacy);
            GameEventManager.Instance.RemoveAction("Game_Started", OnGameStartedLegacy);
            GameEventManager.Instance.RemoveAction(
                "Player_Health_Changed",
                OnPlayerHealthChangedLegacy
            );
        }

        // Modern event handlers (type-safe)
        private void OnPlayerSpawned(GameObject player)
        {
            Debug.Log($"ExampleSignalUser: Player spawned - {player.name}");
        }

        private void OnGameStarted()
        {
            Debug.Log("ExampleSignalUser: Game started!");
        }

        private void OnPlayerHealthChanged((GameObject player, float health, float maxHealth) data)
        {
            Debug.Log(
                $"ExampleSignalUser: Player {data.player.name} health: {data.health}/{data.maxHealth}"
            );
        }

        // Legacy event handlers (string-based)
        private void OnPlayerSpawnedLegacy()
        {
            Debug.Log("ExampleSignalUser: Player spawned (legacy)");
        }

        private void OnGameStartedLegacy()
        {
            Debug.Log("ExampleSignalUser: Game started (legacy)");
        }

        private void OnPlayerHealthChangedLegacy()
        {
            Debug.Log("ExampleSignalUser: Player health changed (legacy)");
        }

        // Example of raising events
        [ContextMenu("Test Modern Events")]
        private void TestModernEvents()
        {
            // Raise modern type-safe events
            RaiseSignal(GameEvents.PlayerSpawned, gameObject);
            RaiseSignal(GameEvents.GameStarted);
            RaiseSignal(GameEvents.PlayerHealthChanged, (gameObject, health, 100f));
        }

        [ContextMenu("Test Legacy Events")]
        private void TestLegacyEvents()
        {
            // Raise legacy string-based events
            GameEventManager.Instance.SendSignal("Player_Spawned");
            GameEventManager.Instance.SendSignal("Game_Started");
            GameEventManager.Instance.SendSignal("Player_Health_Changed");
        }

        [ContextMenu("Damage Player")]
        private void DamagePlayer()
        {
            health -= 10f;
            health = Mathf.Max(0f, health);

            if (useModernEvents)
            {
                RaiseSignal(GameEvents.PlayerHealthChanged, (gameObject, health, 100f));
            }
            else
            {
                GameEventManager.Instance.SendSignal("Player_Health_Changed");
            }

            if (health <= 0f)
            {
                if (useModernEvents)
                {
                    RaiseSignal(GameEvents.PlayerDied, gameObject);
                }
                else
                {
                    GameEventManager.Instance.SendSignal("Player_Died");
                }
            }
        }

        [ContextMenu("Level Up Player")]
        private void LevelUpPlayer()
        {
            playerLevel++;

            if (useModernEvents)
            {
                RaiseSignal(GameEvents.PlayerLevelUp, (gameObject, playerLevel));
            }
            else
            {
                GameEventManager.Instance.SendSignal("Player_Level_Up");
            }
        }

        private void Update()
        {
            // Example of manual registration if not using auto-initialization
            if (Input.GetKeyDown(KeyCode.R) && !SignalsInitialized)
            {
                GameEventManager.Instance.RegisterSignalObject(this);
            }
        }
    }
}
