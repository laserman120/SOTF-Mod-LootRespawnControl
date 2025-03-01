using RedLoader;
using System.Reflection;
using System.Text;
using static RedLoader.RLog;

namespace LootRespawnControl;

public static class Config
{
    public static ConfigCategory Category { get; private set; }

    public static ConfigEntry<bool> enableMultiplayer { get; private set; }
    public static ConfigEntry<bool> useTimerGlobal { get; private set; }
    public static ConfigEntry<int> timeInDays { get; private set; }
    public static ConfigEntry<bool> allowMelee { get; private set; }
    public static ConfigEntry<bool> allowMeleeTimed { get; private set; }
    public static ConfigEntry<bool> removeMelee { get; private set; }
    public static ConfigEntry<bool> enableNetworkingMelee { get; private set; } // New

    public static ConfigEntry<bool> allowRanged { get; private set; }
    public static ConfigEntry<bool> allowRangedTimed { get; private set; }
    public static ConfigEntry<bool> removeRanged { get; private set; }
    public static ConfigEntry<bool> enableNetworkingRanged { get; private set; } // New

    public static ConfigEntry<bool> allowWeaponMods { get; private set; }
    public static ConfigEntry<bool> allowWeaponModsTimed { get; private set; }
    public static ConfigEntry<bool> removeWeaponMods { get; private set; }
    public static ConfigEntry<bool> enableNetworkingWeaponMods { get; private set; } // New

    public static ConfigEntry<bool> allowMaterials { get; private set; }
    public static ConfigEntry<bool> allowMaterialsTimed { get; private set; }
    public static ConfigEntry<bool> removeMaterials { get; private set; }
    public static ConfigEntry<bool> enableNetworkingMaterials { get; private set; } // New

    public static ConfigEntry<bool> allowFood { get; private set; }
    public static ConfigEntry<bool> allowFoodTimed { get; private set; }
    public static ConfigEntry<bool> removeFood { get; private set; }
    public static ConfigEntry<bool> enableNetworkingFood { get; private set; } // New

    public static ConfigEntry<bool> allowMeds { get; private set; }
    public static ConfigEntry<bool> allowMedsTimed { get; private set; }
    public static ConfigEntry<bool> removeMeds { get; private set; }
    public static ConfigEntry<bool> enableNetworkingMeds { get; private set; } // New

    public static ConfigEntry<bool> allowPlants { get; private set; }
    public static ConfigEntry<bool> allowPlantsTimed { get; private set; }
    public static ConfigEntry<bool> removePlants { get; private set; }
    public static ConfigEntry<bool> enableNetworkingPlants { get; private set; } // New

    public static ConfigEntry<bool> allowAmmunition { get; private set; }
    public static ConfigEntry<bool> allowAmmunitionTimed { get; private set; }
    public static ConfigEntry<bool> removeAmmunition { get; private set; }
    public static ConfigEntry<bool> enableNetworkingAmmunition { get; private set; } // New

    public static ConfigEntry<bool> allowThrowables { get; private set; }
    public static ConfigEntry<bool> allowThrowablesTimed { get; private set; }
    public static ConfigEntry<bool> removeThrowables { get; private set; }
    public static ConfigEntry<bool> enableNetworkingThrowables { get; private set; } // New

    public static ConfigEntry<bool> allowExpendables { get; private set; }
    public static ConfigEntry<bool> allowExpendablesTimed { get; private set; }
    public static ConfigEntry<bool> removeExpendables { get; private set; }
    public static ConfigEntry<bool> enableNetworkingExpendables { get; private set; } // New

    public static ConfigEntry<bool> allowBreakables { get; private set; }
    public static ConfigEntry<bool> allowBreakablesTimed { get; private set; }
    public static ConfigEntry<bool> enableNetworkingBreakables { get; private set; }

    public static ConfigEntry<string> allowList { get; private set; }
    public static ConfigEntry<string> allowListTimed { get; private set; }
    public static ConfigEntry<string> removeList { get; private set; }

    public static ConfigEntry<string> networkList { get; private set; }


    public static ConfigEntry<bool> ConsoleLogging { get; private set; }

