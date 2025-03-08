using Alt.Json;
using Bolt;
using RedLoader;
using Sons.Multiplayer;
using SonsSdk.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdpKit;
using UnityEngine;
using static LootRespawnControl.LootManager;
using static LootRespawnControl.LootManager.LootRespawnManager;

namespace LootRespawnControl.Networking
{
    internal class PlayerJoin
    {

        //recieve/send the confirmation that the data was recieved
        internal class LootSyncConfirmationEvent : Packets.NetEvent
        {
            public override string Id => "LootSync_ConfirmationEvent";
            private Dictionary<ulong, float> connectionTimers = new Dictionary<ulong, float>();
            private float timeoutDuration = 30f;

            public void Send(string modVersion, ulong targetId)
            {
                var packet = NewPacket(modVersion.Length * 2, GlobalTargets.OnlyServer);
                packet.Packet.WriteString(modVersion);
                Send(packet);
                RLog.Msg($"Sent confirmation with mod version: {modVersion}");
            }

            private void HandleReceivedConfirmation(string receivedModVersion, BoltConnection connection)
            {
                if (Config.ConsoleLogging.Value)
                {
                    RLog.Msg($"Received confirmation with mod version: {receivedModVersion}   from: " + MultiplayerUtilities.GetSteamId(connection));
                }

                if (LootRespawnControl._modVersion != receivedModVersion)
                {
                    if (Config.ConsoleLogging.Value)
                    {
                        RLog.Msg($"Kicking player due to mismatching version: {receivedModVersion}  SteamID:" + MultiplayerUtilities.GetSteamId(connection));
                    }
                    KickPlayer(connection);
                }
                else
                {
                    if (Config.ConsoleLogging.Value)
                    {
                        RLog.Msg("Loot Sync Confirmed from " + MultiplayerUtilities.GetSteamId(connection));
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
                        RLog.Msg($"Kicked connecting player due to timeout! " + connectionId);
                    }
                    KickPlayer(MultiplayerUtilities.GetConnectionFromSteamId(connectionId));
                    connectionTimers.Remove(connectionId);
                }
            }

            public override void Read(UdpPacket packet, BoltConnection fromConnection)
            {
                var receivedModVersion = packet.ReadString();
                if (!BoltNetwork.isServerOrNotRunning) { return; }
                HandleReceivedConfirmation(receivedModVersion, fromConnection);
            }

            private void KickPlayer(BoltConnection connection)
            {
                connection.Disconnect(new CoopKickToken { Banned = true, KickMessage = "HOST_KICKED_YOU" }.Cast<IProtocolToken>());
            }
        }

        //Send the loot data as well as config data
        internal class LootDataEvent : Packets.NetEvent
        {
            public override string Id => "LootSync_LootDataEvent";

            public void Send(HashSet<LootData> lootData, string configData, BoltConnection connection)
            {
                // Remove any unnecessary data
                HashSet<LootData> networkedLootData = LootManager.LootRespawnManager.GetNetworkedCollected(lootData);

                // Serialize the HashSet to JSON
                string lootDataJson = JsonConvert.SerializeObject(networkedLootData);

                // Combine JSON strings with a separator
                string combinedJson = lootDataJson + "||" + configData;

                combinedJson = combinedJson + "12345678";
                // Calculate packetSize: length of combined JSON string
                int packetSize = sizeof(int) + combinedJson.Length * 2;

                RLog.Msg("Trying to send packet with size: " + packetSize);

                RLog.Msg("Packet data: " + combinedJson);

                var packet = NewPacket(packetSize, connection);

                // Write the combined JSON string
                packet.Packet.WriteString(combinedJson);

                RLog.Msg("Sending data packet (combined JSON)...");
                Send(packet);
            }

            public override void Read(UdpPacket packet, BoltConnection fromConnection)
            {
                RLog.Msg("Received data packet (combined JSON)...");

                // Read the combined JSON string
                string combinedJson = packet.ReadString();

                // Split the combined JSON string
                string[] jsonStrings = combinedJson.Split(new string[] { "||" }, System.StringSplitOptions.None);

                if (jsonStrings.Length != 2)
                {
                    RLog.Error("Invalid combined JSON format received.");
                    return; // Handle error appropriately
                }

                string lootDataJson = jsonStrings[0];
                string configData = jsonStrings[1];

                // Deserialize the HashSet from JSON
                HashSet<LootData> receivedLootData = JsonConvert.DeserializeObject<HashSet<LootData>>(lootDataJson);

                HandleReceivedData(receivedLootData, configData, fromConnection);
            }


            private void HandleReceivedData(HashSet<LootData> receivedLootData, string configData, BoltConnection target)
            {
                if (Config.ConsoleLogging.Value)
                {
                    RLog.Msg("Received Loot Data: " + receivedLootData.Count);
                }
                //Merge the client data with the loot data
                LootRespawnManager.recievedLootIds = receivedLootData;

                ConfigManager.DeserializeConfig(configData);
                if (Config.ConsoleLogging.Value)    
                {
                    RLog.Msg("Recieved Config Data: " + configData);
                }

                NetworkManager.SendLootSyncConfirmation(LootRespawnControl._modVersion, MultiplayerUtilities.GetSteamId(target));
            }
        }
    }
}
