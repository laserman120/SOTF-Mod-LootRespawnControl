using Endnight.Utilities;
using HarmonyLib;
using LootRespawnControl.Managers;
using RedLoader;
using Sons.Environment;
using static LootRespawnControl.LootManager;

namespace LootRespawnControl.Harmony
{
    internal class BreakableObject
    {
        //Patch the spawning of breakables
        [HarmonyPatch(typeof(Sons.Gameplay.BreakableObject), "Awake")]
        private static class BreakableObjectAwakePatch
        {
            private static void Postfix(Sons.Gameplay.BreakableObject __instance)
            {
                LootIdentifier identifierComponent = __instance.transform.gameObject.GetOrAddComponent<LootIdentifier>();
                string identifier = identifierComponent.Identifier;

                if (LootRespawnManager.IsLootCollected(identifier) && !ConfigManager.IsBreakableAllowed())
                {
                    if (__instance.transform.name.Contains("BreakableSticksInteraction"))
                    {
                        DebugManager.ConsoleLogWarning("Attempted to remove BreakableSticksInteraction! Returning out and removing hash...");
                        LootRespawnManager.RemoveLootFromCollected(identifier);
                        return;
                    }

                    if (ConfigManager.IsGlobalTimerEnabled() && LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
                    {
                        LootRespawnManager.RemoveLootFromCollected(identifier);
                        DebugManager.ConsoleLog($"Removed from collected due to time: {__instance.name}");
                        return;
                    }

                    //if breakable category is allowe timed, remove it
                    if (ConfigManager.IsBreakableAllowed() && LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
                    {
                        LootRespawnManager.RemoveLootFromCollected(identifier);
                    DebugManager.ConsoleLog($"Removed from collected due to time: {__instance.name}");
                        return;
                    }

                    DebugManager.ConsoleLog($"Destroying: {__instance.name}");

                    if (ConfigManager.IsGlobalTimerEnabled() || ConfigManager.allowBreakablesTimed)
                    {
                        TimedLootRespawnManager.CreateRespawnDataHolder(__instance.transform.gameObject, identifierComponent, LootRespawnControl._breakableId);
                    }

                    UnityEngine.Object.Destroy(__instance.transform.gameObject);
                }
            }
        }

        //runs whenever a breakable object is broken
        [HarmonyPatch(typeof(Sons.Gameplay.BreakableObject), "OnBreak")]
        private static class OnBreakPatch
        {
            private static void Postfix(Sons.Gameplay.BreakableObject __instance)
            {
                LootIdentifier identifierComponent = __instance.transform.GetComponent<LootIdentifier>();
                if (identifierComponent == null) { return; }

                if (__instance.name.Contains("Clone")) { return; }

                string identifier = identifierComponent.Identifier;

                //Pickup is a simple item spawner on broken
                Sons.Gameplay.PickUp PickUp = __instance._brokenPrefab?.transform.GetComponent<Sons.Gameplay.PickUp>() ?? null;
                int PickUpArrayLength = __instance._spawnDefinitions.Count;
                if (PickUp != null && !ConfigManager.IsBreakableAllowed())
                {
                    //return out if blacklisted item would be dropped
                    if (LootRespawnControl.ItemIdsBlacklistBreakable.Contains(PickUp._itemId)) { DebugManager.ConsoleLog($"Blocked due to blacklist"); return; }

                    LootRespawnManager.MarkLootAsCollected(identifier, __instance.gameObject.name, LootRespawnControl._breakableId, true);

                    if (ConfigManager.IsGlobalTimerEnabled() || ConfigManager.allowBreakablesTimed)
                    {
                        TimedLootRespawnManager.CreateRespawnDataHolder(__instance.transform.gameObject, identifierComponent, LootRespawnControl._breakableId);
                    }

                    DebugManager.ConsoleLogError($"Added: {__instance.gameObject.name}");
                    return;
                }
                if (PickUpArrayLength > 0 && !ConfigManager.IsBreakableAllowed())
                {
                    bool HasBlacklisted = false;
                    Il2CppSystem.Collections.Generic.List<Sons.Gameplay.BreakableObject.SpawnDefinition> SpawnDefinitions = __instance._spawnDefinitions;
                    //Iterate over the pick up array and check if any of the items are blacklisted
                    for (int i = 0; i < PickUpArrayLength; i++)
                    {
                        Sons.Gameplay.PickUp PickUpComponent = SpawnDefinitions[i]._prefab?.transform.GetComponent<Sons.Gameplay.PickUp>() ?? null;
                        if (PickUpComponent == null || LootRespawnControl.ItemIdsBlacklistBreakable.Contains(PickUpComponent._itemId))
                        {
                            //Special Case Propane
                            if(SpawnDefinitions[i]._prefab.name == "ExplosionPropane")
                            {
                                DebugManager.ConsoleLog($"Special Case Propane: {__instance.name}");
                                break;
                            }
                            //if any is blacklisted set true and break out of loop
                            HasBlacklisted = true;
                            DebugManager.ConsoleLog($"Blocked due to blacklist or empty pickup component in array: : {__instance.name}");
                            break;
                        }
                    }
                    if (!HasBlacklisted)
                    {
                        //if not blacklisted and only includes pickup components store the hash
                        LootRespawnManager.MarkLootAsCollected(identifier, __instance.gameObject.name, LootRespawnControl._breakableId, true);

                        if (ConfigManager.IsGlobalTimerEnabled() || ConfigManager.allowBreakablesTimed)
                        {
                            TimedLootRespawnManager.CreateRespawnDataHolder(__instance.transform.gameObject, identifierComponent, LootRespawnControl._breakableId);
                        }

                        DebugManager.ConsoleLog($"Added: {__instance.gameObject.name}");
                    }
                    return;
                }
                if (LootRespawnManager.IsLootCollected(identifier))
                {
                    LootRespawnManager.RemoveLootFromCollected(identifier);
                }
            }
        }
    }
}
