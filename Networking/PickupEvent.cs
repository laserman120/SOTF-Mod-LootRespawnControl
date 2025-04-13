using Bolt;
using RedLoader;
using SonsSdk.Networking;
using UdpKit;
using LootRespawnControl.Harmony;
using LootRespawnControl.Managers;

namespace LootRespawnControl.Networking
{
    internal class PickupEvent
    {
        /// <summary>
        /// Pickup event 
        /// </summary>
        internal class PickupEventHandler : Packets.NetEvent
        {
            public override string Id => "LootSync_PickupEvent";

            //Send the initial loot package
            public void Send(string pickupName, string pickupHash, int pickupId, long time, GlobalTargets target = GlobalTargets.Everyone)
            {
                // Calculate packet size
                int packetSize = pickupName.Length * 2 + pickupHash.Length * 2+ 4 + 8; // string length * 2 (UTF-16) + long time

                var packet = NewPacket(packetSize, target);

                packet.Packet.WriteString(pickupName);
                packet.Packet.WriteString(pickupHash);
                packet.Packet.WriteInt(pickupId);
                packet.Packet.WriteLong(time);

                Send(packet);
            }

            private void HandleNetworkedPickup(string pickupName, string pickupHash, int pickupId, long time)
            {
                // Implement your logic here for handling the received pickup data
                DebugManager.ConsoleLog($"Received Pickup: {pickupName}, Id: {pickupId}, Hash: {pickupHash}, Time: {time}");

                //If the player has the pickup hash in the awaiting reply, as well as the bool set to true we do not need to handle the pickup data.
                if (PickUp.HashExists(pickupHash) && PickUp.GetHashBool(pickupHash) == true) 
                {
                    return;
                }
                LootRespawnControl.HandlePickupDataRecieved(pickupName, pickupHash, pickupId, time);
            }

            public override void Read(UdpPacket packet, BoltConnection fromConnection)
            {
                string pickupName = packet.ReadString();
                string pickupHash = packet.ReadString();
                int pickupId = packet.ReadInt();
                long time = packet.ReadLong();

                HandleNetworkedPickup(pickupName, pickupHash, pickupId, time);
            }
        }


        internal class PickupEventRequest : Packets.NetEvent
        {
            public override string Id => "LootSync_PickupEventRequest";

            //Send the initial loot package
            public void Send(string pickupName, string pickupHash, int pickupId, long time, GlobalTargets target = GlobalTargets.OnlyServer)
            {
                // Calculate packet size
                int packetSize = pickupName.Length * 2 + pickupHash.Length * 2 + 4 + 8; // string length * 2 (UTF-16) + long time

                var packet = NewPacket(packetSize, target);

                packet.Packet.WriteString(pickupName);
                packet.Packet.WriteString(pickupHash);
                packet.Packet.WriteInt(pickupId);
                packet.Packet.WriteLong(time);

                Send(packet);
            }
           
            private void HandlePickupRequest(string pickupName, string pickupHash, int pickupId, long time, BoltConnection fromConnection)
            {
                // Implement your logic here for handling the received pickup data
                DebugManager.ConsoleLog($"Received Pickup request for: {pickupName} {pickupHash}");

                if (!LootManager.LootRespawnManager.IsLootCollected(pickupHash))
                {
                    DebugManager.ConsoleLog($"Accepting Pickup request for: {pickupName} {pickupHash}");
                    LootRespawnControl.HandlePickupDataRecieved(pickupName, pickupHash, pickupId, time);
                    //Return true to allow package to go through
                    NetworkManager.SendPickupAck(pickupHash, true, fromConnection);
                    //Send the data to everyone else as well
                    NetworkManager.SendPickupEvent(pickupName, pickupHash, pickupId, time);
                } else
                {
                    DebugManager.ConsoleLog($"Denying Pickup request for: {pickupName} {pickupHash}");
                    //Return false to deny the request
                    NetworkManager.SendPickupAck(pickupHash, false, fromConnection);
                }
            }

            public override void Read(UdpPacket packet, BoltConnection fromConnection)
            {
                string pickupName = packet.ReadString();
                string pickupHash = packet.ReadString();
                int pickupId = packet.ReadInt();
                long time = packet.ReadLong();

                HandlePickupRequest(pickupName, pickupHash, pickupId, time, fromConnection);
            }
        }

        internal class PickupEventAck : Packets.NetEvent
        {
            public override string Id => "LootSync_PickupEventAck";

            public void Send(string pickupHash, bool confirm, BoltConnection target)
            {
                // Calculate packet size
                int packetSize = pickupHash.Length * 2 + 1; // string length * 2 (UTF-16) + bool

                var packet = NewPacket(packetSize, target);

                packet.Packet.WriteString(pickupHash);
                packet.Packet.WriteBool(confirm);

                Send(packet);
            }

            private void HandleAckRecieved(string pickupHash, bool confirm)
            {
                DebugManager.ConsoleLog($"Recieved ack for: {pickupHash} {confirm}");
                if (PickUp.HashExists(pickupHash))
                {
                    PickUp.SetHashBool(pickupHash, confirm);
                }
            }

            public override void Read(UdpPacket packet, BoltConnection fromConnection)
            {
                string pickupHash = packet.ReadString();
                bool confirm = packet.ReadBool();

                HandleAckRecieved(pickupHash, confirm);
            }
        }
    }
}
