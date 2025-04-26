using System;
using System.Collections.Generic;
using Bolt;
using LootRespawnControl.Managers;
using LootRespawnControl.Networking;
using RedLoader;
using RedLoader.Utils;
using Sons.Multiplayer;
using SonsSdk.Networking;
using UdpKit;
using UnityEngine;
using static LootRespawnControl.LootManager;
using static LootRespawnControl.LootManager.LootRespawnManager;
using static LootRespawnControl.Networking.PickupEvent;
using static LootRespawnControl.Networking.PlayerJoin;

namespace LootRespawnControl
{
    //Global Event Listener
    [RegisterTypeInIl2Cpp]
    public class EventHandler : GlobalEventListener
    {
        public static EventHandler Instance;

        public static void Create()
        {
            if (Instance)
                return;

            Instance = new GameObject("LootSyncEventHandler").AddComponent<EventHandler>();
        }

        public override void Connected(BoltConnection connection)
        {
            if (!Config.enableMultiplayer.Value) { return; }
            DebugManager.ConsoleLog("Player connected, SteamID: " + MultiplayerUtilities.GetSteamId(connection));
            NetworkManager.SendConfigData(connection);
        }
    }

    // Networking manager, register and base methods for sending
    internal class NetworkManager
    {
        private static ConfigDataEvent _configDataEvent;
        private static ConfigDataAck _configDataAck;
        private static ConfigSyncConfirmationEvent _configSyncConfirmationEvent;
        private static LootDataEvent _lootDataEvent;
        private static LootDataAck _lootDataAck;
        private static PickupEventHandler _pickupEvent;
        private static PickupEventRequest _pickupRequest;
        private static PickupEventAck _pickupAck;
        private static RespawnEvent.RespawnEventHandler _respawnEvent;
        private static RespawnEvent.RespawnEventRequest _respawnRequest;

        public static void RegisterPackets()
        {
            _configDataEvent = new ConfigDataEvent();
            _configDataAck = new ConfigDataAck();
            _configSyncConfirmationEvent = new ConfigSyncConfirmationEvent();
            _lootDataEvent = new LootDataEvent();
            _lootDataAck = new LootDataAck();
            _pickupEvent = new PickupEventHandler();
            _pickupRequest = new PickupEventRequest();
            _pickupAck = new PickupEventAck();
            _respawnEvent = new RespawnEvent.RespawnEventHandler();
            _respawnRequest = new RespawnEvent.RespawnEventRequest();
            Packets.Register(_configDataEvent);
            Packets.Register(_configDataAck);
            Packets.Register(_configSyncConfirmationEvent);
            Packets.Register(_lootDataEvent);
            Packets.Register(_lootDataAck);
            Packets.Register(_pickupEvent);
            Packets.Register(_pickupRequest);
            Packets.Register(_pickupAck);
            Packets.Register(_respawnEvent);
            Packets.Register(_respawnRequest);
        }

        public static void ResetJoinData()
        {
            PlayerJoin.ConfigDataEvent.receivedConfigData = "";
            PlayerJoin.LootDataEvent.receivedLootData = "";
        }

        public static void SendConfigData(BoltConnection connection)
        {
            _configDataEvent.Send(ConfigManager.currentlySetConfig, connection);
            _configSyncConfirmationEvent.StartTimer(connection);
        }

        public static void SendConfigDataAck()
        {
            _configDataAck.SendAck();
        }

        public static void SendConfigSyncConfirmation(string modVersion, ulong targetId)
        {
            _configSyncConfirmationEvent.Send(modVersion, targetId);
        }

        public static void SendLootData(HashSet<LootData> lootData, BoltConnection connection)
        {
            _lootDataEvent.SendChunkedLootData(lootData, connection);
        }

        public static void SendLootDataAck()
        {
            _lootDataAck.SendAck();
        }

        public static HashSet<LootData> GetHostLootData()
        {
            HashSet<LootData> lootData = LootRespawnManager.collectedLootIds;
            return lootData;
        }

        public static void SendPickupEvent(string pickupName, string pickupHash, int pickupId, long time, GlobalTargets target = GlobalTargets.Everyone)
        {
            _pickupEvent.Send(pickupName, pickupHash, pickupId, time, target);
        }

        public static void SendPickupRequest(string pickupName, string pickupHash, int pickupId, long time)
        {
            _pickupRequest.Send(pickupName, pickupHash, pickupId, time);
        }

        public static void SendPickupAck(string pickupHash, bool confirm, BoltConnection target)
        {
            _pickupAck.Send(pickupHash, confirm, target);
        }

        public static void SendRespawnEvent(string pickupHash)
        {
            _respawnEvent.Send(pickupHash);
        }

        public static void SendRespawnRequest(string pickupName, string pickupHash, int id, bool isBreakable)
        {
            _respawnRequest.Send(pickupName, pickupHash, id, isBreakable);
        }

        public static void Update()
        {
            _configSyncConfirmationEvent.UpdateTimers();
        }
    }
}