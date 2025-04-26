using Endnight.Utilities;
using HarmonyLib;
using LootRespawnControl.Managers;
using RedLoader;
using Sons.Environment;
using UnityEngine;
using static LootRespawnControl.LootManager;

namespace LootRespawnControl.Harmony
{


    internal class PickUp
    {
        public static List<Sons.Gameplay.PickUp> PickupsPendingCheck = new List<Sons.Gameplay.PickUp>();

        //Patch the spawning of pickups
        [HarmonyPatch(typeof(Sons.Gameplay.PickUp), "Awake")]
        private static class PickUpAwakePatch
        {
            private static void Postfix(Sons.Gameplay.PickUp __instance)
            {
                CheckIfPickupShouldBeDeleted(__instance);

                if (!LootRespawnControl.DoubleCheckedCollectedLoot)
                {
                    if(!PickupsPendingCheck.Contains(__instance))
                    {
                        PickupsPendingCheck.Add(__instance);
                    }
                }

            }
        }

        [HarmonyPatch(typeof(Sons.Gameplay.PickUp), "OnEnable")]
        private static class PickUpEnablePatch
        {
            private static void Postfix(Sons.Gameplay.PickUp __instance)
            {
                CheckIfPickupShouldBeDeleted(__instance);
            }
        }

        public static void CheckIfPickupShouldBeDeleted(Sons.Gameplay.PickUp __instance)
        {
            LootIdentifier identifierComponent = __instance.transform.gameObject.GetOrAddComponent<LootIdentifier>();
            string identifier = identifierComponent.Identifier;

            if (ConfigManager.IsItemIdBlocked(__instance._itemId))
            {
                if (__instance.name.Contains("Clone")) { return; }

                Transform PickupGui = __instance.transform.Find("_PickupGui_");
                if (PickupGui == null)
                {
                    DebugManager.ConsoleLog($"Prevented destruction of: {__instance.name} due to missing PickupGui"); return;
                }

                if (__instance.name.Contains("SkinningPickupHelper"))
                {
                    DebugManager.ConsoleLogWarning($"Prevented destruction of: {__instance.name} with id {__instance._itemId} due to being a skinning pickup helper"); return;
                }

                DebugManager.ConsoleLog($"Destroying due to blocked config: {__instance.name}");
                UnityEngine.Object.Destroy(__instance._destroyTarget);
            }

            if (LootRespawnManager.IsLootCollected(identifier) && ConfigManager.ShouldIdBeRemoved(__instance._itemId))
            {
                if ((ConfigManager.IsGlobalTimerEnabled() && LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString()))) || ConfigManager.ShouldIdBeRemovedTimed(__instance._itemId) && LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
                {
                    LootRespawnManager.RemoveLootFromCollected(identifier);
                    DebugManager.ConsoleLog($"Removed from collected due to time: {__instance.name}");
                    return;
                }
                // Create holder for respawning...
                if(ConfigManager.IsGlobalTimerEnabled() || ConfigManager.ShouldIdBeRemovedTimed(__instance._itemId))
                {
                    TimedLootRespawnManager.CreateRespawnDataHolder(__instance._destroyTarget, identifierComponent, __instance._itemId);
                    UnityEngine.Object.Destroy(__instance._destroyTarget);
                    DebugManager.ConsoleLog($"Destroyed Pickup and created respawn holder: {__instance.name}");
                    return;
                }
                // Destroy the pickup
                DebugManager.ConsoleLog($"Destroying: {__instance.name}");
                UnityEngine.Object.Destroy(__instance._destroyTarget);
            }
        }

        // Runs whenever a pickup is collected
        [HarmonyPatch(typeof(Sons.Gameplay.PickUp), "Collect")]
        private static class CollectPatch
        {
            private static bool Prefix(Sons.Gameplay.PickUp __instance)
            {
                LootIdentifier identifierComponent = __instance.transform.GetComponent<LootIdentifier>();
                if (identifierComponent == null) { DebugManager.ConsoleLog($"Prevented collection of: {__instance.name} due to missing IdentifierComponent"); return true; }

                // Hotfix for interaction components which also feature a pickup component of any type
                Transform PickupGui = __instance.transform.Find("_PickupGui_");
                if (PickupGui == null && !IsValidRadio(__instance))
                {
                    DebugManager.ConsoleLog($"Prevented collection of: {__instance.name} due to missing PickupGui "); return true;
                }

                if (__instance.name.Contains("Clone")) { return true; }

                string identifier = identifierComponent.Identifier;

                if (ConfigManager.ShouldIdBeRemoved(__instance._itemId) || ConfigManager.ShouldIdBeNetworked(__instance._itemId))
                {
                    // Networking check
                    if (BoltNetwork.isClient && ConfigManager.ShouldIdBeNetworked(__instance._itemId))
                    {
                        if (!HashExists(identifier))
                        {
                            // Send the request to the server,
                            // Coroutine checks for a reply
                            // Coroutine handles the reply and runs the collect method again
                            DebugManager.ConsoleLog($"Set hash to awaiting reply, sending request: {__instance.name}");
                            __instance.gameObject.SetActive(false);
                            AddHash(identifier);
                            NetworkManager.SendPickupRequest(__instance.name, identifier, __instance._itemId, LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString()));
                            AwaitPickupConfirmation(__instance, identifier).RunCoro();
                            return false;
                        } else
                        {
                            if (GetHashBool(identifier) == false)
                            {
                                return false;
                            } else
                            {
                                RemoveHash(identifier);
                                DebugManager.ConsoleLog($"Hash is true, removing it from the list and continuing execution: {__instance.name}");
                            }
                        }
                    } else if (BoltNetwork.isRunning && ConfigManager.ShouldIdBeNetworked(__instance._itemId))
                    {
                        // If host send out the pickup event directly
                        NetworkManager.SendPickupEvent(__instance.name, identifier, __instance._itemId, LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString()));
                    }
                    

