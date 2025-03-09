using System;
using System.Collections.Generic;
using Bolt;
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
            if (Config.ConsoleLogging.Value){ RLog.Msg("Player connected, SteamID: " + MultiplayerUtilities.GetSteamId(connection)); }
            NetworkManager.SendConfigData(connection);
        }
    }

    // Networking manager, register and base methods for sending

    internal class NetworkManager
    {
        public static ConfigDataEvent _configDataEvent;
        public static ConfigSyncConfirmationEvent _configSyncConfirmationEvent;
        public static LootDataEvent _lootDataEvent;
        public static LootDataAck _lootDataAck;
        public static PickupEventHandler _pickupEvent;
        public static PickupEventRequest _pickupRequest;
        public static PickupEventAck _pickupAck;

        public static void RegisterPackets()
        {
            _configDataEvent = new ConfigDataEvent();
            _configSyncConfirmationEvent = new ConfigSyncConfirmationEvent();
            _lootDataEvent = new LootDataEvent();
            _lootDataAck = new LootDataAck();
            _pickupEvent = new PickupEventHandler();
            _pickupRequest = new PickupEventRequest();
            _pickupAck = new PickupEventAck();
            Packets.Register(_configDataEvent);
            Packets.Register(_configSyncConfirmationEvent);
            Packets.Register(_lootDataEvent);
            Packets.Register(_lootDataAck);
            Packets.Register(_pickupEvent);
            Packets.Register(_pickupRequest);
            Packets.Register(_pickupAck);
        }

        public static void SendConfigData(BoltConnection connection)
        {
            HashSet<LootData> hostLootData = GetHostLootData();
            _configDataEvent.Send(hostLootData, Config.Serialize(), connection);
            _configSyncConfirmationEvent.StartTimer(connection);
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

        public static void Update()
        {
            _configSyncConfirmationEvent.UpdateTimers();
        }
    }
}