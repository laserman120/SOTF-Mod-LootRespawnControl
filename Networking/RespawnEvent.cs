using Bolt;
using RedLoader;
using SonsSdk.Networking;
using UdpKit;
using LootRespawnControl.Harmony;
using LootRespawnControl.Managers;
using Sons.Environment;
using static SonsSdk.ItemTools;
using FMODCustom;

namespace LootRespawnControl.Networking
{
    internal class RespawnEvent
    {
        /// <summary>
        /// Pickup event 
        /// </summary>
        internal class RespawnEventHandler : Packets.NetEvent
        {
            public override string Id => "LootSync_RespawnEvent";

            // Send the initial loot package
            public void Send(string pickupHash, GlobalTargets target = GlobalTargets.Everyone)
            {
                // Calculate packet size
                int packetSize = pickupHash.Length * 2;  // string length * 2 (UTF-16)

                var packet = NewPacket(packetSize, target);

                packet.Packet.WriteString(pickupHash);

                Send(packet);
            }

            private void HandleNetworkedPickup(string pickupHash)
            {
                // Implement your logic here for handling the received pickup data
                DebugManager.ConsoleLog($"Received removal command for identifier: {pickupHash}");
                LootManager.LootRespawnManager.RemoveLootFromCollected(pickupHash);
                TimedLootRespawnManager.CheckForRespawn();
            }

            public override void Read(UdpPacket packet, BoltConnection fromConnection)
            {

                string pickupHash = packet.ReadString();
 
                HandleNetworkedPickup(pickupHash);
            }
        }


        internal class RespawnEventRequest : Packets.NetEvent
        {
            public override string Id => "LootSync_RespawnEventRequest";

            //Send the initial loot package
            public void Send(string pickupName, string pickupHash, int id, bool isBreakable, GlobalTargets target = GlobalTargets.OnlyServer)
            {
                // Calculate packet size
                int packetSize = pickupName.Length * 2 + pickupHash.Length * 2 + 4 + 1; // string length * 2 (UTF-16) + string length * 2 (UTF-16) + int + bool

                var packet = NewPacket(packetSize, target);

                packet.Packet.WriteString(pickupName);
                packet.Packet.WriteString(pickupHash);
                packet.Packet.WriteInt(id);
                packet.Packet.WriteBool(isBreakable);

                Send(packet);
            }
           
            private void HandlePickupRequest(string pickupName, string identifier, int id, bool isBreakable, BoltConnection fromConnection)
            {
                // Implement your logic here for handling the received pickup data
                DebugManager.ConsoleLog($"Received respawn request for: {pickupName} {identifier}");
                bool result = false;
                if(LootManager.LootRespawnManager.IsLootCollected(identifier))
                {
                    if (isBreakable)
                    {
                        if (ConfigManager.IsGlobalTimerEnabled() && LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())) || ConfigManager.allowBreakablesTimed && LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        if (ConfigManager.IsGlobalTimerEnabled() && LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())) || ConfigManager.ShouldIdBeRemovedTimed(id) && LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
                        {
                            result = true;
                        }
                    }
                } 
                else 
                {
                    //If we can no longer find the hash in our collected we trust the client and send the respawn event
                    result = true;
                }
                if (result)
                {
                    LootManager.LootRespawnManager.RemoveLootFromCollected(identifier);
                    NetworkManager.SendRespawnEvent(identifier);
                }
            }
            public override void Read(UdpPacket packet, BoltConnection fromConnection)
            {
                string pickupName = packet.ReadString();
                string pickupHash = packet.ReadString();
                int pickupId = packet.ReadInt();
                bool isBreakable = packet.ReadBool();

                HandlePickupRequest(pickupName, pickupHash, pickupId, isBreakable, fromConnection);
            }
        }
    }
}
