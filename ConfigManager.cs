using RedLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LootRespawnControl
{
    public static class ConfigManager
    {
        public static bool useTimerGlobal { get; private set; }
        public static int timeInDays { get; private set; }
        public static bool allowMelee { get; private set; }
        public static bool allowMeleeTimed { get; private set; }
        public static bool removeMelee{ get; private set; }
        public static bool enableNetworkingMelee{ get; private set; } 
        public static bool allowRanged{ get; private set; }
        public static bool allowRangedTimed{ get; private set; }
        public static bool removeRanged{ get; private set; }
        public static bool enableNetworkingRanged{ get; private set; }
        public static bool allowWeaponMods{ get; private set; }
        public static bool allowWeaponModsTimed{ get; private set; }
        public static bool removeWeaponMods{ get; private set; }
        public static bool enableNetworkingWeaponMods{ get; private set; }
        public static bool allowMaterials{ get; private set; }
        public static bool allowMaterialsTimed{ get; private set; }
        public static bool removeMaterials{ get; private set; }
        public static bool enableNetworkingMaterials{ get; private set; } 
        public static bool allowFood{ get; private set; }
        public static bool allowFoodTimed{ get; private set; }
        public static bool removeFood{ get; private set; }
        public static bool enableNetworkingFood{ get; private set; } 
        public static bool allowMeds{ get; private set; }
        public static bool allowMedsTimed{ get; private set; }
        public static bool removeMeds{ get; private set; }
        public static bool enableNetworkingMeds{ get; private set; }
        public static bool allowPlants{ get; private set; }
        public static bool allowPlantsTimed{ get; private set; }
        public static bool removePlants{ get; private set; }
        public static bool enableNetworkingPlants{ get; private set; }
        public static bool allowAmmunition{ get; private set; }
        public static bool allowAmmunitionTimed{ get; private set; }
        public static bool removeAmmunition{ get; private set; }
        public static bool enableNetworkingAmmunition{ get; private set; } 
        public static bool allowThrowables{ get; private set; }
        public static bool allowThrowablesTimed{ get; private set; }
        public static bool removeThrowables{ get; private set; }
        public static bool enableNetworkingThrowables{ get; private set; }
        public static bool allowExpendables{ get; private set; }
        public static bool allowExpendablesTimed{ get; private set; }
        public static bool removeExpendables{ get; private set; }
        public static bool enableNetworkingExpendables{ get; private set; } 
        public static bool allowBreakables{ get; private set; }
        public static bool allowBreakablesTimed{ get; private set; }
        public static bool enableNetworkingBreakables { get; private set; }
        public static string allowList { get; private set; } = "";
        public static string allowListTimed{ get; private set; } = "";
        public static string removeList{ get; private set; } = "";
        public static string networkList{ get; private set; } = "";

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

        public static bool IsBreakablesNetworked()
        {
            return enableNetworkingBreakables;
        }

        public static bool IsGlobalTimerEnabled()
        {
            return useTimerGlobal;
        }

        public static void SetLocalConfigValues()
        {
            Dictionary<string, string> configManagerFields = GetConfigManagerFields();
            Dictionary<string, string> configFields = Config.GetLocalConfigFields();

            RLog.Msg($"ConfigManagerFields = {configManagerFields.Count}   configFields = {configFields.Count}");
            foreach (var kvp in configManagerFields)
            {
                if (configFields.ContainsKey(kvp.Key))
                {
                    PropertyInfo configManagerProperty = typeof(ConfigManager).GetProperty(kvp.Key, BindingFlags.Static | BindingFlags.Public);

                    if (configManagerProperty != null)
                    {
                        try
                        {
                            string configValueString = configFields[kvp.Key];
                            object convertedValue = Convert.ChangeType(configValueString, configManagerProperty.PropertyType);
                            configManagerProperty.SetValue(null, convertedValue);
                            RLog.Msg($"Synced {kvp.Key} to {convertedValue}");
                        }
                        catch (Exception ex)
                        {
                            RLog.Msg($"Error syncing {kvp.Key}: {ex.Message}");
                        }
                    }
                    else
                    {
                        RLog.Msg($"Property {kvp.Key} not found in ConfigManager.");
                    }
                }
                else
                {
                    RLog.Msg($"Config does not contain field: {kvp.Key}");
                }
            }

            LootRespawnControl.CustomWhitelist = LootRespawnControl.ExtractIds(allowList);
            LootRespawnControl.CustomWhitelistTimed = LootRespawnControl.ExtractIds(allowListTimed);
            LootRespawnControl.CustomBlacklist = LootRespawnControl.ExtractIds(removeList);
            LootRespawnControl.CustomNetworkingList = LootRespawnControl.ExtractIds(networkList);
        }

        public static Dictionary<string, string> GetConfigManagerFields()
        {
            Dictionary<string, string> configValues = new Dictionary<string, string>();
            StringBuilder sb = new StringBuilder();
            Type configType = typeof(ConfigManager);
            foreach (var property in configType.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                try
                {
                    object value = property.GetValue(null);
                    configValues.Add(property.Name, value.ToString());
                    RLog.Msg($"Found internal value {property.Name} = {value}");
                }
                catch (Exception ex)
                {
                    RLog.Msg($"Error getting property {property.Name}: {ex.Message}");
                }
            }
            return configValues;
        }

        public static void DeserializeConfig(string serializedConfig)
        {
            if (string.IsNullOrEmpty(serializedConfig))
            {
                return; // Nothing to deserialize
            }

            Dictionary<string, string> recievedValues = new Dictionary<string, string>();
            string[] configPairs = serializedConfig.Split(';');

            foreach (string pair in configPairs)
            {
                if (string.IsNullOrEmpty(pair)) continue; // Skip empty pairs

                string[] parts = pair.Split('=');
                if (parts.Length != 2) continue; // Invalid format

                string fieldName = parts[0].Trim();
                string valueString = parts[1].Trim();
                RLog.Msg($"Switching {fieldName} to {valueString}");
                recievedValues.Add(fieldName, valueString);
            }

            Dictionary<string, string> configManagerFields = GetConfigManagerFields();
            foreach (var kvp in configManagerFields)
            {
                if (recievedValues.ContainsKey(kvp.Key))
                {
                    PropertyInfo configManagerProperty = typeof(ConfigManager).GetProperty(kvp.Key, BindingFlags.Static | BindingFlags.Public);

                    if (configManagerProperty != null)
                    {
                        try
                        {
                            string configValueString = recievedValues[kvp.Key];
                            object convertedValue = Convert.ChangeType(configValueString, configManagerProperty.PropertyType);
                            configManagerProperty.SetValue(null, convertedValue);
                            RLog.Msg($"Synced {kvp.Key} to {convertedValue}");
                        }
                        catch (Exception ex)
                        {
                            RLog.Msg($"Error syncing {kvp.Key}: {ex.Message}");
                        }
                    }
                    else
                    {
                        RLog.Msg($"Property {kvp.Key} not found in ConfigManager.");
                    }
                }
                else
                {
                    RLog.Msg($"Config does not contain field: {kvp.Key}");
                }
            }
        }
    }
}
