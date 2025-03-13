using Alt.Json;
using RedLoader;
using Sons.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static LootRespawnControl.LootManager.LootRespawnManager;

namespace LootRespawnControl
{
    internal class LootManager
    {
        public class LootRespawnManager
        {
            public static HashSet<LootData> collectedLootIds = new HashSet<LootData>();

            public static HashSet<LootData> recievedLootIds = new HashSet<LootData>();

            public static string GenerateLootID(Vector3 position, Quaternion rotation, string name)
            {
                string combinedString = $"{position.x:F6}-{position.y:F6}-{position.z:F6}-{rotation.x:F6}-{rotation.y:F6}-{rotation.z:F6}-{rotation.w:F6}-{name.Substring(0, 3)}";
                using (MD5 md5Hash = MD5.Create())
                {
                    byte[] bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
                    // Convert the byte array to hexadecimal string
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }

                    return builder.ToString();
                }

                
            }

            public static string SaveCollectedLoot()
            {
                var serializableSet = new SerializableHashSet<LootData>(LootRespawnManager.collectedLootIds);
                return JsonConvert.SerializeObject(serializableSet);
            }

            public static void LoadCollectedLoot(string jsonData)
            {
                if (jsonData == null)
                {
                    LootRespawnManager.collectedLootIds = new HashSet<LootData>();
                    return;
                }
                var serializableSet = JsonConvert.DeserializeObject<SerializableHashSet<LootData>>(jsonData);

                LootRespawnManager.collectedLootIds = CleanupCollected(serializableSet.ToHashSet());

                //if we have recieved data merge it now
                if(recievedLootIds.Count > 0)
                {
                    if (Config.ConsoleLogging.Value) { RLog.Msg($"Syncing local collected data with networked collected data"); }
                    LootRespawnManager.collectedLootIds = CleanupCollected(MergeCollectedLoot(LootRespawnManager.collectedLootIds, recievedLootIds));

                    //Reset recieved data
                    recievedLootIds = new HashSet<LootData>();
                }

                //Double check any loaded objects that were created before loading has occured
                Harmony.PickUp.PickupsPendingCheck.ForEach(PickUp =>
                {
                    if (PickUp != null)
                    {
                        Harmony.PickUp.CheckIfPickupShouldBeDeleted(PickUp);
                    }
                });

                //Set this value to confirm that the double check has occured
                LootRespawnControl.DoubleCheckedCollectedLoot = true;
                Harmony.PickUp.PickupsPendingCheck = new List<Sons.Gameplay.PickUp>();

            }


            public static bool IsLootCollected(string identifier)
            {
                return collectedLootIds.Any(lootData => lootData.Hash == identifier);
            }

            public static void MarkLootAsCollected(string identifier, string objectName = null, int itemId = 0, bool isBreakable = false, long timestamp = 0)
            {
                if (timestamp == 0) { timestamp = LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString()); }
                collectedLootIds.Add(new LootData(identifier, timestamp, itemId));

                //DO not continue if not the host or singleplayer.
                if (BoltNetwork.isClient) { return; }
                if (isBreakable && ConfigManager.enableMultiplayer && ConfigManager.IsBreakablesNetworked())
                {
                    if (Config.ConsoleLogging.Value) { RLog.Msg($"Sending Breakable Pickup Event...: {objectName}"); }
                    NetworkManager.SendPickupEvent(objectName, identifier, 0, timestamp);
                }
            }

            public static HashSet<LootData> CleanupCollected(HashSet<LootData> collectedLootIds)
            {
                HashSet<LootData> preservedLoot = new HashSet<LootData>();

                if (collectedLootIds == null)
                {
                    return preservedLoot;
                }

                foreach (LootData lootData in collectedLootIds)
                {
                    if (ConfigManager.ShouldIdBeRemoved(lootData.Id))
                    {
                        preservedLoot.Add(lootData);
                    } else
                    {
                        if (Config.ConsoleLogging.Value) { RLog.Msg($"Removing loot item with ID {lootData.Id} and Hash {lootData.Hash} due to configuration change."); }
                    }
                }

                return preservedLoot;
            }

            public static HashSet<LootData> GetNetworkedCollected(HashSet<LootData> collectedLootIds)
            {
                HashSet<LootData> networkedLoot = new HashSet<LootData>();
                if (collectedLootIds == null)
                {
                    if (Config.ConsoleLogging.Value) { RLog.Msg($"Collected loot is null!"); }
                    return networkedLoot;
                }

                foreach (LootData lootData in collectedLootIds)
                {
                    if (ConfigManager.ShouldIdBeNetworked(lootData.Id))
                    {
                        networkedLoot.Add(lootData);
                    }
                }

                return networkedLoot;
            }

            public static HashSet<LootData> MergeCollectedLoot(HashSet<LootData> clientLoot, HashSet<LootData> serverLoot)
            {
                if (clientLoot == null)
                {
                    return serverLoot ?? new HashSet<LootData>();
                }
                if (serverLoot == null)
                {
                    return clientLoot ?? new HashSet<LootData>();
                }
                HashSet<LootData> mergedLoot = new HashSet<LootData>(clientLoot);

                foreach (LootData serverItem in serverLoot)
                {
                    if (!mergedLoot.Any(clientItem => clientItem.Hash == serverItem.Hash))
                    {
                        mergedLoot.Add(serverItem);
                    }
                }

                return mergedLoot;
            }

            public static void RemoveLootFromCollected(string identifier)
            {
                collectedLootIds.RemoveWhere(lootData => lootData.Hash == identifier);
            }

            public static long? GetLootTimestamp(string identifier)
            {
                LootData lootData = collectedLootIds.FirstOrDefault(ld => ld.Hash == identifier);
                return lootData != null ? lootData.Timestamp : null;
            }

            public class LootData
            {
                public string Hash { get; set; }
                public long Timestamp { get; set; }
                public int Id { get; set; }

                public LootData(string hash, long timestamp, int id)
                {
                    Hash = hash;
                    Timestamp = timestamp;
                    Id = id;
                }
            }

            [Serializable]
            public class SerializableHashSet<T>
            {
                public List<T> list;
                public SerializableHashSet() { }
                public SerializableHashSet(HashSet<T> hashSet)
                {
                    list = new List<T>(hashSet);
                }
                public HashSet<T> ToHashSet()
                {
                    return new HashSet<T>(list);
                }
            }
        }
    }
}