    public static void Init()
    {
        Category = ConfigSystem.CreateFileCategory("Multiplayer Settings", "Multiplayer Settings", "LootRespawnControl.cfg");

        enableMultiplayer = Category.CreateEntry(
        "enableMultiplayer",
        false,
        "Should pickups be synced across players?",
        "Enabling this will remove any loot a player picks up for other players as well");

        Category = ConfigSystem.CreateFileCategory("Timer Settings", "Timer Settings", "LootRespawnControl.cfg");

        useTimerGlobal = Category.CreateEntry(
        "UseTimerGlobal",
        false,
        "Should all loot be allowed to respawn after X days?",
        "Enabling this will enable timed respawn for all categories");

        timeInDays = Category.CreateEntry(
        "TimeInDays",
        7,
        "Time in Days",
        "How many ingame days need to pass for loot to once again respawn");
        timeInDays.SetRange(1, 50); // Days of ingame time for respawn

        Category = ConfigSystem.CreateFileCategory("Weapons", "Weapons", "LootRespawnControl.cfg");

        //melee
        allowMelee = Category.CreateEntry(
            "AllowMelee",
            false,
            "Allow Melee Weapons",
            "Allows melee weapons to respawn (Modern Axe, Fire Axe, Stun Baton...");

        allowMeleeTimed = Category.CreateEntry(
            "AllowMeleeTimed",
            false,
            "Allow Melee Weapons to respawn with the Timer",
            "Allows melee weapons to respawn (Modern Axe, Fire Axe, Stun Baton...)");

        removeMelee = Category.CreateEntry(
            "RemoveMelee",
            false,
            "Block Melee Weapons",
            "Will prevent all melee weapons from spawning (Modern Axe, Fire Axe, Stun Baton...)");

        enableNetworkingMelee = Category.CreateEntry( // New
            "NetworkMelee",
            false,
            "Sync Melee Weapons",
            "Will synchronize pickup of melee weapons in multiplayer (Modern Axe, Fire Axe, Stun Baton...");


        //ranged
        allowRanged = Category.CreateEntry(
            "AllowRanged",
            false,
            "Allow Ranged Weapons",
            "Allows ranged weapons to respawn (Pistol, Shotgun, Crossbow...)");

        allowRangedTimed = Category.CreateEntry(
            "AllowRangedTimed",
            false,
            "Allow Ranged Weapons to respawn with the Timer",
            "Allows ranged weapons to respawn (Pistol, Shotgun, Crossbow...)");

        removeRanged = Category.CreateEntry(
            "RemoveRanged",
            false,
            "Block Ranged Weapons",
            "Will prevent all ranged weapons from spawning (Pistol, Shotgun, Crossbow...)");

        enableNetworkingRanged = Category.CreateEntry( // New
            "NetworkRanged",
            false,
            "Sync Ranged Weapons",
            "Will synchronize pickup of ranged weapons in multiplayer (Pistol, Shotgun, Crossbow...)");


        //mods
        allowWeaponMods = Category.CreateEntry(
            "AllowWeaponMods",
            false,
            "Allow Weapon Mods",
            "Allows weapon mods to respawn (Pistol Rail, Gun Flashlight...)");

        allowWeaponModsTimed = Category.CreateEntry(
            "AllowWeaponModsTimed",
            false,
            "Allow Weapon Mods to respawn with the Timer",
            "Allows weapon mods to respawn (Pistol Rail, Gun Flashlight...)");

        removeWeaponMods = Category.CreateEntry(
            "RemoveWeaponMods",
            false,
            "Block Weapon Mods",
            "Will prevent all weapon mods from spawning (Pistol Rail, Gun Flashlight...)");

        enableNetworkingWeaponMods = Category.CreateEntry( // New
            "NetworkWeaponMods",
            false,
            "Sync Weapon Mods",
            "Will synchronize pickup of weapon mods in multiplayer (Pistol Rail, Gun Flashlight...)");


        //ammo
        allowAmmunition = Category.CreateEntry(
            "AllowAmmunition",
            false,
            "Allow Ammunition",
            "Allows ammunition to respawn (Pistol ammo, Arrows, Bolts...)");

        allowAmmunitionTimed = Category.CreateEntry(
            "AllowAmmunitionTimed",
            false,
            "Allow Ammunition to respawn with the Timer",
            "Allows ammunition to respawn (Pistol ammo, Arrows, Bolts...)");

        removeAmmunition = Category.CreateEntry(
            "RemoveAmmunition",
            false,
            "Block Ammunition",
            "Will prevent all ammunition from spawning (Pistol ammo, Arrows, Bolts...)");

        enableNetworkingAmmunition = Category.CreateEntry( // New
            "NetworkAmmunition",
            false,
            "Sync Ammunition",
            "Will synchronize pickup of ammunition in multiplayer (Pistol ammo, Arrows, Bolts...)");


        //throwables
        allowThrowables = Category.CreateEntry(
            "AllowThrowables",
            false,
            "Allow Throwables",
            "Allows throwables to respawn (Grenades, Sticky Bombs, Golf Balls...)");

        allowThrowablesTimed = Category.CreateEntry(
            "AllowThrowablesTimed",
            false,
            "Allow Throwables to respawn with the Timer",
            "Allows throwables to respawn (Grenades, Sticky Bombs, Golf Balls...)");

        removeThrowables = Category.CreateEntry(
            "RemoveThrowables",
            false,
            "Block Throwables",
            "Will prevent all throwables from spawning (Grenades, Sticky Bombs, Golf Balls...)");

        enableNetworkingThrowables = Category.CreateEntry( // New
            "NetworkThrowables",
            false,
            "Sync Throwables",
            "Will synchronize pickup of throwables in multiplayer (Grenades, Sticky Bombs, Golf Balls...)");


        Category = ConfigSystem.CreateFileCategory("Pickups", "Pickups", "LootRespawnControl.cfg");

        //materials
        allowMaterials = Category.CreateEntry(
            "AllowMaterials",
            false,
            "Allow Materials",
            "Allows crafting materials to respawn (Duct Tape, Rope, Coins...)");

        allowMaterialsTimed = Category.CreateEntry(
            "AllowMaterialsTimed",
            false,
            "Allow Materials to respawn with the Timer",
            "Allows crafting materials to respawn (Duct Tape, Rope, Coins...)");

        removeMaterials = Category.CreateEntry(
            "RemoveMaterials",
            false,
            "Block Materials",
            "Will prevent all crafting materials from spawning (Duct Tape, Rope, Coins...)");

        enableNetworkingMaterials = Category.CreateEntry( // New
            "NetworkMaterials",
            false,
            "Sync Materials",
            "Will synchronize pickup of crafting materials in multiplayer (Duct Tape, Rope, Coins...)");


        //food
        allowFood = Category.CreateEntry(
            "AllowFood",
            false,
            "Allow Food",
            "This allows food to respawn (Cat Food, Cereal, MRE packs...)");

        allowFoodTimed = Category.CreateEntry(
            "AllowFoodTimed",
            false,
            "Allow Food to respawn with the Timer",
            "This allows food to respawn (Cat Food, Cereal, MRE packs...)");

        removeFood = Category.CreateEntry(
            "RemoveFood",
            false,
            "Block Food",
            "Will prevent all food from spawning (Cat Food, Cereal, MRE packs...)");

        enableNetworkingFood = Category.CreateEntry( // New
            "NetworkFood",
            false,
            "Sync Food",
            "Will synchronize pickup of food in multiplayer (Cat Food, Cereal, MRE packs...)");


        //meds
        allowMeds = Category.CreateEntry(
            "AllowMeds",
            false,
            "Allow Medicine & Energy",
            "Allows medicine and energy drinks to respawn");

        allowMedsTimed = Category.CreateEntry(
            "AllowMedsTimed",
            false,
            "Allow Medicine & Energy to respawn with the Timer",
            "Allows medicine and energy drinks to respawn");

        removeMeds = Category.CreateEntry(
            "RemoveMeds",
            false,
            "Block Medicine & Energy",
            "Will prevent all medicine and energy drinks from spawning");

        enableNetworkingMeds = Category.CreateEntry( // New
            "NetworkMeds",
            false,
            "Sync Meds & Energy",
            "Will synchronize pickup of medicine and energy drinks in multiplayer");


        //plants
        allowPlants = Category.CreateEntry(
            "AllowPlants",
            false,
            "Allow plants to spawn",
            "Allows plants to respawn(Aloe Vera, Mushrooms, Chicory...)");

        allowPlantsTimed = Category.CreateEntry(
            "AllowPlantsTimed",
            false,
            "Allow plants to spawn with the Timer",
            "Allows plants to respawn(Aloe Vera, Mushrooms, Chicory...)");

        removePlants = Category.CreateEntry(
            "RemovePlants",
            false,
            "Block Plants",
            "Will prevent all plants from spawning (Aloe Vera, Mushrooms, Chicory...)");

        enableNetworkingPlants = Category.CreateEntry( // New
            "NetworkPlants",
            false,
            "Sync Plants",
            "Will synchronize pickup of plants in multiplayer (Aloe Vera, Mushrooms, Chicory...)");


        Category = ConfigSystem.CreateFileCategory("Miscellaneous", "Miscellaneous", "LootRespawnControl.cfg");

        //expendables
        allowExpendables = Category.CreateEntry(
            "AllowExpendables",
            false,
            "Allow Expendables",
            "Allows expendables to respawn (Air Canisters, Printer Ink, Hide Bags)");

        allowExpendablesTimed = Category.CreateEntry(
            "AllowExpendablesTimed",
            false,
            "Allow Expendables to respawn with the Timer",
            "Allows expendables to respawn (Air Canisters, Printer Ink, Hide Bags)");

        removeExpendables = Category.CreateEntry(
            "RemoveExpendables",
            false,
            "Block Expendables",
            "Will prevent all expendables from spawning (Air Canisters, Printer Ink, Hide Bags)");

        enableNetworkingExpendables = Category.CreateEntry( // New
            "NetworkExpendables",
            false,
            "Sync Expendables",
            "Will synchronize pickup of expendables in multiplayer (Air Canisters, Printer Ink, Hide Bags)");

        //breakables
        allowBreakables = Category.CreateEntry(
            "AllowBreakables",
            false,
            "Allow Breakables",
            "Allows breakable objects to respawn (Laptops, Gore Vases...)");

        allowBreakablesTimed = Category.CreateEntry(
            "AllowBreakablesTimed",
            false,
            "Allow Breakables to respawn with the Timer",
            "Allows breakable objects to respawn (Laptops, Gore Vases...)");

        enableNetworkingBreakables = Category.CreateEntry( // New
            "NetworkBreakables",
            false,
            "Sync Breakables",
            "Will synchronize destruction of breakables in multiplayer (Laptops, Gore Vases...)");

        Category = ConfigSystem.CreateFileCategory("Custom Options", "Custom Options", "LootRespawnControl.cfg");

        allowList = Category.CreateEntry(
            "Whitelist",
            "",
            "Custom Whitelist",
            "Allows you to setup a custom whitelist, items will always respawn. Seperate each item id with a ; for example: 437; 634",
            false);

        allowListTimed = Category.CreateEntry(
            "WhitelistTimed",
            "",
            "Custom Whitelist with the Timer",
            "Allows you to setup a custom whitelist, items will respawn once enough time has passed. Seperate each item id with a ; for example: 437; 634",
            false);

        removeList = Category.CreateEntry(
            "Blacklist",
            "",
            "Custom Blacklist",
            "Allows you to setup a custom blacklist, item ids added here will never spawn. Seperate each item id with a ; for example: 437; 634",
            false);

        networkList = Category.CreateEntry(
            "networkList",
            "",
            "Custom Networking",
            "Allows you to setup a custom list of networked pickups, picking them up will also remove them for other players. Seperate each item id with a ; for example: 437; 634",
            false);

        Category = ConfigSystem.CreateFileCategory("Debugging", "Debugging", "LootRespawnControl.cfg");

        ConsoleLogging = Category.CreateEntry(
            "ConsoleLogging",
            false,
            "Enable Logging (for debugging only!)",
            "Will log nearly everything the mod does into the log files");
    }

