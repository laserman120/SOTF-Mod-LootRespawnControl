using Alt.Json;
using LootRespawnControl.Managers;
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
using static SonsSdk.ItemTools;

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
                byte[] positionBytes = Vector3ToBytes(position);
                byte[] rotationBytes = QuaternionToBytes(rotation);

                string safeName = "";
                if (!string.IsNullOrEmpty(name))
                {
                    safeName = name.Length >= 3 ? name.Substring(0, 3) : name; // Take first 3 characters, or whole name if shorter
                }

                byte[] nameBytes = Encoding.UTF8.GetBytes(safeName);

                using (MD5 md5Hash = MD5.Create())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(positionBytes, 0, positionBytes.Length);
                        ms.Write(rotationBytes, 0, rotationBytes.Length);
                        ms.Write(nameBytes, 0, nameBytes.Length);

                        byte[] bytes = md5Hash.ComputeHash(ms.ToArray());
                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            builder.Append(bytes[i].ToString("x2"));
                        }
                        return builder.ToString();
                    }
                }
            }


            public static byte[] Vector3ToBytes(Vector3 vector)
            {
                byte[] bytes = new byte[12]; // 3 floats * 4 bytes
                Buffer.BlockCopy(BitConverter.GetBytes(vector.x), 0, bytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(vector.y), 0, bytes, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(vector.z), 0, bytes, 8, 4);
                return bytes;
            }

            public static byte[] QuaternionToBytes(Quaternion quaternion)
            {
                byte[] bytes = new byte[16]; // 4 floats * 4 bytes
                Buffer.BlockCopy(BitConverter.GetBytes(quaternion.x), 0, bytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(quaternion.y), 0, bytes, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(quaternion.z), 0, bytes, 8, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(quaternion.w), 0, bytes, 12, 4);
                return bytes;
            }

            public static string SaveCollectedLoot()
            {
                performCleanupBeforeSave();
                var serializableSet = new SerializableHashSet<LootData>(LootRespawnManager.collectedLootIds);
                return JsonConvert.SerializeObject(serializableSet);
            }

            public static void performCleanupBeforeSave()
            {
                foreach (LootData lootData in LootRespawnManager.collectedLootIds)
                {
                    if (ConfigManager.ShouldIdBeRemovedTimed(lootData.Id) || ConfigManager.useTimerGlobal)
                    {
                        if (LootRespawnControl.HasEnoughTimePassed(lootData.Hash, LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
                        {
                            DebugManager.ConsoleLog($"Removing loot item with ID {lootData.Id} and Hash {lootData.Hash} due to time passed.");
                            LootRespawnManager.collectedLootIds.Remove(lootData);
                        }
                    }
                }
            }

            public static void LoadCollectedLoot(string jsonData)
            {
                if (jsonData == null)
                {
                    LootRespawnManager.collectedLootIds = new HashSet<LootData>();
                    return;
                }

                HandleStartupLootData(jsonData);
            }

            public static void HandleStartupLootData(string jsonData)
            {
                //Check if the user is in singleplayer or if the user is in multiplayer but the host has not sent any config data
                if (BoltNetwork.isRunning && BoltNetwork.isClient && !LootRespawnControl.recievedConfigData)
                {
                    DebugManager.ConsoleLog($"Client is in multiplayer and has not recieved any config data... setting local config values and forcing networking off"); 
                    ConfigManager.SetLocalConfigValues();
                    ConfigManager.SetMultiplayerConfigValue(false);
                    return;
                }
                else if (BoltNetwork.isRunning && BoltNetwork.isServer)
                {
                    DebugManager.ConsoleLog($"User is the host... setting local config values");
                    ConfigManager.SetLocalConfigValues();
                } else if(!BoltNetwork.isRunning)
                {
                    DebugManager.ConsoleLog($"User is in singleplayer... setting local config values");
                    ConfigManager.SetLocalConfigValues();
                    ConfigManager.SetMultiplayerConfigValue(false);
                }

                //now after the configuration is set we handle the saved data
                if (jsonData != null)
                {
                    //Deserialize the data
                    var serializableSet = JsonConvert.DeserializeObject<SerializableHashSet<LootData>>(jsonData);

                    //Cleanup the data
                    if (BoltNetwork.isServerOrNotRunning)
                    {
                        LootRespawnManager.collectedLootIds = CleanupCollected(serializableSet.ToHashSet());
                    } else
                    {
                        LootRespawnManager.collectedLootIds = serializableSet.ToHashSet();
                    }
                }

                //if we have recieved data merge it now
                if (recievedLootIds.Count > 0)
                {
                    DebugManager.ConsoleLog($"Syncing local collected data with networked collected data");
                    //We do not clean here, the host already cleaned the data for us, trust the host
                    LootRespawnManager.collectedLootIds = MergeCollectedLoot(LootRespawnManager.collectedLootIds, recievedLootIds);

                    //Reset recieved data
                    recievedLootIds = new HashSet<LootData>();
                }

                //Initialize the loot respawn manager
                TimedLootRespawnManager.IntitializeManager();

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

                if (isBreakable && ConfigManager.enableMultiplayer && ConfigManager.IsBreakablesNetworked())
                {
                    DebugManager.ConsoleLog($"Sending Breakable Pickup Event...: {objectName}");
                    NetworkManager.SendPickupEvent(objectName, identifier, LootRespawnControl._breakableId, timestamp);
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
                        DebugManager.ConsoleLog($"Removing loot item with ID {lootData.Id} and Hash {lootData.Hash} due to configuration change.");
                    }
                }

                return preservedLoot;
            }

            public static HashSet<LootData> GetNetworkedCollected(HashSet<LootData> collectedLootIds)
            {
                HashSet<LootData> networkedLoot = new HashSet<LootData>();
                if (collectedLootIds == null)
                {
                    DebugManager.ConsoleLog($"Collected loot is null!");
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
