using RedLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LootRespawnControl
{
    internal class ConfigManager
    {
        internal static bool useTimerGlobal;
        internal static int timeInDays;
        internal static bool allowMelee;
        internal static bool allowMeleeTimed;
        internal static bool removeMelee;
        internal static bool enableNetworkingMelee; 
        internal static bool allowRanged;
        internal static bool allowRangedTimed;
        internal static bool removeRanged;
        internal static bool enableNetworkingRanged;
        internal static bool allowWeaponMods;
        internal static bool allowWeaponModsTimed;
        internal static bool removeWeaponMods;
        internal static bool enableNetworkingWeaponMods;
        internal static bool allowMaterials;
        internal static bool allowMaterialsTimed;
        internal static bool removeMaterials;
        internal static bool enableNetworkingMaterials; 
        internal static bool allowFood;
        internal static bool allowFoodTimed;
        internal static bool removeFood;
        internal static bool enableNetworkingFood; 
        internal static bool allowMeds;
        internal static bool allowMedsTimed;
        internal static bool removeMeds;
        internal static bool enableNetworkingMeds;
        internal static bool allowPlants;
        internal static bool allowPlantsTimed;
        internal static bool removePlants;
        internal static bool enableNetworkingPlants;
        internal static bool allowAmmunition;
        internal static bool allowAmmunitionTimed;
        internal static bool removeAmmunition;
        internal static bool enableNetworkingAmmunition; 
        internal static bool allowThrowables;
        internal static bool allowThrowablesTimed;
        internal static bool removeThrowables;
        internal static bool enableNetworkingThrowables;
        internal static bool allowExpendables;
        internal static bool allowExpendablesTimed;
        internal static bool removeExpendables;
        internal static bool enableNetworkingExpendables; 
        internal static bool allowBreakables;
        internal static bool allowBreakablesTimed;
        internal static bool removeBreakables;
        internal static string allowList;
        internal static string allowListTimed;
        internal static string removeList;
        internal static string networkList;

        public static bool ShouldIdBeRemoved(int ItemId)
        {
            bool result = false;
            if (LootRespawnControl.ItemIdsMeleeWeapons.Contains(ItemId) && !allowMelee) { result = true; }
            if (LootRespawnControl.ItemIdsRangedWeapons.Contains(ItemId) && !allowRanged) { result = true; }
            if (LootRespawnControl.ItemIdsMaterials.Contains(ItemId) && !allowMaterials) { result = true; }
            if (LootRespawnControl.ItemIdsAmmunition.Contains(ItemId) && !allowAmmunition) { result = true; }
            if (LootRespawnControl.ItemIdsExpendables.Contains(ItemId) && !allowExpendables) { result = true; }
            if (LootRespawnControl.ItemIdsFood.Contains(ItemId) && !allowFood) { result = true; }
            if (LootRespawnControl.ItemIdsThrowables.Contains(ItemId) && !allowThrowables) { result = true; }
            if (LootRespawnControl.ItemIdsMedicineAndEnergy.Contains(ItemId) && !allowMeds) { result = true; }
            if (LootRespawnControl.ItemIdsPlants.Contains(ItemId) && !allowPlants) { result = true; }

            //Force no removal if whitelisted
            if (LootRespawnControl.CustomWhitelist.Contains(ItemId)) { result = false; }

            return result;
        }

        public static bool ShouldIdBeRemovedTimed(int ItemId)
        {
            bool result = false;
            if (LootRespawnControl.ItemIdsMeleeWeapons.Contains(ItemId) && allowMeleeTimed) { result = true; }
            if (LootRespawnControl.ItemIdsRangedWeapons.Contains(ItemId) && allowRangedTimed) { result = true; }
            if (LootRespawnControl.ItemIdsMaterials.Contains(ItemId) && allowMaterialsTimed) { result = true; }
            if (LootRespawnControl.ItemIdsAmmunition.Contains(ItemId) && allowAmmunitionTimed) { result = true; }
            if (LootRespawnControl.ItemIdsExpendables.Contains(ItemId) && allowExpendablesTimed) { result = true; }
            if (LootRespawnControl.ItemIdsFood.Contains(ItemId) && allowFoodTimed) { result = true; }
            if (LootRespawnControl.ItemIdsThrowables.Contains(ItemId) && allowThrowablesTimed) { result = true; }
            if (LootRespawnControl.ItemIdsMedicineAndEnergy.Contains(ItemId) && allowMedsTimed) { result = true; }
            if (LootRespawnControl.ItemIdsPlants.Contains(ItemId) && allowPlantsTimed) { result = true; }

            //Allow timed spawn of specific item if timed whitelist includes it
            if (LootRespawnControl.CustomWhitelistTimed.Contains(ItemId)) { result = true; }

            return result;
        }

        public static bool IsItemIdBlocked(int ItemId)
        {
            bool result = false;
            if (LootRespawnControl.ItemIdsMeleeWeapons.Contains(ItemId) && removeMelee) { result = true; }
            if (LootRespawnControl.ItemIdsRangedWeapons.Contains(ItemId) && removeRanged) { result = true; }
            if (LootRespawnControl.ItemIdsMaterials.Contains(ItemId) && removeMaterials) { result = true; }
            if (LootRespawnControl.ItemIdsAmmunition.Contains(ItemId) && removeAmmunition) { result = true; }
            if (LootRespawnControl.ItemIdsExpendables.Contains(ItemId) && removeExpendables) { result = true; }
            if (LootRespawnControl.ItemIdsFood.Contains(ItemId) && removeFood) { result = true; }
            if (LootRespawnControl.ItemIdsThrowables.Contains(ItemId) && removeThrowables) { result = true; }
            if (LootRespawnControl.ItemIdsMedicineAndEnergy.Contains(ItemId) && removeMeds) { result = true; }
            if (LootRespawnControl.ItemIdsPlants.Contains(ItemId) && removePlants) { result = true; }

            //Allow timed spawn of specific item if timed whitelist includes it
            if (LootRespawnControl.CustomBlacklist.Contains(ItemId)) { result = true; }

            return result;
        }

        public static bool ShouldIdBeNetworked(int ItemId)
        {
            bool result = false;
            if (LootRespawnControl.ItemIdsMeleeWeapons.Contains(ItemId) && enableNetworkingMelee) { result = true; }
            if (LootRespawnControl.ItemIdsRangedWeapons.Contains(ItemId) && enableNetworkingRanged) { result = true; }
            if (LootRespawnControl.ItemIdsMaterials.Contains(ItemId) && enableNetworkingMaterials) { result = true; }
            if (LootRespawnControl.ItemIdsAmmunition.Contains(ItemId) && enableNetworkingAmmunition) { result = true; }
            if (LootRespawnControl.ItemIdsExpendables.Contains(ItemId) && enableNetworkingExpendables) { result = true; }
            if (LootRespawnControl.ItemIdsFood.Contains(ItemId) && enableNetworkingFood) { result = true; }
            if (LootRespawnControl.ItemIdsThrowables.Contains(ItemId) && enableNetworkingThrowables) { result = true; }
            if (LootRespawnControl.ItemIdsMedicineAndEnergy.Contains(ItemId) && enableNetworkingMeds) { result = true; }
            if (LootRespawnControl.ItemIdsPlants.Contains(ItemId) && enableNetworkingPlants) { result = true; }

            //Force no removal if whitelisted
            if (LootRespawnControl.CustomNetworkingList.Contains(ItemId)) { result = true; }

            return result;
        }

        public static bool IsBreakableAllowed()
        {
            return allowBreakables;
        }

        public static bool IsGlobalTimerEnabled()
        {
            return useTimerGlobal;
        }

        public static void SetLocalConfigValues()
        {
            useTimerGlobal = Config.useTimerGlobal.Value;
            timeInDays = Config.timeInDays.Value;
            allowMelee = Config.allowMelee.Value;
            allowMeleeTimed = Config.allowMeleeTimed.Value;
            removeMelee = Config.removeMelee.Value;
            enableNetworkingMelee = Config.enableNetworkingMelee.Value;
            allowRanged = Config.allowRanged.Value;
            allowRangedTimed = Config.allowRangedTimed.Value;
            removeRanged = Config.removeRanged.Value;
            enableNetworkingRanged = Config.enableNetworkingRanged.Value; 
            allowWeaponMods = Config.allowWeaponMods.Value;
            allowWeaponModsTimed = Config.allowWeaponModsTimed.Value;
            removeWeaponMods = Config.removeWeaponMods.Value;
            enableNetworkingWeaponMods = Config.enableNetworkingWeaponMods.Value;
            allowMaterials = Config.allowMaterials.Value;
            allowMaterialsTimed = Config.allowMaterialsTimed.Value;
            removeMaterials = Config.removeMaterials.Value;
            enableNetworkingMaterials = Config.enableNetworkingMaterials.Value; 
            allowFood = Config.allowFood.Value;
            allowFoodTimed = Config.allowFoodTimed.Value;
            removeFood = Config.removeFood.Value;
            enableNetworkingFood = Config.enableNetworkingFood.Value;
            allowMeds = Config.allowMeds.Value;
            allowMedsTimed = Config.allowMedsTimed.Value;
            removeMeds = Config.removeMeds.Value;
            enableNetworkingMeds = Config.enableNetworkingMeds.Value;
            allowPlants = Config.allowPlants.Value;
            allowPlantsTimed = Config.allowPlantsTimed.Value;
            removePlants = Config.removePlants.Value;
            enableNetworkingPlants = Config.enableNetworkingPlants.Value;
            allowAmmunition = Config.allowAmmunition.Value;
            allowAmmunitionTimed = Config.allowAmmunitionTimed.Value;
            removeAmmunition = Config.removeAmmunition.Value;
            enableNetworkingAmmunition = Config.enableNetworkingAmmunition.Value;
            allowThrowables = Config.allowThrowables.Value;
            allowThrowablesTimed = Config.allowThrowablesTimed.Value;
            removeThrowables = Config.removeThrowables.Value;
            enableNetworkingThrowables = Config.enableNetworkingThrowables.Value;
            allowExpendables = Config.allowExpendables.Value;
            allowExpendablesTimed = Config.allowExpendablesTimed.Value;
            removeExpendables = Config.removeExpendables.Value;
            enableNetworkingExpendables = Config.enableNetworkingExpendables.Value;
            allowBreakables = Config.allowBreakables.Value;
            allowBreakablesTimed = Config.allowBreakablesTimed.Value;
            allowList = Config.allowList.Value;
            allowListTimed = Config.allowListTimed.Value;
            networkList = Config.networkList.Value;
            removeList = Config.removeList.Value;

            LootRespawnControl.CustomWhitelist = LootRespawnControl.ExtractIds(allowList);
            LootRespawnControl.CustomWhitelistTimed = LootRespawnControl.ExtractIds(allowListTimed);
            LootRespawnControl.CustomBlacklist = LootRespawnControl.ExtractIds(removeList);
            LootRespawnControl.CustomNetworkingList = LootRespawnControl.ExtractIds(networkList);
        }

        public static void DeserializeConfig(string serializedConfig)
        {
            if (string.IsNullOrEmpty(serializedConfig))
            {
                return; // Nothing to deserialize
            }

            string[] configPairs = serializedConfig.Split(';');

            foreach (string pair in configPairs)
            {
                if (string.IsNullOrEmpty(pair)) continue; // Skip empty pairs

                string[] parts = pair.Split('=');
                if (parts.Length != 2) continue; // Invalid format

                string fieldName = parts[0].Trim();
                string valueString = parts[1].Trim();
                RLog.Msg($"Switching {fieldName} to {valueString}");
                switch (fieldName)
                {
                    case "useTimerGlobal":
                        useTimerGlobal = bool.Parse(valueString);
                        break;
                    case "timeInDays":
                        timeInDays = int.Parse(valueString);
                        break;
                    case "allowMelee":
                        allowMelee = bool.Parse(valueString);
                        break;
                    case "allowMeleeTimed":
                        allowMeleeTimed = bool.Parse(valueString);
                        break;
                    case "removeMelee":
                        removeMelee = bool.Parse(valueString);
                        break;
                    case "enableNetworkingMelee": 
                        enableNetworkingMelee = bool.Parse(valueString);
                        break;
                    case "allowRanged":
                        allowRanged = bool.Parse(valueString);
                        break;
                    case "allowRangedTimed":
                        allowRangedTimed = bool.Parse(valueString);
                        break;
                    case "removeRanged":
                        removeRanged = bool.Parse(valueString);
                        break;
                    case "enableNetworkingRanged": 
                        enableNetworkingRanged = bool.Parse(valueString);
                        break;
                    case "allowWeaponMods":
                        allowWeaponMods = bool.Parse(valueString);
                        break;
                    case "allowWeaponModsTimed":
                        allowWeaponModsTimed = bool.Parse(valueString);
                        break;
                    case "removeWeaponMods":
                        removeWeaponMods = bool.Parse(valueString);
                        break;
                    case "enableNetworkingWeaponMods": 
                        enableNetworkingWeaponMods = bool.Parse(valueString);
                        break;
                    case "allowMaterials":
                        allowMaterials = bool.Parse(valueString);
                        break;
                    case "allowMaterialsTimed":
                        allowMaterialsTimed = bool.Parse(valueString);
                        break;
                    case "removeMaterials":
                        removeMaterials = bool.Parse(valueString);
                        break;
                    case "enableNetworkingMaterials": 
                        enableNetworkingMaterials = bool.Parse(valueString);
                        break;
                    case "allowFood":
                        allowFood = bool.Parse(valueString);
                        break;
                    case "allowFoodTimed":
                        allowFoodTimed = bool.Parse(valueString);
                        break;
                    case "removeFood":
                        removeFood = bool.Parse(valueString);
                        break;
                    case "enableNetworkingFood": 
                        enableNetworkingFood = bool.Parse(valueString);
                        break;
                    case "allowMeds":
                        allowMeds = bool.Parse(valueString);
                        break;
                    case "allowMedsTimed":
                        allowMedsTimed = bool.Parse(valueString);
                        break;
                    case "removeMeds":
                        removeMeds = bool.Parse(valueString);
                        break;
                    case "enableNetworkingMeds": 
                        enableNetworkingMeds = bool.Parse(valueString);
                        break;
                    case "allowPlants":
                        allowPlants = bool.Parse(valueString);
                        break;
                    case "allowPlantsTimed":
                        allowPlantsTimed = bool.Parse(valueString);
                        break;
                    case "removePlants":
                        removePlants = bool.Parse(valueString);
                        break;
                    case "enableNetworkingPlants": 
                        enableNetworkingPlants = bool.Parse(valueString);
                        break;
                    case "allowAmmunition":
                        allowAmmunition = bool.Parse(valueString);
                        break;
                    case "allowAmmunitionTimed":
                        allowAmmunitionTimed = bool.Parse(valueString);
                        break;
                    case "removeAmmunition":
                        removeAmmunition = bool.Parse(valueString);
                        break;
                    case "enableNetworkingAmmunition": 
                        enableNetworkingAmmunition = bool.Parse(valueString);
                        break;
                    case "allowThrowables":
                        allowThrowables = bool.Parse(valueString);
                        break;
                    case "allowThrowablesTimed":
                        allowThrowablesTimed = bool.Parse(valueString);
                        break;
                    case "removeThrowables":
                        removeThrowables = bool.Parse(valueString);
                        break;
                    case "enableNetworkingThrowables": 
                        enableNetworkingThrowables = bool.Parse(valueString);
                        break;
                    case "allowExpendables":
                        allowExpendables = bool.Parse(valueString);
                        break;
                    case "allowExpendablesTimed":
                        allowExpendablesTimed = bool.Parse(valueString);
                        break;
                    case "removeExpendables":
                        removeExpendables = bool.Parse(valueString);
                        break;
                    case "enableNetworkingExpendables": 
                        enableNetworkingExpendables = bool.Parse(valueString);
                        break;
                    case "allowBreakables":
                        allowBreakables = bool.Parse(valueString);
                        break;
                    case "allowBreakablesTimed":
                        allowBreakablesTimed = bool.Parse(valueString);
                        break;
                    case "removeBreakables":
                        removeBreakables = bool.Parse(valueString);
                        break;
                    case "allowList":
                        allowList = valueString;
                        LootRespawnControl.CustomWhitelist = LootRespawnControl.ExtractIds(allowList);
                        break;
                    case "allowListTimed":
                        allowListTimed = valueString;
                        LootRespawnControl.CustomWhitelistTimed = LootRespawnControl.ExtractIds(allowListTimed);
                        break;
                    case "removeList":
                        removeList = valueString;
                        LootRespawnControl.CustomBlacklist = LootRespawnControl.ExtractIds(removeList);
                        break;
                    case "networkList":
                        networkList = valueString;
                        LootRespawnControl.CustomNetworkingList = LootRespawnControl.ExtractIds(networkList);
                        break;
                    default:
                        if (Config.ConsoleLogging.Value) { RLog.Msg($"Could not find config entry {fieldName}"); }
                        break;
                }
            }
        }
    }
}