    public static void OnSettingsUiClosed()
    {
        LootRespawnControl.CustomWhitelist = LootRespawnControl.ExtractIds(Config.allowList.Value);
        LootRespawnControl.CustomWhitelistTimed = LootRespawnControl.ExtractIds(Config.allowListTimed.Value);
        LootRespawnControl.CustomBlacklist = LootRespawnControl.ExtractIds(Config.removeList.Value);
        LootRespawnControl.CustomNetworkingList = LootRespawnControl.ExtractIds(Config.networkList.Value);
    }

    public static string Serialize()
    {
        StringBuilder sb = new StringBuilder();
        Type configType = typeof(Config);

        foreach (var property in configType.GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ConfigEntry<>))
            {
                object configEntry = property.GetValue(null);
                object value = configEntry.GetType().GetProperty("Value").GetValue(configEntry);

                sb.Append(property.Name).Append("=").Append(value).Append(";");
            }
        }
        return sb.ToString();
    }

    public static Dictionary<string, string> GetLocalConfigFields()
    {
        Dictionary<string, string> configValues = new Dictionary<string, string>();
        Type configType = typeof(Config);

        foreach (var property in configType.GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ConfigEntry<>))
            {
                object configEntry = property.GetValue(null);
                string configEntryName = property.Name;
                object value = configEntry.GetType().GetProperty("Value").GetValue(configEntry);
                configValues.Add(configEntryName, value.ToString());
            }
        }
        return configValues;
    }
}