using System;
using System.Collections.Generic;
using Bolt;
using RedLoader;
using RedLoader.Utils;
using Sons.Multiplayer;
using SonsSdk.Networking;
using UdpKit;
using UnityEngine;
using static LootRespawnControl.LootRespawnManager;

namespace LootRespawnControl
{
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
            if (Config.ConsoleLogging.Value)
            {
                RLog.Msg("Player connected");
            }
            NetworkManager.SendLootData(connection);
        }
    }

    internal class LootDataEvent : Packets.NetEvent
    {
        public override string Id => "LootSync_LootDataEvent";

        public void Send(HashSet<LootData> lootData, string configData, BoltConnection connection)
        {
            int packetSize = sizeof(int) +  // Number of loot data entries
                            sizeof(int);  // Length of the message string

            foreach (var data in lootData)
            {
                packetSize += data.Hash.Length * 2 + 8; // Hash length * 2 (UTF-16) + long timestamp
            }

            packetSize += configData.Length * 2; // Message length * 2 (UTF-16) 

            var packet = NewPacket(packetSize, connection);

            packet.Packet.WriteInt(lootData.Count);
            foreach (var data in lootData)
            {
                packet.Packet.WriteString(data.Hash);
                packet.Packet.WriteLong(data.Timestamp);
            }

            packet.Packet.WriteString(configData); // Write the message string

            Send(packet);
        }

        public override void Read(UdpPacket packet, BoltConnection fromConnection)
        {
            int count = packet.ReadInt();
            HashSet<LootData> receivedLootData = new HashSet<LootData>();
            for (int i = 0; i < count; i++)
            {
                string hash = packet.ReadString();
                long timestamp = packet.ReadLong();
                receivedLootData.Add(new LootData(hash, timestamp));
            }

            string configData = packet.ReadString(); // Read the message string

            // Now you have both receivedLootData and message
            HandleReceivedData(receivedLootData, configData);
        }

        private void HandleReceivedData(HashSet<LootData> receivedLootData, string configData)
        {
            if (Config.ConsoleLogging.Value)
            {
                RLog.Msg("Received Loot Data");
            }
           

            LootRespawnManager.collectedLootIds = receivedLootData;

            // Do something with the message, e.g., deserialize it
            ConfigManager.DeserializeConfig(configData);
            if (Config.ConsoleLogging.Value)
            {
                RLog.Msg("Recieved Config Data: " + configData);
            }
            

            NetworkManager.SendLootSyncConfirmation(LootRespawnControl._modVersion);
        }
    }

    internal class LootSyncConfirmationEvent : Packets.NetEvent
    {
        public override string Id => "LootSync_ConfirmationEvent";
        private Dictionary<ulong, float> connectionTimers = new Dictionary<ulong, float>();
        private float timeoutDuration = 20f;

        public void Send(string modVersion, GlobalTargets target = GlobalTargets.Everyone)
        {
            var packet = NewPacket(modVersion.Length * 2, target);
            packet.Packet.WriteString(modVersion);
            Send(packet);
        }

        private void HandleReceivedConfirmation(string receivedModVersion, BoltConnection connection)
        {
            if (Config.ConsoleLogging.Value)
            {
                RLog.Msg($"Received confirmation with mod version: {receivedModVersion}");
            }
            
            if (LootRespawnControl._modVersion != receivedModVersion)
            {
                if (Config.ConsoleLogging.Value)
                {
                    RLog.Msg($"Kicking player due to mismatching version: {receivedModVersion}");
                }
                KickPlayer(connection);
            }
            else
            {
                if (Config.ConsoleLogging.Value)
                {
                    RLog.Msg("Loot Sync Confirmed");
                }
                
                connectionTimers.Remove(MultiplayerUtilities.GetSteamId(connection));
            }
        }

        public void StartTimer(BoltConnection connection)
        {
            connectionTimers[MultiplayerUtilities.GetSteamId(connection)] = Time.time;
        }

        public void UpdateTimers()
        {
            List<ulong> timedOutConnections = new List<ulong>();
            foreach (var pair in connectionTimers)
            {
                if (Time.time - pair.Value > timeoutDuration)
                {
                    timedOutConnections.Add(pair.Key);
                }
            }
            foreach (var connectionId in timedOutConnections)
            {
                if (Config.ConsoleLogging.Value)
                {
                    RLog.Msg($"Kicked connecting player due to timeout!" + connectionId);
                }
                KickPlayer(MultiplayerUtilities.GetConnectionFromSteamId(connectionId));
                connectionTimers.Remove(connectionId);
            }
        }

        public override void Read(UdpPacket packet, BoltConnection fromConnection)
        {
            var receivedModVersion = packet.ReadString();
            HandleReceivedConfirmation(receivedModVersion, fromConnection);
        }

        private void KickPlayer(BoltConnection connection)
        {
            connection.Disconnect(new CoopKickToken { Banned = true, KickMessage = "HOST_KICKED_YOU" }.Cast<IProtocolToken>());
        }
    }

    internal class PickupEvent : Packets.NetEvent
    {
        public override string Id => "LootSync_PickupEvent";

        public void Send(string pickupName, string pickupHash, long time, GlobalTargets target = GlobalTargets.Everyone)
        {
            // Calculate packet size
            int packetSize = pickupName.Length * 2 + pickupHash.Length * 2 + 8; // string length * 2 (UTF-16) + long time

            var packet = NewPacket(packetSize, target);

            packet.Packet.WriteString(pickupName);
            packet.Packet.WriteString(pickupHash);
            packet.Packet.WriteLong(time);

            Send(packet);
        }

        private void HandleNetworkedPickup(string pickupName, string pickupHash, long time)
        {
            // Implement your logic here for handling the received pickup data
            if (Config.ConsoleLogging.Value)
            {
                RLog.Msg($"Received Pickup: {pickupName}, Hash: {pickupHash}, Time: {time}");
            }
            
            LootRespawnControl.HandlePickupDataRecieved(pickupName, pickupHash);
        }

        public override void Read(UdpPacket packet, BoltConnection fromConnection)
        {
            string pickupName = packet.ReadString();
            string pickupHash = packet.ReadString();
            long time = packet.ReadLong();

            HandleNetworkedPickup(pickupName, pickupHash, time);
        }
    }

    internal class NetworkManager
    {
        private static bool _initialized = false;
        public static LootDataEvent _lootDataEvent;
        public static LootSyncConfirmationEvent _lootSyncConfirmationEvent;
        public static PickupEvent _pickupEvent;

        public static void RegisterPackets()
        {
            _lootDataEvent = new LootDataEvent();
            _lootSyncConfirmationEvent = new LootSyncConfirmationEvent();
            _pickupEvent = new PickupEvent();
            Packets.Register(_lootDataEvent);
            Packets.Register(_lootSyncConfirmationEvent);
            Packets.Register(_pickupEvent);
        }

        public static void SendLootData(BoltConnection connection)
        {
            HashSet<LootData> hostLootData = GetHostLootData();
            _lootDataEvent.Send(hostLootData, Config.Serialize(), connection);
            _lootSyncConfirmationEvent.StartTimer(connection);
        }

        public static void SendLootSyncConfirmation(string modVersion)
        {
            _lootSyncConfirmationEvent.Send(modVersion);
        }

        private static HashSet<LootData> GetHostLootData()
        {
            HashSet<LootData> lootData = LootRespawnManager.collectedLootIds;
            return lootData;
        }

        public static void SendPickupEvent(string pickupName, string pickupHash, long time, GlobalTargets target = GlobalTargets.Everyone)
        {
            _pickupEvent.Send(pickupName, pickupHash, time, target);
        }

        public static void Update()
        {
            _lootSyncConfirmationEvent.UpdateTimers();
        }
    }
}