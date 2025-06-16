using Endnight.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LootRespawnControl.Harmony
{
    internal class GreebleZonePatch
    {
        [HarmonyPatch(typeof(GreebleZone), nameof(GreebleZone.SpawnIndex))]
        private static class GreebleZoneAwakePatch
        {
            private static void Postfix(GreebleZone __instance, int index, bool forPreview = false)
            {
                GameObject spawnedGreeble = __instance.Instances[index];
                if (!spawnedGreeble)
                {
                    return;
                }

                Sons.Gameplay.PickUp pickUpComponent = spawnedGreeble.transform.GetComponentInChildren<Sons.Gameplay.PickUp>();

                if (pickUpComponent == null)
                {
                    return;
                }

                LootIdentifier lootIdentifier = pickUpComponent.gameObject.GetOrAddComponent<LootIdentifier>();

                lootIdentifier.enforceIdentifier = true;
                lootIdentifier.Identifier = LootManager.LootRespawnManager.GenerateLootID(__instance.transform.position, __instance.transform.rotation, index.ToString());
                lootIdentifier.GreebleZone = __instance.gameObject;

                // After fixing the identifier, check if it should be deleted
                PickUp.CheckIfPickupShouldBeDeleted(pickUpComponent);
            }
        }
    }
}
