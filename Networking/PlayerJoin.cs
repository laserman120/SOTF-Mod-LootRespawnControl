using Alt.Json;
using Bolt;
using LootRespawnControl.Managers;
using RedLoader;
using Sons.Multiplayer;
using SonsSdk.Networking;
using System.Collections;
using UdpKit;
using UnityEngine;
using static LootRespawnControl.LootManager;
using static LootRespawnControl.LootManager.LootRespawnManager;

namespace LootRespawnControl.Networking
{
    internal class PlayerJoin
    {

        //recieve/send the confirmation that the data was recieved
        internal class ConfigSyncConfirmationEvent : Packets.NetEvent
        {
            public override string Id => "LootSync_ConfirmationEvent";
            private Dictionary<ulong, float> connectionTimers = new Dictionary<ulong, float>();
            private float timeoutDuration = 5f;

            public void Send(string modVersion, ulong targetId)
            {
                var packet = NewPacket(modVersion.Length * 2, GlobalTargets.OnlyServer);
                packet.Packet.WriteString(modVersion);
                Send(packet);
                DebugManager.ConsoleLog($"Sent confirmation with mod version: {modVersion}");
            }

            private void HandleReceivedConfirmation(string receivedModVersion, BoltConnection connection)
            {
                DebugManager.ConsoleLog($"Received confirmation with mod version: {receivedModVersion}   from: " + MultiplayerUtilities.GetSteamId(connection));

                if (LootRespawnControl._modVersion != receivedModVersion)
                {
                    DebugManager.ConsoleLog($"Kicking player due to mismatching version: {receivedModVersion}  SteamID:" + MultiplayerUtilities.GetSteamId(connection));
                    
                    KickPlayer(connection);
                    connectionTimers.Remove(MultiplayerUtilities.GetSteamId(connection));
                }
                else
                {
                    DebugManager.ConsoleLog("Config Sync Confirmed from " + MultiplayerUtilities.GetSteamId(connection));
                    

                    connectionTimers.Remove(MultiplayerUtilities.GetSteamId(connection));
                    NetworkManager.SendLootData(NetworkManager.GetHostLootData(), connection);
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
                    DebugManager.ConsoleLog($"Kicked connecting player due to timeout! " + connectionId);
                    
                    if(MultiplayerUtilities.GetConnectionFromSteamId(connectionId) == null) 
                    {
                        connectionTimers.Remove(connectionId);
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
                connection.Disconnect(new CoopKickToken { Banned = false, KickMessage = "HOST_KICKED_YOU" }.Cast<IProtocolToken>());
            }
        }

        //Send the loot data as well as config data
        internal class ConfigDataEvent : Packets.NetEvent
        {
            public override string Id => "ConfigSync_LootDataEvent";

            private const int CHUNK_SIZE = 1024; // * 2 due to UTF 16 therefore 2048
            private const string COMPLETE_MARKER = "COMPLETE";

            public static string receivedConfigData = "";

            public void Send(string configData, BoltConnection connection)
            {
                SendConfigDataChunks(configData, connection).RunCoro();
            }

            private IEnumerator SendConfigDataChunks(string lootDataJson, BoltConnection connection)
            {
                for (int i = 0; i < lootDataJson.Length; i += CHUNK_SIZE)
                {
                    string chunk = lootDataJson.Substring(i, Mathf.Min(CHUNK_SIZE, lootDataJson.Length - i));
                    SendConfigDataChunk(chunk, connection);
                    while (!LootDataAckReceived(connection)) // Check every frame
                    {
                        yield return null;
                    }
                }
                SendConfigDataChunk(COMPLETE_MARKER, connection);
                while (!LootDataAckReceived(connection)) // Check every frame for final ACK
                {
                    yield return null;
                }
            }

            private void SendConfigDataChunk(string chunk, BoltConnection connection)
            {
                int packetSize = sizeof(int) + chunk.Length * 2;
                var packet = NewPacket(packetSize, connection);
                DebugManager.ConsoleLog("Sending chunk with size: " + packetSize + "    Packet Data:   " + chunk);
                packet.Packet.WriteString(chunk);
                Send(packet);
                ClearAckFlag(connection); // Clear the ACK flag before sending
            }

            public override void Read(UdpPacket packet, BoltConnection fromConnection)
            {
                string chunk = packet.ReadString();

                if (chunk == COMPLETE_MARKER)
                {
                    HandleReceivedConfigData(receivedConfigData, fromConnection);
                    receivedConfigData = "";
                }
                else
                {
                    receivedConfigData += chunk;
                    DebugManager.ConsoleLog("Received config data chunk...");
                    NetworkManager.SendConfigDataAck(); // Send ACK
                }
            }

            private void HandleReceivedConfigData(string configData, BoltConnection target)
            {
                //HANDLE DATA
                DebugManager.ConsoleLog("Finished receiving Config Data  " + configData);
                ConfigManager.DeserializeConfig(configData);
                LootRespawnControl.recievedConfigData = true;
                NetworkManager.SendConfigSyncConfirmation(LootRespawnControl._modVersion, MultiplayerUtilities.GetSteamId(target));
            }

            // ACK Handling
            private static Dictionary<ulong, bool> ackReceived = new Dictionary<ulong, bool>();

            private bool LootDataAckReceived(BoltConnection connection)
            {
                if (ackReceived.ContainsKey(MultiplayerUtilities.GetSteamId(connection)) && ackReceived[MultiplayerUtilities.GetSteamId(connection)])
                {
                    return true;
                }
                return false;
            }

            private void ClearAckFlag(BoltConnection connection)
            {
                if (!ackReceived.ContainsKey(MultiplayerUtilities.GetSteamId(connection)))
                {
                    ackReceived[MultiplayerUtilities.GetSteamId(connection)] = false;
                }
                else
                {
                    ackReceived[MultiplayerUtilities.GetSteamId(connection)] = false;
                }
            }

            public static void SetAckReceived(BoltConnection connection)
            {
                ackReceived[MultiplayerUtilities.GetSteamId(connection)] = true;
            }
        }

        internal class ConfigDataAck : Packets.NetEvent
        {
            public override string Id => "ConfigSync_LootDataAck";

            public void SendAck(GlobalTargets target = GlobalTargets.OnlyServer)
            {
                var packet = NewPacket(16, target);
                packet.Packet.WriteString("ACK");
                Send(packet);
                DebugManager.ConsoleLog("Sent Config Data ACK");
            }

            public override void Read(UdpPacket packet, BoltConnection fromConnection)
            {
                DebugManager.ConsoleLog("Received Config Data ACK");
                ConfigDataEvent.SetAckReceived(fromConnection);
            }
        }


        internal class LootDataEvent : Packets.NetEvent
        {
            public override string Id => "LootSync_LootDataEvent";

            private const int CHUNK_SIZE = 1024; // * 2 due to UTF 16 therefore 2048
            private const string COMPLETE_MARKER = "COMPLETE";

            public static string receivedLootData = "";

            public void SendChunkedLootData(HashSet<LootData> lootData, BoltConnection connection)
            {
                HashSet<LootData> networkedLootData = LootManager.LootRespawnManager.GetNetworkedCollected(lootData);
                string lootDataJson = JsonConvert.SerializeObject(networkedLootData);
                SendLootDataChunks(lootDataJson, connection).RunCoro();
            }

            private IEnumerator SendLootDataChunks(string lootDataJson, BoltConnection connection)
            {
                for (int i = 0; i < lootDataJson.Length; i += CHUNK_SIZE)
                {
                    string chunk = lootDataJson.Substring(i, Mathf.Min(CHUNK_SIZE, lootDataJson.Length - i));
                    SendLootDataChunk(chunk, connection);
                    while (!LootDataAckReceived(connection)) // Check every frame
                    {
                        yield return null;
                    }
                }
                SendLootDataChunk(COMPLETE_MARKER, connection);
                while (!LootDataAckReceived(connection)) // Check every frame for final ACK
                {
                    yield return null;
                }
            }

            private void SendLootDataChunk(string chunk, BoltConnection connection)
            {
                int packetSize = sizeof(int) + chunk.Length * 2;
                var packet = NewPacket(packetSize, connection);
                DebugManager.ConsoleLog("Sending chunk with size: " + packetSize + "    Packet Data:   " + chunk);
                packet.Packet.WriteString(chunk);
                Send(packet);
                ClearAckFlag(connection); // Clear the ACK flag before sending
            }

            public override void Read(UdpPacket packet, BoltConnection fromConnection)
            {
                string chunk = packet.ReadString();

                if (chunk == COMPLETE_MARKER)
                {
                    HandleReceivedData(receivedLootData, fromConnection);
                    receivedLootData = "";
                }
                else
                {
                    receivedLootData += chunk;
                    DebugManager.ConsoleLog("Received loot data chunk...");
                    NetworkManager.SendLootDataAck(); // Send ACK
                }
            }

            private void HandleReceivedData(string lootDataJson, BoltConnection target)
            {
                HashSet<LootData> receivedLootData = JsonConvert.DeserializeObject<HashSet<LootData>>(lootDataJson);
                DebugManager.ConsoleLog("Finished receiving Loot Data: " + receivedLootData.Count);
                LootRespawnManager.recievedLootIds = receivedLootData;
            }

            // ACK Handling
            private static Dictionary<ulong, bool> ackReceived = new Dictionary<ulong, bool>();

            private bool LootDataAckReceived(BoltConnection connection)
            {
                if (ackReceived.ContainsKey(MultiplayerUtilities.GetSteamId(connection)) && ackReceived[MultiplayerUtilities.GetSteamId(connection)])
                {
                    return true;
                }
                return false;
            }

            private void ClearAckFlag(BoltConnection connection)
            {
                if (!ackReceived.ContainsKey(MultiplayerUtilities.GetSteamId(connection)))
                {
                    ackReceived[MultiplayerUtilities.GetSteamId(connection)] = false;
                }
                else
                {
                    ackReceived[MultiplayerUtilities.GetSteamId(connection)] = false;
                }
            }

            public static void SetAckReceived(BoltConnection connection)
            {
                ackReceived[MultiplayerUtilities.GetSteamId(connection)] = true;
            }
        }

        internal class LootDataAck : Packets.NetEvent
        {
            public override string Id => "LootSync_LootDataAck";

            public void SendAck(GlobalTargets target = GlobalTargets.OnlyServer)
            {
                var packet = NewPacket(16, target);
                packet.Packet.WriteString("ACK");
                Send(packet);
                DebugManager.ConsoleLog("Sent Loot Data ACK");
            }

            public override void Read(UdpPacket packet, BoltConnection fromConnection)
            {
                DebugManager.ConsoleLog("Received Loot Data ACK");
                LootDataEvent.SetAckReceived(fromConnection);
            }
        }
    }
}