                    LootRespawnManager.MarkLootAsCollected(identifier, __instance.name, __instance._itemId);
                    // Create holder for respawning...
                    if (ConfigManager.IsGlobalTimerEnabled() || ConfigManager.ShouldIdBeRemovedTimed(__instance._itemId))
                    {
                        TimedLootRespawnManager.CreateRespawnDataHolder(__instance._destroyTarget, identifierComponent, __instance._itemId);
                    }

                    DebugManager.ConsoleLog($"Added: {__instance.name}");
                    return true;
                }

                if (LootRespawnManager.IsLootCollected(identifier))
                {
                    LootRespawnManager.RemoveLootFromCollected(identifier);
                    DebugManager.ConsoleLog($"Removed Loot From collected: {__instance.name}"); return true;
                }
                return true;
            }
        }

        private static System.Collections.IEnumerator AwaitPickupConfirmation(Sons.Gameplay.PickUp __instance, string identifier)
        {
            float startTime = Time.time;
            float duration = 2f;

            while (Time.time - startTime < duration)
            {
                if (GetHashBool(identifier) == true)
                {
                    DebugManager.ConsoleLog($"Ack recieved, running collect: {__instance.name}");
                    __instance.Collect();
                    yield break;
                }

                yield return null; // Wait for the next frame
            }

            DebugManager.ConsoleLog($"Coro stopped with timeout: {identifier}");

            if (LootRespawnManager.IsLootCollected(identifier))
            {
                //has already been marked as collected, destroy it
                GameObject.Destroy(__instance.gameObject);
            }
            else
            {
                if(GetHashBool(identifier) == false)
                {
                    DebugManager.ConsoleLogWarning($"DESYNC DETECTED! Pickup already collected on the server side, but not on the client. Destroying Pickup and marking as collected! {identifier}");
                    LootRespawnManager.MarkLootAsCollected(identifier, __instance.name, __instance._itemId);
                    GameObject.Destroy(__instance.gameObject);
                    yield break;
                }

                DebugManager.ConsoleLogWarning($"Host failed to respond to pickup request! {identifier}");
                __instance.gameObject.SetActive(true);
            }
        }

        // Method to check if a hash exists in the list
        public static bool HashExists(string hash)
        {
            foreach (KeyValuePair<string, bool?> entry in LootRespawnControl.pickupsAwaitingReply)
            {
                if (entry.Key == hash)
                {
                    return true;
                }
            }
            return false;
        }

        // Method to add a new hash entry to the list
        public static void AddHash(string hash)
        {
            if (!HashExists(hash))
            {
                LootRespawnControl.pickupsAwaitingReply.Add(new KeyValuePair<string, bool?>(hash, null)); // Default to false
            }

        }

        // Method to remove a hash entry from the list
        public static void RemoveHash(string hash)
        {
            for (int i = 0; i < LootRespawnControl.pickupsAwaitingReply.Count; i++)
            {
                if (LootRespawnControl.pickupsAwaitingReply[i].Key == hash)
                {
                    LootRespawnControl.pickupsAwaitingReply.RemoveAt(i);
                    return;
                }
            }
        }

        // Method to change the boolean value of a hash
        public static void SetHashBool(string hash, bool newValue)
        {
            for (int i = 0; i < LootRespawnControl.pickupsAwaitingReply.Count; i++)
            {
                if (LootRespawnControl.pickupsAwaitingReply[i].Key == hash)
                {
                    LootRespawnControl.pickupsAwaitingReply[i] = new KeyValuePair<string, bool?>(hash, newValue);
                    return;
                }
            }
        }

        public static bool? GetHashBool(string hash)
        {
            foreach (KeyValuePair<string, bool?> entry in LootRespawnControl.pickupsAwaitingReply)
            {
                if (entry.Key == hash)
                {
                    return entry.Value;
                }
            }
            return false;
        }

        public static bool IsValidRadio(Sons.Gameplay.PickUp __instance)
        {
            if(__instance.name.StartsWith("Radio") && !__instance.name.Contains("FromStructure")){
                return true;
            }
            return false;
        }
    }
}
